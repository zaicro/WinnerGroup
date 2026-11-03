Imports System.Net.Http.Headers
Imports System.Security.Claims
Imports System.Threading
Imports System.Web.Http.Controllers
Imports System.Web.Http.Filters
Imports System.Web.Http.Results
Imports Cirsa.WebApi2.Authentication.Validation
Imports Cirsa.WebApi2.Authentication.Attributes
Imports System.Web.Http

Namespace Filters

    Public NotInheritable Class AuthenticationFilter
        Implements IAuthenticationFilter

        Private ReadOnly _authorizeValidator As AuthorizeValidator

        Public Sub New(authorizeValidator As AuthorizeValidator)
            If authorizeValidator Is Nothing Then Throw New ArgumentNullException(NameOf(authorizeValidator))
            _authorizeValidator = authorizeValidator
        End Sub

        Public ReadOnly Property AllowMultiple As Boolean _
            Implements IFilter.AllowMultiple

            Get
                Return False
            End Get
        End Property

        Public Function AuthenticateAsync(context As HttpAuthenticationContext, cancellationToken As CancellationToken) As Task _
            Implements IAuthenticationFilter.AuthenticateAsync

            cancellationToken.ThrowIfCancellationRequested()

            Dim mode = GetMode(context.ActionContext)

            If mode = AuthenticationMode.None Then Return Task.CompletedTask

            context.Principal = New ClaimsPrincipal(New ClaimsIdentity())

            Dim checksApiKey =
                mode = AuthenticationMode.ApiKey OrElse
                mode = AuthenticationMode.Both OrElse
                mode = AuthenticationMode.Either

            Dim checksJwt =
                mode = AuthenticationMode.Jwt OrElse
                mode = AuthenticationMode.Both OrElse
                mode = AuthenticationMode.Either

            Dim apiKeyValid As Boolean = False
            Dim apiKeyName As String = Nothing
            Dim jwtValid As Boolean = False
            Dim jwtPrincipal As ClaimsPrincipal = Nothing

            If checksApiKey Then
                Dim values As IEnumerable(Of String) = Nothing

                If context.Request.Headers.TryGetValues("X-Api-Key", values) Then
                    Dim keys = values.ToArray()
                    apiKeyValid = keys.Length = 1 AndAlso _authorizeValidator.TryValidateApiKey(keys(0), apiKeyName)
                End If
            End If

            If checksJwt Then
                Dim authorization = context.Request.Headers.Authorization

                If authorization IsNot Nothing AndAlso String.Equals(authorization.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) Then
                    jwtValid = _authorizeValidator.TryValidateJwt(authorization.Parameter, jwtPrincipal)
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
                    Throw New InvalidOperationException("Unsupported security mode.")
            End Select

            If Not accepted Then Reject(context, mode) : Return Task.CompletedTask

            If jwtValid Then
                context.Principal = jwtPrincipal
            Else
                context.Principal = New ClaimsPrincipal(
                New ClaimsIdentity(
                    New Claim() {
                        New Claim("api_key_name", apiKeyName),
                        New Claim("credential_type", "api_key")
                    },
                    "ApiKey",
                    "api_key_name",
                    ClaimTypes.Role))
            End If

            Return Task.CompletedTask
        End Function

        Public Function ChallengeAsync(context As HttpAuthenticationChallengeContext, cancellationToken As CancellationToken) As Task _
            Implements IAuthenticationFilter.ChallengeAsync
            Return Task.CompletedTask
        End Function

        Private Function GetMode(actionContext As HttpActionContext) As AuthenticationMode
            Dim actionDescriptor = actionContext.ActionDescriptor
            Dim controllerDescriptor = actionContext.ControllerContext.ControllerDescriptor
            Dim allowsAnonymous =
                actionDescriptor.GetCustomAttributes(Of AllowAnonymousAttribute)().Any() OrElse
                controllerDescriptor.GetCustomAttributes(Of AllowAnonymousAttribute)().Any()

            If allowsAnonymous Then Return AuthenticationMode.None

            Dim actionPolicy = actionDescriptor.GetCustomAttributes(Of ApiAuthorizeAttribute)().FirstOrDefault()

            If actionPolicy IsNot Nothing Then Return ResolveMode(actionPolicy)

            Dim controllerPolicy = controllerDescriptor.GetCustomAttributes(Of ApiAuthorizeAttribute)().FirstOrDefault()

            If controllerPolicy IsNot Nothing Then Return ResolveMode(controllerPolicy)

            Return AuthenticationMode.None
        End Function

        Private Function ResolveMode(policy As ApiAuthorizeAttribute) As AuthenticationMode
            If String.IsNullOrWhiteSpace(policy.AuthenticationSchemes) Then _
                Throw New InvalidOperationException("AuthenticationSchemes must specify at least one authentication scheme.")

            Dim schemes = policy.AuthenticationSchemes.Split(","c).Select(Function(value) value.Trim()).ToArray()
            Dim usesApiKey As Boolean = False
            Dim usesJwt As Boolean = False

            For Each scheme In schemes
                Select Case scheme.ToUpperInvariant()
                    Case "APIKEY"
                        usesApiKey = True
                    Case "BEARER"
                        usesJwt = True
                    Case Else
                        Throw New InvalidOperationException("Unsupported authentication scheme: " & scheme)
                End Select
            Next

            If usesApiKey AndAlso usesJwt Then Return If(policy.RequireAllSchemes, AuthenticationMode.Both, AuthenticationMode.Either)

            Return If(usesApiKey, AuthenticationMode.ApiKey, AuthenticationMode.Jwt)
        End Function

        Private Shared Sub Reject(context As HttpAuthenticationContext, mode As AuthenticationMode)

            Dim challenges As New List(Of AuthenticationHeaderValue)()

            If mode = AuthenticationMode.ApiKey OrElse mode = AuthenticationMode.Both OrElse mode = AuthenticationMode.Either Then _
                challenges.Add(New AuthenticationHeaderValue("ApiKey"))
            If mode = AuthenticationMode.Jwt OrElse mode = AuthenticationMode.Both OrElse mode = AuthenticationMode.Either Then _
                challenges.Add(New AuthenticationHeaderValue("Bearer"))

            context.ErrorResult = New UnauthorizedResult(challenges, context.Request)
        End Sub

    End Class

End Namespace
