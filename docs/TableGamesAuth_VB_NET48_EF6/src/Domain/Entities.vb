Namespace TableGamesAuth.Domain
    Public Interface IEntity
        Property Id As Guid
        Property Version As Guid
    End Interface

    Public Class Role
        Implements IEntity
        Public Property Id As Guid = Guid.NewGuid() Implements IEntity.Id
        Public Property Version As Guid = Guid.NewGuid() Implements IEntity.Version
        Public Property Name As String
        Public Property NormalizedName As String
    End Class

    Public Class ClientApp
        Implements IEntity
        Public Property Id As Guid = Guid.NewGuid() Implements IEntity.Id
        Public Property Version As Guid = Guid.NewGuid() Implements IEntity.Version
        Public Property Name As String
        Public Property Code As String
        Public Property IsActive As Boolean = True
    End Class

    Public Class User
        Implements IEntity
        Public Property Id As Guid = Guid.NewGuid() Implements IEntity.Id
        Public Property Version As Guid = Guid.NewGuid() Implements IEntity.Version
        Public Property UserName As String
        Public Property NormalizedUserName As String
        Public Property PasswordHash As String
        Public Property RoleId As Guid
        Public Property Role As Role
        Public Property IsActive As Boolean = True
        Public Property SecurityStamp As Guid = Guid.NewGuid()
        Public Property FailedLoginCount As Integer
        Public Property LockoutUntilUtc As DateTime?
    End Class

    Public Class UserApp
        Implements IEntity
        Public Property Id As Guid = Guid.NewGuid() Implements IEntity.Id
        Public Property Version As Guid = Guid.NewGuid() Implements IEntity.Version
        Public Property UserId As Guid
        Public Property AppId As Guid
        Public Property User As User
        Public Property App As ClientApp
    End Class
End Namespace
