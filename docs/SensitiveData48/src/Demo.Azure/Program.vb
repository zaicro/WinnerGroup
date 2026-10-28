Imports System.Configuration
Imports Global.Azure.Core
Imports SensitiveData48.Core
Imports SensitiveData48.AzureIntegration

Module Program
    Function Main() As Integer
        Try
            Dim credential As TokenCredential
            Select Case Setting("AzureAuthentication")
                Case "ManagedIdentity"
                    credential = CredentialFactory.ManagedIdentity(ConfigurationManager.AppSettings("ManagedIdentityClientId"))
                Case "ClientCertificate"
                    credential = CredentialFactory.ClientCertificate(Setting("TenantId"), Setting("ClientId"), Setting("CertificateThumbprint"))
                Case "AzureCli"
                    credential = CredentialFactory.DeveloperAzureCli(Setting("Environment"))
                Case Else
                    Throw New ConfigurationErrorsException("Identidad de Azure desconocida.")
            End Select
            Dim provider As New KeyVaultSecretStore(New Uri(Setting("KeyVaultUri")), credential)
            Dim cache As New CachedSecretStore(provider, TimeSpan.FromSeconds(60))
            Dim jwt As New JwtTokenService(cache, Setting("JwtIssuer"), Setting("JwtAudience"), Setting("JwtCurrentKid"), Setting("JwtAcceptedKids").Split(","c))
            Dim token = jwt.Issue("usuario-demo")
            Console.WriteLine("JWT firmado y validado con una llave recuperada de Key Vault: " & (jwt.Validate(token).Identity.Name = "usuario-demo").ToString())
            Dim hmac As New HmacService(cache)
            Console.WriteLine("HMAC validado: " & hmac.Verify("demo", hmac.Sign("demo")).ToString())
            ' Activar solo si la aplicación necesita contraseña SQL y se creó el secreto correspondiente.
            If ConfigurationManager.AppSettings("UseSqlSecret") = "true" Then
                Using connection = DatabaseConnectionFactory.CreateFromSecret(cache, "db-connection-string")
                    Console.WriteLine("Conexión construida desde secreto con TLS. No se abrió ni se imprimió.")
                End Using
            End If
            Return 0
        Catch ex As Exception
            Console.Error.WriteLine("Operación fallida. Tipo: " & ex.GetType().Name & ". Revise identidad, RBAC y conectividad en el depurador.")
            ' No hay fallback automático hacia DPAPI o secretos de desarrollo.
            Return 1
        End Try
    End Function
    Private Function Setting(name As String) As String
        Dim value = ConfigurationManager.AppSettings(name)
        If String.IsNullOrWhiteSpace(value) Then Throw New ConfigurationErrorsException("Falta una opción pública requerida.")
        Return value
    End Function
End Module
