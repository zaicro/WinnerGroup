Imports System.Runtime.CompilerServices
Imports System.Linq
Imports Microsoft.Extensions.DependencyInjection
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration
Imports Cirsa.WebApi2.Extensions.Authentication.Tokens
Imports Cirsa.WebApi2.Extensions.Authentication.Validation

Namespace Authentication.Extensions

    ''' <summary>
    ''' Registers API authentication and token issuance services.
    ''' </summary>
    Public Module ServiceCollectionExtensions

        <Extension()>
        Public Function AddApiAuthentication(
            services As IServiceCollection,
            options As ApiAuthenticationOptions
        ) As IServiceCollection

            If services Is Nothing Then
                Throw New ArgumentNullException(NameOf(services))
            End If

            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If

            If services.Any(Function(item) item.ServiceType = GetType(AuthorizeValidator)) Then
                Throw New InvalidOperationException("API authentication services are already registered.")
            End If
            services.AddSingleton(Of AuthorizeValidator)(New AuthorizeValidator(options))

            Return services
        End Function

        <Extension()>
        Public Function AddJwtTokenIssuer(
            services As IServiceCollection,
            options As JwtTokenIssuerOptions
        ) As IServiceCollection

            If services Is Nothing Then
                Throw New ArgumentNullException(NameOf(services))
            End If

            If options Is Nothing Then
                Throw New ArgumentNullException(NameOf(options))
            End If

            If services.Any(Function(item) item.ServiceType = GetType(JwtTokenIssuer)) Then
                Throw New InvalidOperationException("JWT issuance services are already registered.")
            End If
            Dim issuer As New JwtTokenIssuer(
                options.Issuer,
                options.Audience,
                options.SigningKeyBase64,
                options.LifetimeMinutes)

            services.AddSingleton(Of JwtTokenIssuer)(issuer)

            Return services
        End Function

    End Module

End Namespace