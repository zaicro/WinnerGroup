Imports System.Web.Http

Namespace Controllers.V1
    <RoutePrefix("api/v1/personas")>
    Public Class PersonasV1Controller
        Inherits ApiController

        <HttpGet, Route("")>
        Public Function GetAll() As IHttpActionResult
            Return Ok(New Object() {New With {.id = 1, .nombre = "Andrés"}})
        End Function
    End Class
End Namespace
