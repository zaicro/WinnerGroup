Imports System.Linq
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Threading
Imports System.Threading.Tasks
Imports Microsoft.IdentityModel.Tokens
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain
Imports TableGamesAuth.Security

Module Program
    Private passed As Integer
    Private Sub Check(condition As Boolean, label As String)
        If Not condition Then Throw New Exception("FAIL: " & label)
        passed += 1
        Console.WriteLine("PASS: " & label)
    End Sub
    Private Async Function ExpectStatus(status As Integer, action As Func(Of Task)) As Task
        Try
            Await action()
        Catch ex As AppException When ex.Status = status
            Check(True, "HTTP " & status)
            Return
        End Try
        Throw New Exception("Expected status " & status)
    End Function
    Public Sub Main()
        Try
            Run().GetAwaiter().GetResult()
            Console.WriteLine(passed & " checks passed.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.ToString())
            Environment.ExitCode = 1
        End Try
    End Sub
    Private Async Function Run() As Task
        Dim sink As New log4net.Appender.MemoryAppender()
        log4net.Config.BasicConfigurator.Configure(sink)
        Dim logger As New TableGamesAuth.Logging.Log4NetLogger()
        logger.Info("test_info")
        logger.Warn("test_warning")
        logger.Error("test_error", New InvalidOperationException("SECRET-PASSWORD-DO-NOT-LOG"))
        Dim events = sink.GetEvents()
        Check(events.Length = 3, "log4net captures Info, Warn, Error")
        Check(Not String.Join(" ", events.Select(Function(e) e.RenderedMessage)).Contains("SECRET-PASSWORD"), "Provider error messages are redacted")
        Check(events(2).RenderedMessage.Contains("InvalidOperationException"), "Logger preserves exception type")
        Dim ct = CancellationToken.None
        Dim hasher As New Pbkdf2PasswordHasher()
        Dim password = "Long-Passphrase-2026!"
        Dim h = hasher.Hash(password)
        Check(h <> hasher.Hash(password), "Random salt")
        Check(hasher.Verify(password, h), "Correct password")
        Check(Not hasher.Verify("wrong", h), "Wrong password")
        Check(Not hasher.Verify(password, "pbkdf2-sha256$999999999$bad$bad"), "Untrusted work factor rejected")
        Check(Not hasher.IsSupportedHash("sha256$abcdef"), "Legacy/unrecognized imports rejected")
        Await ExpectStatus(400, Function() Task.Run(Sub() hasher.Hash("short")))
        Dim key = Convert.ToBase64String(Enumerable.Range(1, 32).Select(Function(i) CByte(i)).ToArray())
        Dim tokens As New JwtTokenService(key, "test-issuer", "test-audience")
        Dim db As New MemoryUnitOfWork()
        Dim role As New Role With {.Name = "Operator", .NormalizedName = "OPERATOR"}
        Dim admin As New Role With {.Name = "SystemAdmin", .NormalizedName = "SYSTEMADMIN"}
        db.RoleStore.Add(role) : db.RoleStore.Add(admin)
        Dim app As New ClientApp With {.Name = "Roulette", .Code = "ROULETTE"}
        db.AppStore.Add(app)
        Dim user As New User With {.UserName = "andres", .NormalizedUserName = "ANDRES", .PasswordHash = h, .RoleId = role.Id}
        db.UserStore.Add(user)
        Dim grant As New UserApp With {.UserId = user.Id, .AppId = app.Id}
        db.GrantStore.Add(grant)
        Dim auth As New AuthService(db, hasher, tokens, h)
        Dim input As New LoginInput With {.UserName = "ANDRES", .Password = password, .AppId = app.Id}
        Dim result = Await auth.Login(input, app.Id, ct)
        Check(db.LastTransaction.Committed, "Successful login commits")
        Dim principal = tokens.Validate(result.AccessToken)
        Check(principal.FindFirst("app_id").Value = app.Id.ToString(), "JWT bound to application")
        Check((Await auth.ValidateSession(principal, app.Id, ct)).IsInRole("Operator"), "Current role authorized")
        user.RoleId = admin.Id
        Check((Await auth.ValidateSession(principal, app.Id, ct)).IsInRole("SystemAdmin"), "Role changes use database state")
        user.IsActive = False
        Check(Await auth.ValidateSession(principal, app.Id, ct) Is Nothing, "Disabled user revokes JWT")
        user.IsActive = True
        Dim stamp = user.SecurityStamp
        user.SecurityStamp = Guid.NewGuid()
        Check(Await auth.ValidateSession(principal, app.Id, ct) Is Nothing, "Password security stamp revokes JWT")
        user.SecurityStamp = stamp
        db.GrantStore.Remove(grant)
        Check(Await auth.ValidateSession(principal, app.Id, ct) Is Nothing, "Grant deletion revokes JWT")
        db.GrantStore.Add(grant)
        app.IsActive = False
        Check(Await auth.ValidateSession(principal, app.Id, ct) Is Nothing, "App disable revokes JWT")
        app.IsActive = True
        Check(Await auth.ValidateSession(principal, Guid.NewGuid(), ct) Is Nothing, "Cross-application JWT rejected")
        For Each bad In New String() {result.AccessToken.Substring(0, result.AccessToken.Length - 5) & "AAAAA", "not-a-jwt"}
            Dim rejected As Boolean
            Try
                tokens.Validate(bad)
            Catch ex As Exception When TypeOf ex Is SecurityTokenException OrElse TypeOf ex Is ArgumentException
                rejected = True
            End Try
            Check(rejected, "Malformed/tampered token rejected")
        Next
        For Each settings In New String() {"wrong-issuer", "wrong-audience", "expired", "HS512"}
            Dim issuer = If(settings = "wrong-issuer", "bad", "test-issuer")
            Dim audience = If(settings = "wrong-audience", "bad", "test-audience")
            Dim expires = If(settings = "expired", DateTime.UtcNow.AddMinutes(-2), DateTime.UtcNow.AddMinutes(15))
            Dim signingKey = If(settings = "HS512", New Byte(63) {}, Convert.FromBase64String(key))
            Dim credentials As New SigningCredentials(New SymmetricSecurityKey(signingKey), If(settings = "HS512", SecurityAlgorithms.HmacSha512, SecurityAlgorithms.HmacSha256))
            Dim jwt As New JwtSecurityToken(issuer, audience, Nothing, DateTime.UtcNow.AddHours(-1), expires, credentials)
            Dim rejected As Boolean
            Try
                tokens.Validate(New JwtSecurityTokenHandler().WriteToken(jwt))
            Catch ex As SecurityTokenException
                rejected = True
            End Try
            Check(rejected, "Reject " & settings)
        Next
        input.Password = "wrong-password"
        For i = 1 To 5
            Await ExpectStatus(401, Async Function() As Task
                                        Await auth.Login(input, app.Id, ct)
                                    End Function)
            Check(db.LastTransaction.Committed, "Failed login count committed")
        Next
        Check(user.FailedLoginCount = 5 AndAlso user.LockoutUntilUtc.GetValueOrDefault() > DateTime.UtcNow, "Five failures lock account")
        input.Password = password
        Await ExpectStatus(401, Async Function() As Task
                                    Await auth.Login(input, app.Id, ct)
                                End Function)
        user.LockoutUntilUtc = DateTime.UtcNow.AddSeconds(-1)
        Await auth.Login(input, app.Id, ct)
        Check(user.FailedLoginCount = 0 AndAlso Not user.LockoutUntilUtc.HasValue, "Successful login resets expired lock")
        Await ExpectStatus(401, Async Function() As Task
                                    Await auth.Login(input, Guid.NewGuid(), ct)
                                End Function)
        Dim crud As New CrudService(db, hasher)
        Await ExpectStatus(409, Async Function() As Task
                                    Await crud.SaveUser(user.Id, New UserInput With {.UserName = "andres", .RoleId = role.Id, .Version = Guid.NewGuid()}, ct)
                                End Function)
        Check(db.LastTransaction.RolledBack, "Concurrency failure rolls transaction back")
        Await ExpectStatus(400, Async Function() As Task
                                    Await crud.SaveUser(Nothing, New UserInput With {.UserName = "new-user", .RoleId = role.Id, .Password = password, .PasswordHash = h}, ct)
                                End Function)
        Await ExpectStatus(409, Async Function() As Task
                                    Await crud.Delete("roles", role.Id, Guid.Empty, ct)
                                End Function)
        Dim view = Await crud.GetUser(user.Id, ct)
        Check(view.GetType().GetProperty("PasswordHash") Is Nothing, "Public user DTO excludes hash")
        db.SaveFails = True
        Await ExpectStatus(409, Async Function() As Task
                                    Await crud.SaveRole(Nothing, New RoleInput With {.Name = "NewRole"}, ct)
                                End Function)
        Check(db.LastTransaction.RolledBack AndAlso Not db.LastTransaction.Committed, "Save exception rolls back transaction")
        ' These fakes verify transaction orchestration; only database integration tests prove durable rollback.
    End Function
End Module
