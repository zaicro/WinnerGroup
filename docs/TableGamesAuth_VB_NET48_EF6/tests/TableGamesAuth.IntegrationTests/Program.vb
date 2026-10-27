Imports System.Data.Entity
Imports System.Data.Entity.Migrations
Imports System.Data.SqlClient
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Microsoft.Extensions.DependencyInjection
Imports Newtonsoft.Json
Imports Newtonsoft.Json.Linq
Imports Npgsql
Imports TableGamesAuth.Api
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain
Imports TableGamesAuth.DataAccess.Ef6
Imports TableGamesAuth.DataAccess.Sql
Imports TableGamesAuth.DataAccess.Postgres
Imports TableGamesAuth.Security

Module Program
    Private passed As Integer
    Private ReadOnly ct As CancellationToken = CancellationToken.None
    Public Sub Main()
        Try
            Run().GetAwaiter().GetResult()
            Console.WriteLine(passed & " integration checks passed.")
        Catch ex As Exception
            Console.Error.WriteLine(ex.GetType().Name & ": " & ex.Message)
            Environment.ExitCode = 1
        End Try
    End Sub
    Private Sub Check(condition As Boolean, label As String)
        If Not condition Then Throw New Exception("FAIL: " & label)
        passed += 1
        Console.WriteLine("PASS: " & label)
    End Sub
    Private Async Function Status(expected As Integer, action As Func(Of Task)) As Task
        Try
            Await action()
        Catch ex As AppException When ex.Status = expected
            Check(True, "Expected database conflict " & expected)
            Return
        End Try
        Throw New Exception("Expected status " & expected)
    End Function
    Private Function Db(provider As String, connection As String) As AuthDbContext
        If provider = "SqlServer" Then Return New SqlAuthDbContext(connection)
        Return New PostgresAuthDbContext(connection)
    End Function
    Private Function Uow(provider As String, connection As String) As IUnitOfWork
        Return New EfUnitOfWork(Db(provider, connection))
    End Function
    Private Async Function Run() As Task
        DbConfiguration.SetConfiguration(New ProvidersConfiguration())
        Dim configured As Integer
        For Each provider In New String() {"SqlServer", "Postgres"}
            Dim variable = If(provider = "SqlServer", "TG_INTEGRATION_SQL_CONNECTION", "TG_INTEGRATION_POSTGRES_CONNECTION")
            Dim connection = Environment.GetEnvironmentVariable(variable)
            If String.IsNullOrWhiteSpace(connection) Then
                Console.WriteLine("SKIP " & provider & ": " & variable & " no configurada.")
                Continue For
            End If
            Dim databaseName = If(provider = "SqlServer", New SqlConnectionStringBuilder(connection).InitialCatalog, New NpgsqlConnectionStringBuilder(connection).Database)
            If String.IsNullOrEmpty(databaseName) OrElse Not databaseName.EndsWith("_tests", StringComparison.OrdinalIgnoreCase) Then Throw New InvalidOperationException("La base de integración debe terminar en _tests.")
            configured += 1
            Await RunProvider(provider, connection)
        Next
        If configured = 0 Then Throw New InvalidOperationException("No se ejecutó integración: configure al menos una conexión de pruebas.")
    End Function
    Private Async Function RunProvider(provider As String, connection As String) As Task
        Console.WriteLine("Provider: " & provider)
        Dim oldConnection = Environment.GetEnvironmentVariable("TG_DB_CONNECTION")
        Environment.SetEnvironmentVariable("TG_DB_CONNECTION", connection)
        Try
            Dim configuration As DbMigrationsConfiguration
            If provider = "SqlServer" Then
                configuration = New TableGamesAuth.DataAccess.Sql.Migrations.Configuration()
            Else
                configuration = New TableGamesAuth.DataAccess.Postgres.Migrations.Configuration()
            End If
            Dim migrator As New DbMigrator(configuration)
            migrator.Update()
            Check(migrator.GetPendingMigrations().Count() = 0, "Initial migration applied")
            Using context = Db(provider, connection)
                Dim before = Await context.Roles.CountAsync(ct)
                Dim appsBefore = Await context.Apps.CountAsync(ct)
                migrator.Update() ' Seed runs again: no duplicated records.
                Check(Await context.Roles.CountAsync(ct) = before AndAlso Await context.Apps.CountAsync(ct) = appsBefore, "Seed is idempotent")
                Check(Await context.Roles.AnyAsync(Function(r) r.Id = SeedData.AdminRoleId, ct) AndAlso Await context.Apps.AnyAsync(Function(a) a.Id = SeedData.BaccaratAppId, ct), "Seed IDs exist")
            End Using
            Dim suffix = Guid.NewGuid().ToString("N")
            Dim role As New Role With {.Name = "test-" & suffix, .NormalizedName = "TEST-" & suffix.ToUpperInvariant()}
            Dim app As New ClientApp With {.Name = "test-" & suffix, .Code = "TEST-" & suffix.ToUpperInvariant()}
            Dim hasher As New Pbkdf2PasswordHasher()
            Dim admin As New User With {.UserName = "admin-" & suffix, .NormalizedUserName = "ADMIN-" & suffix.ToUpperInvariant(), .PasswordHash = hasher.Hash("Integration-Passphrase-2026!"), .RoleId = SeedData.AdminRoleId}
            Dim user As New User With {.UserName = "user-" & suffix, .NormalizedUserName = "USER-" & suffix.ToUpperInvariant(), .PasswordHash = admin.PasswordHash, .RoleId = role.Id}
            Dim grant As New UserApp With {.UserId = user.Id, .AppId = app.Id}
            Using work = Uow(provider, connection), tx = work.BeginTransaction()
                work.Roles.Add(role) : work.Apps.Add(app) : work.Users.Add(admin) : work.Users.Add(user)
                work.UserApps.Add(grant)
                work.UserApps.Add(New UserApp With {.UserId = admin.Id, .AppId = app.Id})
                Await work.SaveAsync(ct) : tx.Commit()
            End Using
            ' Unique constraint exercised directly, bypassing application prechecks.
            Using work = Uow(provider, connection)
                Await Status(409, Async Function() As Task
                                      Using tx = work.BeginTransaction()
                                          work.UserApps.Add(New UserApp With {.UserId = user.Id, .AppId = app.Id})
                                          Await work.SaveAsync(ct)
                                          tx.Commit()
                                      End Using
                                  End Function)
            End Using
            Using work = Uow(provider, connection)
                Await Status(409, Async Function() As Task
                                      Using tx = work.BeginTransaction()
                                          work.Roles.Remove(Await work.Roles.GetAsync(role.Id, ct))
                                          Await work.SaveAsync(ct)
                                          tx.Commit()
                                      End Using
                                  End Function)
            End Using
            ' Save succeeds once, then a later Save fails. The first Save MUST also roll back.
            Dim rollbackRole As New Role With {.Name = "rollback-" & suffix, .NormalizedName = "ROLLBACK-" & suffix.ToUpperInvariant()}
            Using work = Uow(provider, connection)
                Await Status(409, Async Function() As Task
                                      Using tx = work.BeginTransaction()
                                          work.Roles.Add(rollbackRole)
                                          Await work.SaveAsync(ct)
                                          work.Apps.Add(New ClientApp With {.Name = "duplicate", .Code = app.Code})
                                          Await work.SaveAsync(ct)
                                          tx.Commit()
                                      End Using
                                  End Function)
            End Using
            Using work = Uow(provider, connection)
                Check(Await work.Roles.GetAsync(rollbackRole.Id, ct) Is Nothing, "Real rollback reverses previously successful SaveChanges")
            End Using
            ' Two contexts load the same version, then update sequentially.
            Using first = Uow(provider, connection), second = Uow(provider, connection)
                Dim a = Await first.Roles.GetAsync(role.Id, ct)
                Dim b = Await second.Roles.GetAsync(role.Id, ct)
                Using tx = first.BeginTransaction()
                    a.Name &= "-first"
                    Await first.SaveAsync(ct) : tx.Commit()
                End Using
                Await Status(409, Async Function() As Task
                                      Using tx = second.BeginTransaction()
                                          b.Name &= "-second"
                                          Await second.SaveAsync(ct)
                                          tx.Commit()
                                      End Using
                                  End Function)
            End Using
            Await HttpChecks(provider, connection, app, admin, user)
            Console.WriteLine("Datos de prueba identificados por " & suffix & "; no se elimina la base automáticamente.")
        Finally
            Environment.SetEnvironmentVariable("TG_DB_CONNECTION", oldConnection)
        End Try
    End Function
    Private Async Function HttpChecks(provider As String, connection As String, app As ClientApp, admin As User, user As User) As Task
        Dim random(31) As Byte
        Using rng = RandomNumberGenerator.Create()
            rng.GetBytes(random)
        End Using
        Dim rawKey = Convert.ToBase64String(random)
        Dim hash As String
        Using sha = SHA256.Create()
            hash = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(rawKey))).Replace("-", "")
        End Using
        Dim settings As New Settings With {.Provider = provider, .ConnectionString = connection, .JwtKey = rawKey, .ApiKeys = New List(Of ApiKeyEntry) From {New ApiKeyEntry With {.AppId = app.Id, .Sha256 = hash}}}
        Using services = Composition.Build(settings), config = ApiConfiguration.Create(services, settings), server As New HttpServer(config), client As New HttpClient(server)
            client.BaseAddress = New Uri("https://localhost/")
            Check(CInt((Await client.GetAsync("api/v1/users")).StatusCode) = 401, "HTTP: no API key")
            client.DefaultRequestHeaders.Add("X-Api-Key", rawKey)
            Check(CInt((Await client.GetAsync("api/v1/users")).StatusCode) = 401, "HTTP: API key without JWT")
            Dim body = New With {.userName = admin.UserName, .password = "Integration-Passphrase-2026!", .appId = app.Id}
            Dim login = Await SendJson(client, HttpMethod.Post, "api/v1/auth/login", body)
            Check(login.StatusCode = HttpStatusCode.OK, "HTTP: administrator login")
            Dim token = JObject.Parse(Await login.Content.ReadAsStringAsync())("accessToken").Value(Of String)()
            client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", token)
            Check((Await client.GetAsync("api/v1/auth/me")).StatusCode = HttpStatusCode.OK, "HTTP: API key + JWT")
            Dim users = Await client.GetAsync("api/v1/users/" & user.Id.ToString())
            Dim json = Await users.Content.ReadAsStringAsync()
            Check(users.StatusCode = HttpStatusCode.OK AndAlso Not json.Contains("passwordHash") AndAlso Not json.Contains("securityStamp"), "HTTP: sensitive fields excluded")
            Dim name = "api-" & Guid.NewGuid().ToString("N")
            Dim created = Await SendJson(client, HttpMethod.Post, "api/v1/roles", New With {.name = name})
            Check(created.StatusCode = HttpStatusCode.Created, "HTTP: create role")
            Dim role = JObject.Parse(Await created.Content.ReadAsStringAsync())
            Dim route = "api/v1/roles/" & role("id").Value(Of String)()
            Dim wrong = Await SendJson(client, HttpMethod.Put, route, New With {.name = name & "x", .version = Guid.NewGuid()})
            Check(CInt(wrong.StatusCode) = 409, "HTTP: stale version rejected")
            Dim updated = Await SendJson(client, HttpMethod.Put, route, New With {.name = name & "x", .version = role("version").Value(Of String)()})
            Check(updated.StatusCode = HttpStatusCode.OK, "HTTP: update role")
            Dim current = JObject.Parse(Await updated.Content.ReadAsStringAsync())
            Check((Await client.DeleteAsync(route & "?version=" & current("version").Value(Of String)())).StatusCode = HttpStatusCode.NoContent, "HTTP: delete role")
            Check((Await client.GetAsync(route)).StatusCode = HttpStatusCode.NotFound, "HTTP: deleted resource absent")
            ' Exercise the other three resources through HTTP, including DTO round trips.
            Dim createdApp = Await SendJson(client, HttpMethod.Post, "api/v1/apps", New With {.name = name, .code = name, .isActive = True})
            Check(createdApp.StatusCode = HttpStatusCode.Created, "HTTP: create app")
            Dim appJson = JObject.Parse(Await createdApp.Content.ReadAsStringAsync())
            Dim appRoute = "api/v1/apps/" & appJson("id").Value(Of String)()
            Dim appUpdate = Await SendJson(client, HttpMethod.Put, appRoute, New With {.name = name & " updated", .code = name, .isActive = True, .version = appJson("version").Value(Of String)()})
            Check(appUpdate.StatusCode = HttpStatusCode.OK, "HTTP: update app")
            appJson = JObject.Parse(Await appUpdate.Content.ReadAsStringAsync())
            Check((Await client.GetAsync(appRoute)).StatusCode = HttpStatusCode.OK, "HTTP: read app")
            Dim createdUser = Await SendJson(client, HttpMethod.Post, "api/v1/users", New With {.userName = name, .password = "Integration-New-Password-2026!", .roleId = SeedData.OperatorRoleId, .isActive = True})
            Check(createdUser.StatusCode = HttpStatusCode.Created, "HTTP: create user and hash password")
            Dim userJson = JObject.Parse(Await createdUser.Content.ReadAsStringAsync())
            Dim userRoute = "api/v1/users/" & userJson("id").Value(Of String)()
            Dim userUpdate = Await SendJson(client, HttpMethod.Put, userRoute, New With {.userName = name & "x", .roleId = SeedData.OperatorRoleId, .isActive = True, .version = userJson("version").Value(Of String)()})
            Check(userUpdate.StatusCode = HttpStatusCode.OK, "HTTP: update user without changing password")
            userJson = JObject.Parse(Await userUpdate.Content.ReadAsStringAsync())
            Dim createdGrant = Await SendJson(client, HttpMethod.Post, "api/v1/user-apps", New With {.userId = userJson("id").Value(Of String)(), .appId = appJson("id").Value(Of String)()})
            Check(createdGrant.StatusCode = HttpStatusCode.Created, "HTTP: create grant")
            Dim grantJson = JObject.Parse(Await createdGrant.Content.ReadAsStringAsync())
            Dim grantRoute = "api/v1/user-apps/" & grantJson("id").Value(Of String)()
            Dim grantRead = Await client.GetAsync(grantRoute)
            Check(grantRead.StatusCode = HttpStatusCode.OK AndAlso Not (Await grantRead.Content.ReadAsStringAsync()).Contains("passwordHash"), "HTTP: grant DTO excludes user navigation secrets")
            Dim grantUpdate = Await SendJson(client, HttpMethod.Put, grantRoute, New With {.userId = userJson("id").Value(Of String)(), .appId = appJson("id").Value(Of String)(), .version = grantJson("version").Value(Of String)()})
            Check(grantUpdate.StatusCode = HttpStatusCode.OK, "HTTP: update grant")
            grantJson = JObject.Parse(Await grantUpdate.Content.ReadAsStringAsync())
            Check(CInt((Await client.DeleteAsync(userRoute & "?version=" & userJson("version").Value(Of String)())).StatusCode) = 409, "HTTP: prevent deleting user with grants")
            Check((Await client.DeleteAsync(grantRoute & "?version=" & grantJson("version").Value(Of String)())).StatusCode = HttpStatusCode.NoContent, "HTTP: delete grant")
            Check((Await client.DeleteAsync(userRoute & "?version=" & userJson("version").Value(Of String)())).StatusCode = HttpStatusCode.NoContent, "HTTP: delete user")
            Check((Await client.DeleteAsync(appRoute & "?version=" & appJson("version").Value(Of String)())).StatusCode = HttpStatusCode.NoContent, "HTTP: delete app")
            Check(CInt((Await client.GetAsync("api/v1/users?take=101")).StatusCode) = 400, "HTTP: pagination bound")
            Dim operatorLogin = Await SendJson(client, HttpMethod.Post, "api/v1/auth/login", New With {.userName = user.UserName, .password = "Integration-Passphrase-2026!", .appId = app.Id})
            Dim operatorToken = JObject.Parse(Await operatorLogin.Content.ReadAsStringAsync())("accessToken").Value(Of String)()
            client.DefaultRequestHeaders.Authorization = New AuthenticationHeaderValue("Bearer", operatorToken)
            Check(CInt((Await client.GetAsync("api/v1/users")).StatusCode) = 403, "HTTP: non-admin forbidden")
            Using work = Uow(provider, connection), tx = work.BeginTransaction()
                Dim stored = Await work.Users.GetAsync(user.Id, ct)
                stored.IsActive = False
                Await work.SaveAsync(ct) : tx.Commit()
            End Using
            Check(CInt((Await client.GetAsync("api/v1/auth/me")).StatusCode) = 401, "HTTP: deactivation revokes existing JWT")
        End Using
    End Function
    Private Async Function SendJson(client As HttpClient, method As HttpMethod, path As String, body As Object) As Task(Of HttpResponseMessage)
        Using request As New HttpRequestMessage(method, path)
            request.Content = New StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
            Return Await client.SendAsync(request)
        End Using
    End Function
End Module
