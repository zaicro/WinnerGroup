Imports System.Configuration

Namespace Security
    Public NotInheritable Class AuthSettings
        Private Sub New()
        End Sub

        Public Shared Function Read(name As String) As String
            Dim value = Environment.GetEnvironmentVariable("APIVB48_" & name)
            If String.IsNullOrWhiteSpace(value) Then value = ConfigurationManager.AppSettings(name)
            Return If(value, String.Empty)
        End Function

        Public Shared Function Enabled(name As String) As Boolean
            Return String.Equals(Read(name), "true", StringComparison.OrdinalIgnoreCase)
        End Function

        Public Shared Function SigningKey() As Byte()
            Return Convert.FromBase64String(Read("JwtSigningKeyBase64"))
        End Function

        Public Shared Sub Validate()
            If Read("ApiKey").Length < 32 OrElse Read("ApiKey").Length > 256 Then
                Throw New ConfigurationErrorsException("Configure ApiKey: entre 32 y 256 caracteres aleatorios.")
            End If
            If SigningKey().Length < 32 Then Throw New ConfigurationErrorsException("JWT requiere al menos 32 bytes aleatorios.")
            If String.IsNullOrWhiteSpace(Read("JwtIssuer")) OrElse String.IsNullOrWhiteSpace(Read("JwtAudience")) Then
                Throw New ConfigurationErrorsException("Configure JwtIssuer y JwtAudience.")
            End If
            If Enabled("DemoLoginEnabled") Then
                If String.IsNullOrWhiteSpace(Read("DemoUsername")) OrElse
                    Convert.FromBase64String(Read("DemoPasswordSalt")).Length < 16 OrElse
                    Convert.FromBase64String(Read("DemoPasswordHash")).Length <> 32 Then
                    Throw New ConfigurationErrorsException("Configure las credenciales del proveedor de demostración.")
                End If
            End If
        End Sub
    End Class
End Namespace
