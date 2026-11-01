Namespace Entities

    ''' <summary>
    ''' Gets or sets the stable role code, unique within its application.
    ''' </summary>
    Public Class Role

        Public Sub New()
            RoleId = Guid.NewGuid()
            IsActive = True
            UserRoles = New HashSet(Of UserRole)()
        End Sub

        Public Property RoleId As Guid
        Public Property ApplicationId As Guid
        Public Property Code As String
        Public Property Name As String
        Public Property IsActive As Boolean

        Public Property App As App
        Public Property UserRoles As ICollection(Of UserRole)
    End Class
End Namespace