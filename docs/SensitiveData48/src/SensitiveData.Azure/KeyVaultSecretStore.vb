Imports Global.Azure.Core
Imports Global.Azure.Security.KeyVault.Secrets
Imports SensitiveData48.Core

Public NotInheritable Class KeyVaultSecretStore
    Implements ISecretStore
    Private ReadOnly _client As SecretClient
    Public Sub New(vaultUri As Uri, credential As TokenCredential)
        If vaultUri Is Nothing OrElse vaultUri.Scheme <> Uri.UriSchemeHttps Then Throw New ArgumentException("Key Vault requiere una URI HTTPS.")
        Dim options As New SecretClientOptions()
        options.Retry.MaxRetries = 2
        options.Retry.NetworkTimeout = TimeSpan.FromSeconds(10)
        _client = New SecretClient(vaultUri, credential, options)
    End Sub
    Public Function GetSecret(name As String) As SecretValue Implements ISecretStore.GetSecret
        Dim secret = _client.GetSecret(name).Value
        If secret.Properties.Enabled.HasValue AndAlso Not secret.Properties.Enabled.Value Then
            Throw New InvalidOperationException("Secreto deshabilitado.")
        End If
        Dim now = DateTimeOffset.UtcNow
        If secret.Properties.ExpiresOn.HasValue AndAlso secret.Properties.ExpiresOn.Value <= now Then
            Throw New InvalidOperationException("Secreto vencido.")
        End If
        If secret.Properties.NotBefore.HasValue AndAlso secret.Properties.NotBefore.Value > now Then
            Throw New InvalidOperationException("Secreto aún no vigente.")
        End If
        Return New SecretValue(secret.Value, secret.Properties.Version, secret.Properties.ExpiresOn)
    End Function
End Class
