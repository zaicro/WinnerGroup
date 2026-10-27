Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain

Public Class MemoryRepository(Of T As {Class, IEntity})
    Implements IRepository(Of T)
    Public ReadOnly Items As New List(Of T)
    Public Function GetAsync(id As Guid, ct As CancellationToken) As Task(Of T) Implements IRepository(Of T).GetAsync
        Return Task.FromResult(Items.SingleOrDefault(Function(e) e.Id = id))
    End Function
    Public Function ListAsync(skip As Integer, take As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of T)) Implements IRepository(Of T).ListAsync
        Return Task.FromResult(Of IReadOnlyList(Of T))(Items.Skip(skip).Take(take).ToList())
    End Function
    Public Sub Add(entity As T) Implements IRepository(Of T).Add
        Items.Add(entity)
    End Sub
    Public Sub Remove(entity As T) Implements IRepository(Of T).Remove
        Items.Remove(entity)
    End Sub
End Class
Public Class MemoryUsers
    Inherits MemoryRepository(Of User)
    Implements IUserRepository
    Public Function ByNameAsync(name As String, ct As CancellationToken) As Task(Of User) Implements IUserRepository.ByNameAsync
        Return Task.FromResult(Items.SingleOrDefault(Function(e) e.NormalizedUserName = name))
    End Function
    Public Function HasRoleAsync(id As Guid, ct As CancellationToken) As Task(Of Boolean) Implements IUserRepository.HasRoleAsync
        Return Task.FromResult(Items.Any(Function(e) e.RoleId = id))
    End Function
End Class
Public Class MemoryRoles
    Inherits MemoryRepository(Of Role)
    Implements IRoleRepository
    Public Function ByNameAsync(name As String, ct As CancellationToken) As Task(Of Role) Implements IRoleRepository.ByNameAsync
        Return Task.FromResult(Items.SingleOrDefault(Function(e) e.NormalizedName = name))
    End Function
End Class
Public Class MemoryApps
    Inherits MemoryRepository(Of ClientApp)
    Implements IAppRepository
    Public Function ByCodeAsync(code As String, ct As CancellationToken) As Task(Of ClientApp) Implements IAppRepository.ByCodeAsync
        Return Task.FromResult(Items.SingleOrDefault(Function(e) e.Code = code))
    End Function
End Class
Public Class MemoryGrants
    Inherits MemoryRepository(Of UserApp)
    Implements IUserAppRepository
    Public Function FindAsync(userId As Guid, appId As Guid, ct As CancellationToken) As Task(Of UserApp) Implements IUserAppRepository.FindAsync
        Return Task.FromResult(Items.SingleOrDefault(Function(e) e.UserId = userId AndAlso e.AppId = appId))
    End Function
    Public Function HasAppAsync(appId As Guid, ct As CancellationToken) As Task(Of Boolean) Implements IUserAppRepository.HasAppAsync
        Return Task.FromResult(Items.Any(Function(e) e.AppId = appId))
    End Function
    Public Function HasUserAsync(userId As Guid, ct As CancellationToken) As Task(Of Boolean) Implements IUserAppRepository.HasUserAsync
        Return Task.FromResult(Items.Any(Function(e) e.UserId = userId))
    End Function
End Class
Public Class MemoryTransaction
    Implements ITransaction
    Public Committed As Boolean
    Public RolledBack As Boolean
    Public Sub Commit() Implements ITransaction.Commit
        Committed = True
    End Sub
    Public Sub Rollback() Implements ITransaction.Rollback
        RolledBack = True
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        If Not Committed Then RolledBack = True
    End Sub
End Class
Public Class MemoryUnitOfWork
    Implements IUnitOfWork
    Public ReadOnly UserStore As New MemoryUsers()
    Public ReadOnly RoleStore As New MemoryRoles()
    Public ReadOnly AppStore As New MemoryApps()
    Public ReadOnly GrantStore As New MemoryGrants()
    Public LastTransaction As MemoryTransaction
    Public SaveFails As Boolean
    Public ReadOnly Property Users As IUserRepository Implements IUnitOfWork.Users
        Get
            Return UserStore
        End Get
    End Property
    Public ReadOnly Property Roles As IRoleRepository Implements IUnitOfWork.Roles
        Get
            Return RoleStore
        End Get
    End Property
    Public ReadOnly Property Apps As IAppRepository Implements IUnitOfWork.Apps
        Get
            Return AppStore
        End Get
    End Property
    Public ReadOnly Property UserApps As IUserAppRepository Implements IUnitOfWork.UserApps
        Get
            Return GrantStore
        End Get
    End Property
    Public Function BeginTransaction() As ITransaction Implements IUnitOfWork.BeginTransaction
        LastTransaction = New MemoryTransaction()
        Return LastTransaction
    End Function
    Public Function SaveAsync(ct As CancellationToken) As Task Implements IUnitOfWork.SaveAsync
        If SaveFails Then Throw New AppException(409, "Simulated save conflict")
        Return Task.CompletedTask
    End Function
    Public Sub Dispose() Implements IDisposable.Dispose
    End Sub
End Class
