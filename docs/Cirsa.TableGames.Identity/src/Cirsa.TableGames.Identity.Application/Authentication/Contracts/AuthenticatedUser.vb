Namespace Authentication.Contracts
    Public Class AuthenticatedUser
        Public Property UserId As Guid
        Public Property UserName As String
        Public Property DisplayName As String
        Public Property Roles As List(Of String)
    End Class
End Namespace