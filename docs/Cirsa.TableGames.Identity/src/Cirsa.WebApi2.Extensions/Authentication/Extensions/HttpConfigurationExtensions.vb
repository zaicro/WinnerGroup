Imports System.Runtime.CompilerServices
Imports System.Web.Http
Imports Microsoft.Extensions.DependencyInjection
Imports Cirsa.WebApi2.Extensions.Authentication.Filters
Imports Cirsa.WebApi2.Extensions.Authentication.Validation

Namespace Authentication.Extensions

    ''' <summary>
    ''' Integrates authentication services with the Web API pipeline.
    ''' </summary>
    Public Module HttpConfigurationExtensions

        <Extension()>
        Public Function UseApiAuthentication(
            config As HttpConfiguration,
            provider As IServiceProvider
        ) As HttpConfiguration

            If config Is Nothing Then
                Throw New ArgumentNullException(NameOf(config))
            End If

            If provider Is Nothing Then
                Throw New ArgumentNullException(NameOf(provider))
            End If

            Dim validator =
                provider.GetRequiredService(Of AuthorizeValidator)()

            config.Filters.Add(
                New AuthenticationFilter(validator))

            Return config
        End Function

    End Module

End Namespace