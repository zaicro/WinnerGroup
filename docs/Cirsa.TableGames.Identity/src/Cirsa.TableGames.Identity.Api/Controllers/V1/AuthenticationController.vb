Imports System.Net
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Cirsa.TableGames.Identity.Application.Authentication.Contracts
Imports Cirsa.TableGames.Identity.Application.Authentication.Services

Namespace Controllers.V1
    ''' <summary>
    ''' Provides authentication for registered table game applications.
    ''' </summary>
    <RoutePrefix("api/v1/authentication")>
    Public Class AuthenticationController
        Inherits ApiController

        Private ReadOnly _authenticationService As IAuthenticationService

        Public Sub New(authenticationService As IAuthenticationService)
            If authenticationService Is Nothing Then Throw New ArgumentNullException(NameOf(authenticationService))
            _authenticationService = authenticationService
        End Sub

        ''' <summary>
        ''' Validates credentials and returns the user's application roles.
        ''' </summary>
        <HttpPost>
        <Route("")>
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

            Return Ok(result.User)
        End Function

    End Class
End Namespace