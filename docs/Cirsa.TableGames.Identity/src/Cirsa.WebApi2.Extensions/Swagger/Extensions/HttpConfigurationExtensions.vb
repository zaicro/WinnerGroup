Imports System
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Web.Http
Imports Swashbuckle.Application
Imports Cirsa.WebApi2.Extensions.Swagger.Configuration
Imports Cirsa.WebApi2.Extensions.Swagger.Filters

Namespace Swagger.Extensions

    ''' <summary>
    ''' Registers Swagger documentation and its interactive UI.
    ''' </summary>
    Public Module HttpConfigurationExtensions

        <Extension()>
        Public Function UseSwaggerDocumentation(
            config As HttpConfiguration,
            options As SwaggerOptions
        ) As HttpConfiguration

            If config Is Nothing Then
                Throw New ArgumentNullException(NameOf(config))
            End If

            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If

            If String.IsNullOrWhiteSpace(options.Title) Then
                Throw New ArgumentException(
                    "The API title is required.",
                    NameOf(options))
            End If

            If String.IsNullOrWhiteSpace(options.Version) Then
                Throw New ArgumentException(
                    "The documentation version is required.",
                    NameOf(options))
            End If

            If Not String.IsNullOrWhiteSpace(
                options.XmlDocumentationPath) Then

                If Not File.Exists(options.XmlDocumentationPath) Then
                    Throw New FileNotFoundException(
                        "The API XML documentation file was not found.",
                        options.XmlDocumentationPath)
                End If
            End If

            config.EnableSwagger(
                Sub(swagger)
                    swagger.SingleApiVersion(
                        options.Version,
                        options.Title)

                    If Not String.IsNullOrWhiteSpace(
                        options.XmlDocumentationPath) Then

                        swagger.IncludeXmlComments(
                            options.XmlDocumentationPath)
                    End If

                    If options.IncludeAuthenticationHeaders Then
                        swagger.OperationFilter(
                            Of SwaggerAuthenticationHeadersFilter)()
                    End If
                End Sub) _
                .EnableSwaggerUi()

            Return config
        End Function

    End Module

End Namespace