Imports System.Threading
Imports System.Threading.Tasks
Imports System.Security.Claims
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.Application
    ' No IQueryable or EF types cross this boundary.
    Public Interface IRepository(Of T As {Class, IEntity})
        Function GetAsync(id As Guid, ct As CancellationToken) As Task(Of T)
        Function ListAsync(skip As Integer, take As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of T))
        Sub Add(entity As T)
        Sub Remove(entity As T)
    End Interface
    Public Interface IUserRepository
        Inherits IRepository(Of User)
        Function ByNameAsync(normalized As String, ct As CancellationToken) As Task(Of User)
        Function HasRoleAsync(roleId As Guid, ct As CancellationToken) As Task(Of Boolean)
    End Interface
    Public Interface IRoleRepository
        Inherits IRepository(Of Role)
        Function ByNameAsync(normalized As String, ct As CancellationToken) As Task(Of Role)
    End Interface
    Public Interface IAppRepository
        Inherits IRepository(Of ClientApp)
        Function ByCodeAsync(code As String, ct As CancellationToken) As Task(Of ClientApp)
    End Interface
    Public Interface IUserAppRepository
        Inherits IRepository(Of UserApp)
        Function FindAsync(userId As Guid, appId As Guid, ct As CancellationToken) As Task(Of UserApp)
        Function HasAppAsync(appId As Guid, ct As CancellationToken) As Task(Of Boolean)
        Function HasUserAsync(userId As Guid, ct As CancellationToken) As Task(Of Boolean)
    End Interface
    Public Interface ITransaction
        Inherits IDisposable
        Sub Commit()
        Sub Rollback()
    End Interface
    Public Interface IUnitOfWork
        Inherits IDisposable
        ReadOnly Property Users As IUserRepository
        ReadOnly Property Roles As IRoleRepository
        ReadOnly Property Apps As IAppRepository
        ReadOnly Property UserApps As IUserAppRepository
        Function BeginTransaction() As ITransaction
        Function SaveAsync(ct As CancellationToken) As Task
    End Interface
    Public Interface IPasswordHasher
        Function Hash(password As String) As String
        Function Verify(password As String, encoded As String) As Boolean
        Function IsSupportedHash(encoded As String) As Boolean
        Function NeedsRehash(encoded As String) As Boolean
    End Interface
    Public Interface ITokenService
        Function Issue(user As User, role As String, appId As Guid) As LoginResult
        Function Validate(token As String) As ClaimsPrincipal
    End Interface
    Public Class AppException
        Inherits Exception
        Public ReadOnly Property Status As Integer
        Public Sub New(status As Integer, message As String)
            MyBase.New(message)
            Me.Status = status
        End Sub
    End Class
End Namespace
