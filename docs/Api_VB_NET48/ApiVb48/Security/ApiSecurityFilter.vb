Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Web.Http
Imports System.Web.Http.Controllers
Imports System.Web.Http.Filters
Imports Microsoft.IdentityModel.Tokens

Namespace Security
    ' El registro global no implica autenticación global:
    ' solo los atributos de política activan la validación.
    Public NotInheritable Class ApiSecurityFilter
        Inherits AuthorizationFilterAttribute

        Public Overrides Sub OnAuthorization(context As HttpActionContext)
            If context.Request.RequestUri.Scheme <> Uri.UriSchemeHttps Then
                context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Forbidden, "Se requiere HTTPS.")
                Return
            End If

            Dim mode = AuthenticationPolicy.Resolve(context.ActionDescriptor)
            If mode = AuthenticationMode.None Then Return

            Select Case mode
                Case AuthenticationMode.ApiKey
                    If Not HasValidApiKey(context) Then
                        Reject(context, "API key inválida o ausente.", mode)
                    End If
                Case AuthenticationMode.Jwt
                    AuthenticateJwt(context, mode)
                Case AuthenticationMode.ApiKeyOrJwt
                    ' Una API key válida basta; no asigna identidad de usuario.
                    If HasValidApiKey(context) Then Return
                    AuthenticateJwt(context, mode)
                Case AuthenticationMode.ApiKeyAndJwt
                    If Not HasValidApiKey(context) Then
                        Reject(context, "API key inválida o ausente.", mode)
                        Return
                    End If
                    AuthenticateJwt(context, mode)
                Case Else
                    Throw New InvalidOperationException("Política de autenticación desconocida.")
            End Select
        End Sub

        Private Shared Function HasValidApiKey(context As HttpActionContext) As Boolean
            Dim values As IEnumerable(Of String) = Nothing
            If Not context.Request.Headers.TryGetValues("X-Api-Key", values) Then Return False
            Dim keys = values.ToArray()
            Return keys.Length = 1 AndAlso
                Not String.IsNullOrWhiteSpace(keys(0)) AndAlso
                keys(0).Length <= 256 AndAlso
                SecretComparer.EqualsSecret(keys(0), AuthSettings.Read("ApiKey"))
        End Function

        Private Shared Sub AuthenticateJwt(context As HttpActionContext, mode As AuthenticationMode)
            Dim header = context.Request.Headers.Authorization
            If header Is Nothing OrElse
                Not String.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) OrElse
                String.IsNullOrWhiteSpace(header.Parameter) OrElse header.Parameter.Length > 8192 Then
                Reject(context, "JWT inválido o ausente.", mode)
                Return
            End If
            Try
                context.RequestContext.Principal = JwtService.ValidateToken(header.Parameter.Trim())
            Catch ex As SecurityTokenException
                ' Diagnóstico local: poner un breakpoint aquí e inspeccionar ex e InnerException.
                Reject(context, "JWT inválido o vencido.", mode)
            Catch ex As ArgumentException
                ' Diagnóstico local: poner un breakpoint aquí e inspeccionar ex e InnerException.
                ' Puede ser formato incorrecto o un error interno de decodificación.
                Reject(context, "JWT inválido.", mode)
            End Try
        End Sub

        Private Shared Sub Reject(context As HttpActionContext, message As String, mode As AuthenticationMode)
            context.Response = context.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, message)
            If mode = AuthenticationMode.ApiKey OrElse mode = AuthenticationMode.ApiKeyOrJwt OrElse mode = AuthenticationMode.ApiKeyAndJwt Then
                context.Response.Headers.WwwAuthenticate.Add(New AuthenticationHeaderValue("ApiKey"))
            End If
            If mode = AuthenticationMode.Jwt OrElse mode = AuthenticationMode.ApiKeyOrJwt OrElse mode = AuthenticationMode.ApiKeyAndJwt Then
                context.Response.Headers.WwwAuthenticate.Add(New AuthenticationHeaderValue("Bearer"))
            End If
        End Sub
    End Class
End Namespace
