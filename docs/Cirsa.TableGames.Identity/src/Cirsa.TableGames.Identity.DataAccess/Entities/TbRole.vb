Namespace Entities

    ''' <summary>
    ''' Gets or sets the stable role code, unique within its application.
    ''' </summary>
    Public Class TbRole
        Inherits DbAuditable

        Public Sub New()
            RoleId = Guid.NewGuid()
            IsActive = True
            UserRoles = New HashSet(Of TbUserRole)()
        End Sub

        Public Property RoleId As Guid
        Public Property ApplicationId As Guid
        Public Property Code As String
        Public Property Name As String
        Public Property IsActive As Boolean

        Public Property App As TbApp
        Public Property UserRoles As ICollection(Of TbUserRole)
    End Class
End Namespace