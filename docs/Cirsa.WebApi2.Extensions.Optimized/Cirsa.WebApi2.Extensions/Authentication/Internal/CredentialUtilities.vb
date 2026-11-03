Imports System
Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.IdentityModel.Tokens

Namespace Authentication.Internal
    Friend NotInheritable Class CredentialUtilities
        Private Shared ReadOnly StrictUtf8 As New UTF8Encoding(False, True)
        Private Sub New()
        End Sub

        Friend Shared Function ComputeHash(value As String) As Byte()
            Using algorithm = SHA256.Create()
                Return algorithm.ComputeHash(StrictUtf8.GetBytes(value))
            End Using
        End Function

        Friend Shared Function DecodeSigningKey(value As String) As SymmetricSecurityKey
            If String.IsNullOrWhiteSpace(value) Then Throw New ArgumentException("The JWT signing key is required.", NameOf(value))
            Dim bytes As Byte()
            Try
                bytes = Convert.FromBase64String(value)
            Catch ex As FormatException
                Throw New ArgumentException("The JWT signing key must be valid Base64.", NameOf(value), ex)
            End Try
            If bytes.Length < 32 Then Throw New ArgumentException("The JWT signing key must contain at least 32 bytes.", NameOf(value))
            Return New SymmetricSecurityKey(bytes)
        End Function
    End Class
End Namespace
