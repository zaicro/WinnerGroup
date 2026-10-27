Imports System.Data.Entity
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.DataAccess.Ef6
    Public Class Repository(Of T As {Class, IEntity})
        Implements IRepository(Of T)
        Protected ReadOnly db As AuthDbContext
        Public Sub New(db As AuthDbContext)
            Me.db = db
        End Sub
        Public Async Function GetAsync(id As Guid, ct As CancellationToken) As Task(Of T) Implements IRepository(Of T).GetAsync
            Return Await db.Set(Of T)().SingleOrDefaultAsync(Function(e) e.Id = id, ct)
        End Function
        Public Async Function ListAsync(skip As Integer, take As Integer, ct As CancellationToken) As Task(Of IReadOnlyList(Of T)) Implements IRepository(Of T).ListAsync
            If skip < 0 OrElse skip > 1000000 OrElse take < 1 OrElse take > 100 Then Throw New AppException(400, "Use skip entre 0 y 1000000 y take entre 1 y 100.")
            Return Await db.Set(Of T)().AsNoTracking().OrderBy(Function(e) e.Id).Skip(skip).Take(take).ToListAsync(ct)
        End Function
        Public Sub Add(entity As T) Implements IRepository(Of T).Add
            db.Set(Of T)().Add(entity)
        End Sub
        Public Sub Remove(entity As T) Implements IRepository(Of T).Remove
            db.Set(Of T)().Remove(entity)
        End Sub
    End Class
    Public Class UserRepository
        Inherits Repository(Of User)
        Implements IUserRepository
        Public Sub New(db As AuthDbContext)
            MyBase.New(db)
        End Sub
        Public Function ByNameAsync(normalized As String, ct As CancellationToken) As Task(Of User) Implements IUserRepository.ByNameAsync
            Return db.Users.SingleOrDefaultAsync(Function(u) u.NormalizedUserName = normalized, ct)
        End Function
        Public Function HasRoleAsync(roleId As Guid, ct As CancellationToken) As Task(Of Boolean) Implements IUserRepository.HasRoleAsync
            Return db.Users.AnyAsync(Function(u) u.RoleId = roleId, ct)
        End Function
    End Class
    Public Class RoleRepository
        Inherits Repository(Of Role)
        Implements IRoleRepository
        Public Sub New(db As AuthDbContext)
            MyBase.New(db)
        End Sub
        Public Function ByNameAsync(normalized As String, ct As CancellationToken) As Task(Of Role) Implements IRoleRepository.ByNameAsync
            Return db.Roles.SingleOrDefaultAsync(Function(u) u.NormalizedName = normalized, ct)
        End Function
    End Class
    Public Class AppRepository
        Inherits Repository(Of ClientApp)
        Implements IAppRepository
        Public Sub New(db As AuthDbContext)
            MyBase.New(db)
        End Sub
        Public Function ByCodeAsync(code As String, ct As CancellationToken) As Task(Of ClientApp) Implements IAppRepository.ByCodeAsync
            Return db.Apps.SingleOrDefaultAsync(Function(u) u.Code = code, ct)
        End Function
    End Class
    Public Class UserAppRepository
        Inherits Repository(Of UserApp)
        Implements IUserAppRepository
        Public Sub New(db As AuthDbContext)
            MyBase.New(db)
        End Sub
        Public Function FindAsync(userId As Guid, appId As Guid, ct As CancellationToken) As Task(Of UserApp) Implements IUserAppRepository.FindAsync
            Return db.UserApps.SingleOrDefaultAsync(Function(u) u.UserId = userId AndAlso u.AppId = appId, ct)
        End Function
        Public Function HasAppAsync(appId As Guid, ct As CancellationToken) As Task(Of Boolean) Implements IUserAppRepository.HasAppAsync
            Return db.UserApps.AnyAsync(Function(u) u.AppId = appId, ct)
        End Function
        Public Function HasUserAsync(userId As Guid, ct As CancellationToken) As Task(Of Boolean) Implements IUserAppRepository.HasUserAsync
            Return db.UserApps.AnyAsync(Function(u) u.UserId = userId, ct)
        End Function
    End Class
End Namespace
