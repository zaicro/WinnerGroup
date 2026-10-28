Imports System.Web.Http
Imports ApiVb48.Security

Namespace App_Start
    Public Module WebApiConfig
        Public Sub Register(config As HttpConfiguration)
            'config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Never
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.Always 'para detectar el error de swagger
            config.SuppressHostPrincipal()
            config.Filters.Add(New ApiSecurityFilter())
            config.MapHttpAttributeRoutes()
            config.Formatters.Remove(config.Formatters.XmlFormatter)
            If AuthSettings.Enabled("SwaggerEnabled") Then SwaggerConfig.Register(config)
        End Sub
    End Module
End Namespace
