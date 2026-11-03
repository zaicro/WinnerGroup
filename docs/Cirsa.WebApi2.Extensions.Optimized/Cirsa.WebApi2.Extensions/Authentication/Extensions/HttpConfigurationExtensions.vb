Imports System
Imports System.Linq
Imports System.Runtime.CompilerServices
Imports System.Web.Http
Imports Microsoft.Extensions.DependencyInjection
Imports Cirsa.WebApi2.Extensions.Authentication.Filters
Imports Cirsa.WebApi2.Extensions.Authentication.Policies
Imports Cirsa.WebApi2.Extensions.Authentication.Validation

Namespace Authentication.Extensions
    ''' <summary>Integrates the registered validator with the Web API pipeline.</summary>
    Public Module HttpConfigurationExtensions
        Private Const RegistrationKey As String = "Cirsa.WebApi2.Extensions.Authentication.Registered"

        ''' <summary>Registers one filter and validates discovered action policies during initialization.</summary>
        <Extension()>
        Public Function UseApiAuthentication(config As HttpConfiguration, provider As IServiceProvider) As HttpConfiguration
            If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))
            If provider Is Nothing Then Throw New ArgumentNullException(NameOf(provider))
            If config.Properties.ContainsKey(RegistrationKey) OrElse config.Filters.Any(Function(item) TypeOf item.Instance Is AuthenticationFilter) Then
                Throw New InvalidOperationException("API authentication is already configured.")
            End If
            Dim validator = provider.GetRequiredService(Of AuthorizeValidator)()
            config.Filters.Add(New AuthenticationFilter(validator))
            config.Properties(RegistrationKey) = True

            Dim previousInitializer = config.Initializer
            config.Initializer =
                Sub(configuration)
                    If previousInitializer IsNot Nothing Then previousInitializer(configuration)
                    ValidateAuthenticationPolicies(configuration, validator)
                End Sub
            Return config
        End Function

        ''' <summary>Checks discoverable controller actions after routes have been registered.</summary>
        Public Sub ValidateAuthenticationPolicies(config As HttpConfiguration, validator As AuthorizeValidator)
            If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))
            If validator Is Nothing Then Throw New ArgumentNullException(NameOf(validator))
            For Each controller In config.Services.GetHttpControllerSelector().GetControllerMapping().Values.Distinct()
                For Each action In config.Services.GetActionSelector().GetActionMapping(controller).SelectMany(Function(group) group).Distinct()
                    validator.EnsureModeConfigured(AuthenticationPolicyResolver.Resolve(action))
                Next
            Next
        End Sub
    End Module
End Namespace
