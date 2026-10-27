Imports System.ComponentModel.DataAnnotations
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.Application
    Public Class CrudService
        Private ReadOnly uow As IUnitOfWork
        Private ReadOnly hasher As IPasswordHasher
        Public Sub New(uow As IUnitOfWork, hasher As IPasswordHasher)
            Me.uow = uow
            Me.hasher = hasher
        End Sub
        Public Shared Sub Validate(input As Object)
            If input Is Nothing Then Throw New AppException(400, "El cuerpo es obligatorio.")
            Dim results As New List(Of ValidationResult)
            If Not Validator.TryValidateObject(input, New ValidationContext(input), results, True) Then
                Throw New AppException(400, String.Join(" ", results.Select(Function(r) r.ErrorMessage)))
            End If
        End Sub
        Public Shared Function Normalize(value As String) As String
            Return value.Trim().ToUpperInvariant()
        End Function
        Private Shared Sub CheckVersion(entity As IEntity, version As Guid)
            If entity Is Nothing Then Throw New AppException(404, "Registro no encontrado.")
            If version = Guid.Empty OrElse entity.Version <> version Then Throw New AppException(409, "Versión desactualizada. Consulte el registro y reintente.")
        End Sub
        Public Shared Function View(user As User) As UserView
            If user Is Nothing Then Throw New AppException(404, "Usuario no encontrado.")
            Return New UserView With {.Id = user.Id, .Version = user.Version, .UserName = user.UserName, .RoleId = user.RoleId, .IsActive = user.IsActive}
        End Function
        Public Async Function ListUsers(skip As Integer, take As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of UserView))
            Return (Await uow.Users.ListAsync(skip, take, ct)).Select(Function(u) View(u)).ToList()
        End Function
        Public Async Function GetUser(id As Guid, ct As CancellationToken) As Task(Of UserView)
            Return View(Await uow.Users.GetAsync(id, ct))
        End Function
        Public Async Function SaveUser(id As Guid?, input As UserInput, ct As CancellationToken) As Task(Of UserView)
            Validate(input)
            Using tx = uow.BeginTransaction()
                Dim user As User
                If id.HasValue Then
                    user = Await uow.Users.GetAsync(id.Value, ct)
                    CheckVersion(user, input.Version)
                Else
                    user = New User()
                End If
                If Await uow.Roles.GetAsync(input.RoleId, ct) Is Nothing Then Throw New AppException(400, "Rol inexistente.")
                Dim normalized = Normalize(input.UserName)
                Dim duplicate = Await uow.Users.ByNameAsync(normalized, ct)
                If duplicate IsNot Nothing AndAlso duplicate.Id <> user.Id Then Throw New AppException(409, "Nombre de usuario en uso.")
                Dim passwordProvided = Not String.IsNullOrEmpty(input.Password)
                Dim hashProvided = Not String.IsNullOrEmpty(input.PasswordHash)
                If passwordProvided AndAlso hashProvided Then Throw New AppException(400, "Use Password o PasswordHash, no ambos.")
                If Not id.HasValue AndAlso Not passwordProvided AndAlso Not hashProvided Then Throw New AppException(400, "La contraseña es obligatoria.")
                If passwordProvided Then user.PasswordHash = hasher.Hash(input.Password)
                If hashProvided Then
                    If Not hasher.IsSupportedHash(input.PasswordHash) Then Throw New AppException(400, "Hash no admitido. Use el formato PBKDF2 documentado.")
                    user.PasswordHash = input.PasswordHash
                End If
                user.UserName = input.UserName.Trim()
                user.NormalizedUserName = normalized
                user.RoleId = input.RoleId
                user.IsActive = input.IsActive
                user.SecurityStamp = Guid.NewGuid() ' Invalidates all prior tokens on administrative update.
                If passwordProvided OrElse hashProvided Then
                    user.FailedLoginCount = 0
                    user.LockoutUntilUtc = Nothing
                End If
                If Not id.HasValue Then uow.Users.Add(user)
                Await uow.SaveAsync(ct)
                tx.Commit()
                Return View(user)
            End Using
        End Function
        Public Async Function SaveRole(id As Guid?, input As RoleInput, ct As CancellationToken) As Task(Of Role)
            Validate(input)
            Using tx = uow.BeginTransaction()
                Dim role = If(id.HasValue, Await uow.Roles.GetAsync(id.GetValueOrDefault(), ct), New Role())
                If id.HasValue Then CheckVersion(role, input.Version)
                If id.HasValue AndAlso role.NormalizedName = "SYSTEMADMIN" Then Throw New AppException(409, "El rol reservado SystemAdmin no se modifica.")
                If String.IsNullOrWhiteSpace(input.Name) Then Throw New AppException(400, "Nombre vacío.")
                Dim normalized = Normalize(input.Name)
                Dim duplicate = Await uow.Roles.ByNameAsync(normalized, ct)
                If duplicate IsNot Nothing AndAlso duplicate.Id <> role.Id Then Throw New AppException(409, "Rol existente.")
                If normalized = "SYSTEMADMIN" Then Throw New AppException(409, "Nombre de rol reservado.")
                role.Name = input.Name.Trim()
                role.NormalizedName = normalized
                If Not id.HasValue Then uow.Roles.Add(role)
                Await uow.SaveAsync(ct)
                tx.Commit()
                Return role
            End Using
        End Function
        Public Async Function SaveApp(id As Guid?, input As AppInput, ct As CancellationToken) As Task(Of ClientApp)
            Validate(input)
            Using tx = uow.BeginTransaction()
                Dim app = If(id.HasValue, Await uow.Apps.GetAsync(id.GetValueOrDefault(), ct), New ClientApp())
                If id.HasValue Then CheckVersion(app, input.Version)
                Dim code = Normalize(input.Code)
                Dim duplicate = Await uow.Apps.ByCodeAsync(code, ct)
                If duplicate IsNot Nothing AndAlso duplicate.Id <> app.Id Then Throw New AppException(409, "Código existente.")
                If String.IsNullOrWhiteSpace(input.Name) Then Throw New AppException(400, "Nombre vacío.")
                app.Name = input.Name.Trim()
                app.Code = code
                app.IsActive = input.IsActive
                If Not id.HasValue Then uow.Apps.Add(app)
                Await uow.SaveAsync(ct)
                tx.Commit()
                Return app
            End Using
        End Function
        Public Async Function SaveUserApp(id As Guid?, input As UserAppInput, ct As CancellationToken) As Task(Of UserApp)
            Validate(input)
            Using tx = uow.BeginTransaction()
                Dim grant = If(id.HasValue, Await uow.UserApps.GetAsync(id.GetValueOrDefault(), ct), New UserApp())
                If id.HasValue Then CheckVersion(grant, input.Version)
                Dim user = Await uow.Users.GetAsync(input.UserId, ct)
                If user Is Nothing OrElse Await uow.Apps.GetAsync(input.AppId, ct) Is Nothing Then Throw New AppException(400, "Usuario o aplicación inexistente.")
                Dim duplicate = Await uow.UserApps.FindAsync(input.UserId, input.AppId, ct)
                If duplicate IsNot Nothing AndAlso duplicate.Id <> grant.Id Then Throw New AppException(409, "Permiso existente.")
                grant.UserId = input.UserId
                grant.AppId = input.AppId
                If Not id.HasValue Then uow.UserApps.Add(grant)
                Await uow.SaveAsync(ct)
                tx.Commit()
                Return grant
            End Using
        End Function
        Public Async Function Delete(kind As String, id As Guid, version As Guid, ct As CancellationToken) As Task
            Using tx = uow.BeginTransaction()
                Select Case kind
                    Case "users"
                        Dim e = Await uow.Users.GetAsync(id, ct)
                        CheckVersion(e, version)
                        If Await uow.UserApps.HasUserAsync(id, ct) Then Throw New AppException(409, "Quite primero sus permisos de aplicaciones.")
                        uow.Users.Remove(e)
                    Case "roles"
                        Dim e = Await uow.Roles.GetAsync(id, ct)
                        CheckVersion(e, version)
                        If e.NormalizedName = "SYSTEMADMIN" OrElse Await uow.Users.HasRoleAsync(id, ct) Then Throw New AppException(409, "Rol reservado o en uso.")
                        uow.Roles.Remove(e)
                    Case "apps"
                        Dim e = Await uow.Apps.GetAsync(id, ct)
                        CheckVersion(e, version)
                        If Await uow.UserApps.HasAppAsync(id, ct) Then Throw New AppException(409, "Quite primero los permisos de esta aplicación.")
                        uow.Apps.Remove(e)
                    Case "user-apps"
                        Dim e = Await uow.UserApps.GetAsync(id, ct)
                        CheckVersion(e, version)
                        uow.UserApps.Remove(e)
                    Case Else
                        Throw New AppException(404, "Recurso inexistente.")
                End Select
                Await uow.SaveAsync(ct)
                tx.Commit()
            End Using
        End Function
    End Class
End Namespace
