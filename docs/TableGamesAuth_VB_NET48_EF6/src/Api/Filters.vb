Imports System.Net
Imports System.Net.Http
Imports System.Web.Http.Controllers
Imports System.Web.Http.Filters
Imports System.Web.Http.ExceptionHandling

Namespace TableGamesAuth.Api
    Public Class ValidateModelFilter
        Inherits ActionFilterAttribute
        Public Overrides Sub OnActionExecuting(context As HttpActionContext)
            If Not context.ModelState.IsValid Then
                context.Response = context.Request.CreateResponse(HttpStatusCode.BadRequest, New With {.error = "JSON inválido o campos incorrectos."})
            End If
        End Sub
    End Class
    Public Class SafeExceptionHandler
        Inherits ExceptionHandler
        Private ReadOnly logger As TableGamesAuth.Application.IAppLogger
        Public Sub New(logger As TableGamesAuth.Application.IAppLogger)
            Me.logger = logger
        End Sub
        Public Overrides Sub Handle(context As ExceptionHandlerContext)
            Dim ex = context.Exception
            Dim known = TryCast(ex, TableGamesAuth.Application.AppException)
            Dim status = If(known Is Nothing, HttpStatusCode.InternalServerError, CType(known.Status, HttpStatusCode))
            Dim message = If(known Is Nothing, "Error interno.", known.Message)
            Dim correlation As Object = Nothing
            context.Request.Properties.TryGetValue("CorrelationId", correlation)
            If known Is Nothing Then logger.Error("controller_failed correlation=" & CStr(correlation), ex)
            context.Result = New System.Web.Http.Results.ResponseMessageResult(context.Request.CreateResponse(status, New With {.error = message}))
        End Sub
    End Class
End Namespace
