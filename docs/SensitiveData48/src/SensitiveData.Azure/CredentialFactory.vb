Imports Global.Azure.Core
Imports Global.Azure.Identity
Imports System.Security.Cryptography.X509Certificates

' No usa DefaultAzureCredential: en producción se elige una identidad concreta.
Public Module CredentialFactory
    Public Function ManagedIdentity(Optional userAssignedClientId As String = Nothing) As TokenCredential
        If String.IsNullOrWhiteSpace(userAssignedClientId) Then Return New ManagedIdentityCredential()
        Return New ManagedIdentityCredential(userAssignedClientId)
    End Function
    Public Function DeveloperAzureCli(environmentName As String) As TokenCredential
        If environmentName <> "Development" Then Throw New InvalidOperationException("Azure CLI solo está habilitado para Development.")
        Return New AzureCliCredential()
    End Function
    Public Function ClientCertificate(tenantId As String, clientId As String, thumbprint As String) As TokenCredential
        If String.IsNullOrWhiteSpace(tenantId) OrElse String.IsNullOrWhiteSpace(clientId) OrElse String.IsNullOrWhiteSpace(thumbprint) Then
            Throw New ArgumentException("Configure tenantId, clientId y thumbprint.")
        End If
        Using store As New X509Store(StoreName.My, StoreLocation.LocalMachine)
            store.Open(OpenFlags.ReadOnly)
            Dim certificates = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint.Replace(" ", ""), False)
            For Each certificate As X509Certificate2 In certificates
                If certificate.HasPrivateKey AndAlso certificate.NotBefore.ToUniversalTime() <= DateTime.UtcNow AndAlso certificate.NotAfter.ToUniversalTime() > DateTime.UtcNow Then
                    ' La clave privada permanece en el almacén de Windows. Conceda lectura a la identidad del proceso.
                    Return New ClientCertificateCredential(tenantId, clientId, certificate)
                End If
            Next
        End Using
        Throw New InvalidOperationException("No se encontró un certificado vigente con clave privada accesible en LocalMachine/My.")
    End Function
End Module
