Imports System.Net
Imports System.Security.Cryptography
Imports System.Web.Http
Imports ApiVb48.Models
Imports ApiVb48.Security

Namespace Controllers.V1
    <RoutePrefix("api/v1/auth")>
    Public Class AuthController
        Inherits ApiController

        <HttpPost, Route("token"), ApiKeyOnly>
        Public Function Token(<FromBody> input As LoginRequest) As IHttpActionResult
            If Not AuthSettings.Enabled("DemoLoginEnabled") Then Return NotFound()
            If input Is Nothing OrElse Not ModelState.IsValid Then Return BadRequest("Solicitud de login inválida.")
            Dim salt = Convert.FromBase64String(AuthSettings.Read("DemoPasswordSalt"))
            Dim hash As String
            Using pbkdf = New Rfc2898DeriveBytes(input.Password, salt, 600000, HashAlgorithmName.SHA256)
                hash = Convert.ToBase64String(pbkdf.GetBytes(32))
            End Using
            Dim passwordOk = SecretComparer.EqualsSecret(hash, AuthSettings.Read("DemoPasswordHash"))
            Dim usernameOk = String.Equals(input.Username, AuthSettings.Read("DemoUsername"), StringComparison.Ordinal)
            If Not passwordOk OrElse Not usernameOk Then Return Content(HttpStatusCode.Unauthorized, New With {.message = "Credenciales inválidas."})
            Return Ok(New With {.access_token = JwtService.CreateToken(input.Username), .token_type = "Bearer", .expires_in = 900})
        End Function
    End Class
End Namespace
