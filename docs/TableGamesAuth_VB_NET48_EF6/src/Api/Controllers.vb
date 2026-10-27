Imports System.Net
Imports System.Security.Claims
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.Api
    <RoutePrefix("api/v1/auth")>
    Public Class AuthController
        Inherits ApiController
        Private ReadOnly service As AuthService
        Public Sub New(service As AuthService)
            Me.service = service
        End Sub
        <HttpPost, AllowAnonymous, Route("login")>
        Public Async Function Login(input As LoginInput, ct As CancellationToken) As Task(Of IHttpActionResult)
            Return Ok(Await service.Login(input, CType(Request.Properties("KeyAppId"), Guid), ct))
        End Function
        <HttpGet, Authorize, Route("me")>
        Public Function MeInfo() As IHttpActionResult
            Dim principal = CType(User, ClaimsPrincipal)
            Return Ok(New With {.userId = principal.FindFirst(ClaimTypes.NameIdentifier).Value, .userName = User.Identity.Name, .role = principal.FindFirst(ClaimTypes.Role).Value, .appId = principal.FindFirst("app_id").Value})
        End Function
    End Class
    <Authorize(Roles:="SystemAdmin"), RoutePrefix("api/v1/users")>
    Public Class UsersController
        Inherits ApiController
        Private ReadOnly service As CrudService
        Public Sub New(service As CrudService)
            Me.service = service
        End Sub
        <HttpGet, Route("")>
        Public Async Function List(ct As CancellationToken, Optional skip As Integer = 0, Optional take As Integer = 25) As Task(Of IHttpActionResult)
            Return Ok(Await service.ListUsers(skip, take, ct))
        End Function
        <HttpGet, Route("{id:guid}", Name:="GetUser")>
        Public Async Function GetById(id As Guid, ct As CancellationToken) As Task(Of IHttpActionResult)
            Return Ok(Await service.GetUser(id, ct))
        End Function
        <HttpPost, Route("")>
        Public Async Function Create(input As UserInput, ct As CancellationToken) As Task(Of IHttpActionResult)
            Dim result = Await service.SaveUser(Nothing, input, ct)
            Return CreatedAtRoute("GetUser", New With {.id = result.Id}, result)
        End Function
        <HttpPut, Route("{id:guid}")>
        Public Async Function Update(id As Guid, input As UserInput, ct As CancellationToken) As Task(Of IHttpActionResult)
            Return Ok(Await service.SaveUser(id, input, ct))
        End Function
        <HttpDelete, Route("{id:guid}")>
        Public Async Function Delete(id As Guid, version As Guid, ct As CancellationToken) As Task(Of IHttpActionResult)
            If id.ToString() = CType(User, ClaimsPrincipal).FindFirst(ClaimTypes.NameIdentifier).Value Then Throw New AppException(409, "No puede eliminar su propia cuenta.")
            Await service.Delete("users", id, version, ct)
            Return StatusCode(HttpStatusCode.NoContent)
        End Function
    End Class
    ' No generic public controller: only explicitly declared resources are exposed.
End Namespace
