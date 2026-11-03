Imports System
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Web.Http
Imports Swashbuckle.Application
Imports Cirsa.WebApi2.Extensions.Swagger.Configuration
Imports Cirsa.WebApi2.Extensions.Swagger.Filters

Namespace Swagger.Extensions
    ''' <summary>Registers Swagger documentation and the embedded Swagger UI integration.</summary>
    Public Module HttpConfigurationExtensions
        Private Const RegistrationKey As String = "Cirsa.WebApi2.Extensions.Swagger.Registered"
        Private Const CredentialsResource As String = "Cirsa.WebApi2.Extensions.Swagger.Assets.authorization.js"

        ''' <summary>Configures documentation once, optionally including API key and JWT schemes.</summary>
        <Extension()>
        Public Function UseSwaggerDocumentation(config As HttpConfiguration, options As SwaggerOptions) As HttpConfiguration
            If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            If config.Properties.ContainsKey(RegistrationKey) Then Throw New InvalidOperationException("Swagger documentation is already configured.")
            If String.IsNullOrWhiteSpace(options.Title) Then Throw New ArgumentException("The API title is required.", NameOf(options))
            If String.IsNullOrWhiteSpace(options.Version) Then Throw New ArgumentException("The documentation version is required.", NameOf(options))
            If Not String.IsNullOrWhiteSpace(options.XmlDocumentationPath) AndAlso Not File.Exists(options.XmlDocumentationPath) Then
                Throw New FileNotFoundException("The API XML documentation file was not found.", options.XmlDocumentationPath)
            End If

            ' Snapshot options: later mutations must not change the published document.
            Dim title = options.Title
            Dim version = options.Version
            Dim xmlPath = options.XmlDocumentationPath
            Dim includeAuthentication = options.IncludeAuthenticationSchemes
            Dim resourceAssembly = GetType(SwaggerSecurityRequirementsFilter).Assembly
            If includeAuthentication AndAlso resourceAssembly.GetManifestResourceInfo(CredentialsResource) Is Nothing Then
                Throw New InvalidOperationException("The embedded Swagger credentials asset is missing.")
            End If

            config.EnableSwagger(
                Sub(swagger)
                    swagger.SingleApiVersion(version, title)
                    If Not String.IsNullOrWhiteSpace(xmlPath) Then swagger.IncludeXmlComments(xmlPath)
                    If includeAuthentication Then
                        swagger.ApiKey("ApiKey").Name("X-Api-Key").In("header").Description("Named application API key.")
                        swagger.ApiKey("Bearer").Name("Authorization").In("header").Description("Bearer followed by a space and a JWT.")
                        swagger.OperationFilter(Of SwaggerSecurityRequirementsFilter)()
                    End If
                End Sub).EnableSwaggerUi(
                Sub(ui)
                    If includeAuthentication Then ui.InjectJavaScript(resourceAssembly, CredentialsResource)
                End Sub)
            config.Properties(RegistrationKey) = True
            Return config
        End Function
    End Module
End Namespace
