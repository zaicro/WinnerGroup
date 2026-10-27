Imports System.Web.Http
Imports System.Web.Http.ExceptionHandling
Imports Microsoft.Extensions.DependencyInjection
Imports Newtonsoft.Json.Serialization
Imports TableGamesAuth.Application

Namespace TableGamesAuth.Api
    Public Module ApiConfiguration
        Public Function Create(services As ServiceProvider, settings As Settings) As HttpConfiguration
            Dim config As New HttpConfiguration()
            config.DependencyResolver = New DependencyResolver(services)
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never
            config.MapHttpAttributeRoutes()
            config.Formatters.Remove(config.Formatters.XmlFormatter)
            config.Formatters.JsonFormatter.SerializerSettings.ContractResolver = New CamelCasePropertyNamesContractResolver()
            config.Formatters.JsonFormatter.SerializerSettings.MaxDepth = 16
            config.Filters.Add(New ValidateModelFilter())
            config.Services.Replace(GetType(IExceptionHandler), New SafeExceptionHandler(services.GetRequiredService(Of IAppLogger)()))
            config.MessageHandlers.Add(New ErrorHandler(services.GetRequiredService(Of IAppLogger)()))
            config.MessageHandlers.Add(New SecurityHandler(settings, services.GetRequiredService(Of ITokenService)()))
            Return config
        End Function
    End Module
End Namespace
