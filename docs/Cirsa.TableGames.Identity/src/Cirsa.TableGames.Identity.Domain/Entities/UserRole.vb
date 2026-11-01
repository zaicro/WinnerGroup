Namespace Entities

    ''' <summary>
    ''' Represents the assignment of an application-specific role to a user.
    ''' </summary>
    ''' <remarks>
    ''' The combination of UserId and RoleId must be unique.
    ''' </remarks>
    Public Class UserRole

        Public Sub New()
            UserRoleId = Guid.NewGuid()
        End Sub

        Public Property UserRoleId As Guid
        Public Property UserId As Guid
        Public Property RoleId As Guid
        Public Property IsActive As Boolean

        Public Property User As User
        Public Property Role As Role
    End Class
End Namespace