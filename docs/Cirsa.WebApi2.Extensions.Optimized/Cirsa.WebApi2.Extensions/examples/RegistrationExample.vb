Imports System
Imports System.Collections.Generic
Imports System.Web.Http
Imports Microsoft.Extensions.DependencyInjection
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration
Imports Cirsa.WebApi2.Extensions.Authentication.Extensions
Imports Cirsa.WebApi2.Extensions.Swagger.Configuration
Imports Cirsa.WebApi2.Extensions.Swagger.Extensions

' Integration example. Compile/adapt this in the API, not in the library.
Public Module RegistrationExample
    Public Sub RegisterServices(services As IServiceCollection,
                                apiKeys As IReadOnlyDictionary(Of String, String),
                                issuer As String,
                                audience As String,
                                signingKeyBase64 As String)
        services.AddApiAuthentication(New ApiAuthenticationOptions With {
            .ApiKeys = apiKeys,
            .Issuer = issuer,
            .Audience = audience,
            .SigningKeyBase64 = signingKeyBase64
        })
        services.AddJwtTokenIssuer(New JwtTokenIssuerOptions With {
            .Issuer = issuer,
            .Audience = audience,
            .SigningKeyBase64 = signingKeyBase64,
            .LifetimeMinutes = 15
        })
    End Sub

    Public Sub RegisterHttp(config As HttpConfiguration, rootProvider As IServiceProvider)
        ' The host installs its existing scoped IDependencyResolver separately.
        config.UseApiAuthentication(rootProvider)
        config.UseSwaggerDocumentation(New SwaggerOptions With {
            .Title = "Example API",
            .Version = "v1",
            .IncludeAuthenticationSchemes = True
        })
        config.MapHttpAttributeRoutes()
        config.EnsureInitialized()
    End Sub
End Module
