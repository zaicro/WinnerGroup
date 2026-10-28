Imports System.Reflection
Imports System.Web.Http
Imports Swashbuckle.Application

Namespace App_Start
    Public Module SwaggerConfig
        Public Sub Register(config As HttpConfiguration)
            config.EnableSwagger(
                Sub(c)
                    c.Schemes(New String() {"https"})
                    c.MultipleApiVersions(
                        Function(description, version) description.RelativePath.StartsWith("api/" & version & "/", StringComparison.OrdinalIgnoreCase),
                        Sub(versions)
                            versions.Version("v1", "API VB.NET - v1")
                            versions.Version("v2", "API VB.NET - v2")
                        End Sub)
                    c.ApiKey("ApiKey").Name("X-Api-Key").In("header").Description("Clave de la aplicación cliente.")
                    c.ApiKey("Bearer").Name("Authorization").In("header").Description("Escribir Bearer seguido del JWT.")
                    c.OperationFilter(Of Swagger.SecurityRequirementsFilter)()
                End Sub).EnableSwaggerUi(
                    Sub(ui)
                        ui.InjectJavaScript(Assembly.GetExecutingAssembly(), "ApiVb48.Swagger.credentials.js")
                    End Sub)
        End Sub
    End Module
End Namespace
