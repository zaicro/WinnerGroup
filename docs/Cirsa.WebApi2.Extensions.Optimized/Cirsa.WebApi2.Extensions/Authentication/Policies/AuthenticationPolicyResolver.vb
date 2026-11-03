Imports System
Imports System.Linq
Imports System.Web.Http
Imports System.Web.Http.Controllers
Imports Cirsa.WebApi2.Extensions.Authentication.Attributes
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration

Namespace Authentication.Policies
    ''' <summary>Resolves the same effective policy for authentication and documentation.</summary>
    Public NotInheritable Class AuthenticationPolicyResolver
        Private Sub New()
        End Sub

        ''' <summary>Applies anonymous precedence, action policy, then controller policy.</summary>
        ''' <remarks>Undecorated actions are public. Policies are replaced, not combined.</remarks>
        Public Shared Function Resolve(action As HttpActionDescriptor) As AuthenticationMode
            If action Is Nothing Then Throw New ArgumentNullException(NameOf(action))
            If action.GetCustomAttributes(Of AllowAnonymousAttribute)().Any() OrElse
               action.ControllerDescriptor.GetCustomAttributes(Of AllowAnonymousAttribute)().Any() Then
                Return AuthenticationMode.None
            End If
            Dim policies = action.GetCustomAttributes(Of ApiAuthorizeAttribute)().ToArray()
            If policies.Length = 0 Then policies = action.ControllerDescriptor.GetCustomAttributes(Of ApiAuthorizeAttribute)().ToArray()
            If policies.Length > 1 Then Throw New InvalidOperationException("Only one authentication policy is allowed at each level.")
            If policies.Length = 0 Then Return AuthenticationMode.None
            Return Resolve(policies(0))
        End Function

        ''' <summary>Validates scheme names and resolves a declared policy.</summary>
        Public Shared Function Resolve(policy As ApiAuthorizeAttribute) As AuthenticationMode
            If policy Is Nothing Then Throw New ArgumentNullException(NameOf(policy))
            If String.IsNullOrWhiteSpace(policy.AuthenticationSchemes) Then
                Throw New InvalidOperationException("AuthenticationSchemes must specify at least one authentication scheme.")
            End If
            Dim usesApiKey As Boolean = False
            Dim usesJwt As Boolean = False
            For Each scheme In policy.AuthenticationSchemes.Split(","c)
                Select Case scheme.Trim().ToUpperInvariant()
                    Case "APIKEY"
                        usesApiKey = True
                    Case "BEARER"
                        usesJwt = True
                    Case Else
                        Throw New InvalidOperationException("Unsupported authentication scheme: " & scheme.Trim())
                End Select
            Next
            If usesApiKey AndAlso usesJwt Then Return If(policy.RequireAllSchemes, AuthenticationMode.Both, AuthenticationMode.Either)
            Return If(usesApiKey, AuthenticationMode.ApiKey, AuthenticationMode.Jwt)
        End Function
    End Class
End Namespace
