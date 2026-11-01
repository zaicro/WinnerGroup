Namespace Entities

    ''' <summary>
    ''' Represents a user with shared credentials across applications.
    ''' </summary>
    Public Class TbUser
        Inherits DbAuditable

        Public Sub New()
            UserId = Guid.NewGuid()
            IsActive = True
            UserRoles = New HashSet(Of TbUserRole)()
        End Sub

        Public Property UserId As Guid
        Public Property UserName As String
        Public Property DisplayName As String
        Public Property PasswordHash As String
        Public Property PhoneNumber As String
        Public Property Email As String
        Public Property IsActive As Boolean

        Public Property UserRoles As ICollection(Of TbUserRole)
    End Class
End Namespace