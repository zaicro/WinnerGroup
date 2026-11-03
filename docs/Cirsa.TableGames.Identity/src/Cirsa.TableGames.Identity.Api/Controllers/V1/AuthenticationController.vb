Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Security.Claims
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Cirsa.TableGames.Identity.Application.Authentication.Contracts
Imports Cirsa.TableGames.Identity.Application.Authentication.Services
Imports Cirsa.WebApi2.Extensions.Authentication.Attributes
Imports Cirsa.WebApi2.Extensions.Authentication.Tokens

Namespace Controllers.V1
    ''' <summary>
    ''' Provides authentication for registered table game applications.
    ''' </summary>
    <RoutePrefix("api/v1/authentication")>
    <ApiAuthorize(AuthenticationSchemes:="ApiKey")>
    Public Class AuthenticationController
        Inherits ApiController

        Private ReadOnly _authenticationService As IAuthenticationService
        Private ReadOnly _jwtTokenIssuer As JwtTokenIssuer

        Public Sub New(authenticationService As IAuthenticationService, jwtTokenIssuer As JwtTokenIssuer)
            If authenticationService Is Nothing Then Throw New ArgumentNullException(NameOf(authenticationService))
            If jwtTokenIssuer Is Nothing Then Throw New ArgumentNullException(NameOf(jwtTokenIssuer))
            _authenticationService = authenticationService
            _jwtTokenIssuer = jwtTokenIssuer
        End Sub

        ''' <summary>
        ''' Validates credentials and returns the user's application roles.
        ''' </summary>
        <HttpPost>
        <Route("")>
        <AllowAnonymous>
        Public Async Function Authenticate(<FromBody> request As AuthenticationRequest) As Task(Of IHttpActionResult)

            If request Is Nothing Then _
                Return BadRequest("The request body is required.")

            If Not ModelState.IsValid Then _
                Return BadRequest("The request body is invalid.")

            If String.IsNullOrWhiteSpace(request.UserName) OrElse
               String.IsNullOrEmpty(request.Password) OrElse
               String.IsNullOrWhiteSpace(request.ApplicationCode) Then _
                Return BadRequest("UserName, Password and ApplicationCode are required.")

            Dim result = Await _authenticationService.AuthenticateAsync(request)

            If Not result.Succeeded Then _
                Return StatusCode(HttpStatusCode.Unauthorized)

            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))

            Dim user = result.User

            If user.UserId = Guid.Empty Then Throw New ArgumentException("The user identifier is required.", NameOf(user.UserId))
            If String.IsNullOrWhiteSpace(user.UserName) Then Throw New ArgumentException("The user name is required.", NameOf(user.UserName))
            If user.Roles Is Nothing OrElse user.Roles.Count = 0 Then Throw New ArgumentException("At least one application role is required.", NameOf(user.Roles))

            Dim claims As New List(Of Claim) From {
                New Claim("name", user.UserName),
                New Claim("application_code", request.ApplicationCode)
            }

            If Not String.IsNullOrWhiteSpace(user.DisplayName) Then claims.Add(New Claim("display_name", user.DisplayName))

            Dim addedRoles As New HashSet(Of String)(StringComparer.Ordinal)

            For Each roleCode In user.Roles
                If String.IsNullOrWhiteSpace(roleCode) Then Throw New ArgumentException("Role codes must not be empty.", NameOf(user))
                If addedRoles.Add(roleCode) Then claims.Add(New Claim("role", roleCode))
            Next

            Dim token = _jwtTokenIssuer.Create(result.User.UserId.ToString("D"), claims.AsReadOnly())
            Dim response = Me.Request.CreateResponse(HttpStatusCode.OK,
                New With {
                    .User = result.User,
                    .AccessToken = token.AccessToken,
                    .TokenType = "Bearer",
                    .ExpiresAtUtc = token.ExpiresAtUtc
                })

            response.Headers.CacheControl =
                New CacheControlHeaderValue With {
                    .NoStore = True
                }

            response.Headers.Pragma.Add(New NameValueHeaderValue("no-cache"))

            Return ResponseMessage(response)
            'Return Ok(result.User)
        End Function

    End Class
End Namespace