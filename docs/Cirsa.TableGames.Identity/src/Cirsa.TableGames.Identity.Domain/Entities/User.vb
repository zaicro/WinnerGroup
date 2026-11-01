Namespace Entities

    ''' <summary>
    ''' Represents a user with shared credentials across applications.
    ''' </summary>
    Public Class User

        Public Sub New()
            UserId = Guid.NewGuid()
            IsActive = True
            UserRoles = New HashSet(Of UserRole)()
        End Sub

        Public Property UserId As Guid
        Public Property UserName As String
        Public Property DisplayName As String
        Public Property PasswordHash As String
        Public Property PhoneNumber As String
        Public Property Email As String
        Public Property IsActive As Boolean

        Public Property UserRoles As ICollection(Of UserRole)
    End Class
End Namespace