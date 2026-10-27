Imports System.Linq
Imports System.Net
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.Api
    <Authorize(Roles:="SystemAdmin"), RoutePrefix("api/v1/roles")>
    Public Class RolesController
        Inherits ApiController
        Private ReadOnly service As CrudService
        Private ReadOnly uow As IUnitOfWork
        Public Sub New(service As CrudService, uow As IUnitOfWork)
            Me.service = service : Me.uow = uow
        End Sub
        <HttpGet, Route("")>
        Public Async Function List(ct As CancellationToken, Optional skip As Integer = 0, Optional take As Integer = 25) As Task(Of IHttpActionResult)
            Return Ok(Await uow.Roles.ListAsync(skip, take, ct))
        End Function
        <HttpGet, Route("{id:guid}", Name:="GetRoles")>
        Public Async Function GetById(id As Guid, ct As CancellationToken) As Task(Of IHttpActionResult)
            Dim result = Await uow.Roles.GetAsync(id, ct)
            If result Is Nothing Then Return NotFound()
            Return Ok(result)
        End Function
        <HttpPost, Route("")>
        Public Async Function Create(input As RoleInput, ct As CancellationToken) As Task(Of IHttpActionResult)
            Dim result = Await service.SaveRole(Nothing, input, ct)
            Return CreatedAtRoute("GetRoles", New With {.id = result.Id}, result)
        End Function
        <HttpPut, Route("{id:guid}")>
        Public Async Function Update(id As Guid, input As RoleInput, ct As CancellationToken) As Task(Of IHttpActionResult)
            Dim result = Await service.SaveRole(id, input, ct)
            Return Ok(result)
        End Function
        <HttpDelete, Route("{id:guid}")>
        Public Async Function Delete(id As Guid, version As Guid, ct As CancellationToken) As Task(Of IHttpActionResult)
            Await service.Delete("roles", id, version, ct)
            Return StatusCode(HttpStatusCode.NoContent)
        End Function
    End Class
End Namespace
