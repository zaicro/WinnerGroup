Namespace Entities

    ''' <summary>
    ''' Represents the assignment of an application-specific role to a user.
    ''' </summary>
    ''' <remarks>
    ''' The combination of UserId and RoleId must be unique.
    ''' </remarks>
    Public Class TbUserRole
        Inherits DbAuditable

        Public Sub New()
            UserRoleId = Guid.NewGuid()
        End Sub

        Public Property UserRoleId As Guid
        Public Property UserId As Guid
        Public Property RoleId As Guid
        Public Property IsActive As Boolean

        Public Property User As TbUser
        Public Property Role As TbRole
    End Class
End Namespace