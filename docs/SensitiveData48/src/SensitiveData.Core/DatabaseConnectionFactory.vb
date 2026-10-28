Imports System.Data.SqlClient

Public Module DatabaseConnectionFactory
    Public Function CreateIntegrated(connectionString As String) As SqlConnection
        Dim builder As New SqlConnectionStringBuilder(connectionString)
        If Not builder.IntegratedSecurity OrElse Not String.IsNullOrEmpty(builder.UserID) OrElse Not String.IsNullOrEmpty(builder.Password) Then
            Throw New InvalidOperationException("El modo integrado no admite usuario/contraseña SQL.")
        End If
        Return Harden(builder)
    End Function
    Public Function CreateFromSecret(store As ISecretStore, name As String) As SqlConnection
        Return Harden(New SqlConnectionStringBuilder(store.GetSecret(name).Value))
    End Function
    Private Function Harden(builder As SqlConnectionStringBuilder) As SqlConnection
        builder.Encrypt = True
        builder.TrustServerCertificate = False
        builder.PersistSecurityInfo = False
        Return New SqlConnection(builder.ConnectionString)
    End Function
End Module
