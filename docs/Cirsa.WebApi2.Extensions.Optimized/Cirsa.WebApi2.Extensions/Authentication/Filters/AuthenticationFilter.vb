Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net.Http.Headers
Imports System.Security.Claims
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http.Filters
Imports System.Web.Http.Results
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration
Imports Cirsa.WebApi2.Extensions.Authentication.Policies
Imports Cirsa.WebApi2.Extensions.Authentication.Validation

Namespace Authentication.Filters
    ''' <summary>Authenticates each request according to its effective action policy.</summary>
    Public NotInheritable Class AuthenticationFilter
        Implements IAuthenticationFilter

        Private ReadOnly _validator As AuthorizeValidator

        ''' <summary>Creates a stateless filter backed by an immutable credential validator.</summary>
        Public Sub New(authorizeValidator As AuthorizeValidator)
            If authorizeValidator Is Nothing Then Throw New ArgumentNullException(NameOf(authorizeValidator))
            _validator = authorizeValidator
        End Sub

        ''' <summary>Gets False because only one global authentication filter is supported.</summary>
        Public ReadOnly Property AllowMultiple As Boolean Implements IFilter.AllowMultiple
            Get
                Return False
            End Get
        End Property

        ''' <summary>Checks selected credentials and preserves all successfully validated identities.</summary>
        Public Function AuthenticateAsync(context As HttpAuthenticationContext, cancellationToken As CancellationToken) As Task Implements IAuthenticationFilter.AuthenticateAsync
            If context Is Nothing Then Throw New ArgumentNullException(NameOf(context))
            cancellationToken.ThrowIfCancellationRequested()
            Dim mode = AuthenticationPolicyResolver.Resolve(context.ActionContext.ActionDescriptor)
            _validator.EnsureModeConfigured(mode)
            ' Preserve the existing host behavior for explicitly public actions.
            If mode = AuthenticationMode.None Then Return Task.CompletedTask
            context.Principal = New ClaimsPrincipal(New ClaimsIdentity())

            Dim apiKeyValid As Boolean = False
            Dim keyName As String = Nothing
            Dim jwtValid As Boolean = False
            Dim jwtPrincipal As ClaimsPrincipal = Nothing
            If mode <> AuthenticationMode.Jwt Then
                Dim values As IEnumerable(Of String) = Nothing
                If context.Request.Headers.TryGetValues("X-Api-Key", values) Then
                    Dim keys = values.Take(2).ToArray()
                    apiKeyValid = keys.Length = 1 AndAlso _validator.TryValidateApiKey(keys(0), keyName)
                End If
            End If
            If mode <> AuthenticationMode.ApiKey Then
                Dim header As AuthenticationHeaderValue = Nothing
                Try
                    header = context.Request.Headers.Authorization
                Catch ex As FormatException
                    ' Treat malformed authentication headers as invalid credentials.
                End Try
                If header IsNot Nothing AndAlso String.Equals(header.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) Then
                    jwtValid = _validator.TryValidateJwt(header.Parameter, jwtPrincipal)
                End If
            End If

            Dim accepted As Boolean
            Select Case mode
                Case AuthenticationMode.ApiKey
                    accepted = apiKeyValid
                Case AuthenticationMode.Jwt
                    accepted = jwtValid
                Case AuthenticationMode.Both
                    accepted = apiKeyValid AndAlso jwtValid
                Case AuthenticationMode.Either
                    accepted = apiKeyValid OrElse jwtValid
                Case Else
                    Throw New InvalidOperationException("Unsupported authentication mode.")
            End Select
            If Not accepted Then
                context.ErrorResult = New UnauthorizedResult(AuthenticationChallenges.ForMode(mode), context.Request)
                Return Task.CompletedTask
            End If

            Dim principal = If(jwtValid, jwtPrincipal, New ClaimsPrincipal())
            If apiKeyValid Then
                principal.AddIdentity(New ClaimsIdentity(New Claim() {
                    New Claim("api_key_name", keyName),
                    New Claim("credential_type", "api_key")}, "ApiKey", "api_key_name", ClaimTypes.Role))
            End If
            context.Principal = principal
            Return Task.CompletedTask
        End Function

        ''' <summary>Adds missing challenges to 401 results without duplicating existing headers.</summary>
        Public Function ChallengeAsync(context As HttpAuthenticationChallengeContext, cancellationToken As CancellationToken) As Task Implements IAuthenticationFilter.ChallengeAsync
            If context Is Nothing Then Throw New ArgumentNullException(NameOf(context))
            cancellationToken.ThrowIfCancellationRequested()
            Dim mode = AuthenticationPolicyResolver.Resolve(context.ActionContext.ActionDescriptor)
            If mode <> AuthenticationMode.None Then
                context.Result = New AuthenticationChallengeResult(context.Result, AuthenticationChallenges.ForMode(mode))
            End If
            Return Task.CompletedTask
        End Function
    End Class
End Namespace
