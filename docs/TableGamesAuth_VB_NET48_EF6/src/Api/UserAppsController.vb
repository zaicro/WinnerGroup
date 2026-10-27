Imports System.Linq
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.Api
    <Authorize(Roles:="SystemAdmin"), RoutePrefix("api/v1/user-apps")>
    Public Class UserAppsController
        Inherits ApiController
        Private ReadOnly service As CrudService
        Private ReadOnly uow As IUnitOfWork
        Public Sub New(service As CrudService, uow As IUnitOfWork)
            Me.service = service : Me.uow = uow
        End Sub
        Private Shared Function View(e As UserApp) As Object
            Return New With {.id = e.Id, .version = e.Version, .userId = e.UserId, .appId = e.AppId}
        End Function
        <HttpGet, Route("")>
        Public Async Function List(ct As CancellationToken, Optional skip As Integer = 0, Optional take As Integer = 25) As Task(Of IHttpActionResult)
            Return Ok((Await uow.UserApps.ListAsync(skip, take, ct)).Select(Function(e) View(e)).ToList())
        End Function
        <HttpGet, Route("{id:guid}", Name:="GetUserApps")>
        Public Async Function GetById(id As Guid, ct As CancellationToken) As Task(Of IHttpActionResult)
            Dim result = Await uow.UserApps.GetAsync(id, ct)
            If result Is Nothing Then Return NotFound()
            Return Ok(View(result))
        End Function
        <HttpPost, Route("")>
        Public Async Function Create(input As UserAppInput, ct As CancellationToken) As Task(Of IHttpActionResult)
            Dim result = Await service.SaveUserApp(Nothing, input, ct)
            Return CreatedAtRoute("GetUserApps", New With {.id = result.Id}, View(result))
        End Function
        <HttpPut, Route("{id:guid}")>
        Public Async Function Update(id As Guid, input As UserAppInput, ct As CancellationToken) As Task(Of IHttpActionResult)
            Dim result = Await service.SaveUserApp(id, input, ct)
            Return Ok(View(result))
        End Function
        <HttpDelete, Route("{id:guid}")>
        Public Async Function Delete(id As Guid, version As Guid, ct As CancellationToken) As Task(Of IHttpActionResult)
            Await service.Delete("user-apps", id, version, ct)
            Return StatusCode(HttpStatusCode.NoContent)
        End Function
    End Class
End Namespace
