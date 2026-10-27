Imports System.Security.Claims
Imports System.Threading
Imports System.Threading.Tasks
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.Application
    Public Class AuthService
        Private ReadOnly uow As IUnitOfWork
        Private ReadOnly hasher As IPasswordHasher
        Private ReadOnly tokens As ITokenService
        Private ReadOnly dummyHash As String
        Private ReadOnly logger As IAppLogger
        Public Sub New(uow As IUnitOfWork, hasher As IPasswordHasher, tokens As ITokenService, dummyHash As String, Optional logger As IAppLogger = Nothing)
            Me.uow = uow : Me.hasher = hasher : Me.tokens = tokens : Me.dummyHash = dummyHash
            Me.logger = If(logger, New NullAppLogger())
        End Sub
        Public Async Function Login(input As LoginInput, keyAppId As Guid, ct As CancellationToken) As Task(Of LoginResult)
            CrudService.Validate(input)
            If input.AppId <> keyAppId Then Throw New AppException(401, "Credenciales inválidas.")
            Dim rejected As Boolean
            Dim result As LoginResult = Nothing
            Using tx = uow.BeginTransaction()
                Dim user = Await uow.Users.ByNameAsync(CrudService.Normalize(input.UserName), ct)
                ' Always run the KDF, including unknown or locked users.
                Dim valid = hasher.Verify(input.Password, If(user Is Nothing, dummyHash, user.PasswordHash))
                Dim now = DateTime.UtcNow
                If user Is Nothing Then
                    rejected = True
                ElseIf Not user.IsActive OrElse (user.LockoutUntilUtc.HasValue AndAlso user.LockoutUntilUtc.Value > now) Then
                    rejected = True
                ElseIf Not valid Then
                    If user.LockoutUntilUtc.HasValue AndAlso user.LockoutUntilUtc.Value <= now Then user.FailedLoginCount = 0
                    user.FailedLoginCount += 1
                    If user.FailedLoginCount >= 5 Then user.LockoutUntilUtc = now.AddMinutes(15)
                    Await uow.SaveAsync(ct)
                    rejected = True
                Else
                    Dim app = Await uow.Apps.GetAsync(input.AppId, ct)
                    Dim grant = Await uow.UserApps.FindAsync(user.Id, input.AppId, ct)
                    Dim role = Await uow.Roles.GetAsync(user.RoleId, ct)
                    If app Is Nothing OrElse Not app.IsActive OrElse grant Is Nothing OrElse role Is Nothing Then
                        rejected = True
                    Else
                        user.FailedLoginCount = 0
                        user.LockoutUntilUtc = Nothing
                        If hasher.NeedsRehash(user.PasswordHash) Then user.PasswordHash = hasher.Hash(input.Password)
                        Await uow.SaveAsync(ct)
                        result = tokens.Issue(user, role.Name, input.AppId)
                    End If
                End If
                ' Persist failed attempts before returning 401; do not throw inside the transaction.
                tx.Commit()
            End Using
            If rejected Then
                logger.Warn("login_rejected app=" & keyAppId.ToString())
                Throw New AppException(401, "Credenciales inválidas.")
            End If
            logger.Info("login_succeeded app=" & keyAppId.ToString())
            Return result
        End Function
        Public Async Function ValidateSession(principal As ClaimsPrincipal, appId As Guid, ct As CancellationToken) As Task(Of ClaimsPrincipal)
            Dim id, stamp, tokenApp As Guid
            If Not Guid.TryParse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value, id) OrElse
               Not Guid.TryParse(principal.FindFirst("stamp")?.Value, stamp) OrElse
               Not Guid.TryParse(principal.FindFirst("app_id")?.Value, tokenApp) OrElse tokenApp <> appId Then Return Nothing
            Dim user = Await uow.Users.GetAsync(id, ct)
            If user Is Nothing OrElse Not user.IsActive OrElse user.SecurityStamp <> stamp Then Return Nothing
            Dim app = Await uow.Apps.GetAsync(appId, ct)
            If app Is Nothing OrElse Not app.IsActive OrElse Await uow.UserApps.FindAsync(id, appId, ct) Is Nothing Then Return Nothing
            Dim role = Await uow.Roles.GetAsync(user.RoleId, ct)
            If role Is Nothing Then Return Nothing
            ' Rebuild roles from the current database; never authorize using stale role claims.
            Return New ClaimsPrincipal(New ClaimsIdentity(New Claim() {
                New Claim(ClaimTypes.NameIdentifier, id.ToString()), New Claim(ClaimTypes.Name, user.UserName),
                New Claim(ClaimTypes.Role, role.Name), New Claim("app_id", appId.ToString())}, "Bearer"))
        End Function
    End Class
End Namespace
