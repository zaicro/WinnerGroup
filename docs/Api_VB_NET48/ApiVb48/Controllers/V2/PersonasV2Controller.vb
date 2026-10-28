Imports System.Web.Http

Namespace Controllers.V2
    <RoutePrefix("api/v2/personas")>
    Public Class PersonasV2Controller
        Inherits ApiController

        <HttpGet, Route("")>
        Public Function GetAll() As IHttpActionResult
            Return Ok(New With {
                .version = "v2",
                .usuario = User.Identity.Name,
                .datos = New Object() {New With {.id = 1, .nombre = "Andrés", .activo = True}}
            })
        End Function
    End Class
End Namespace
