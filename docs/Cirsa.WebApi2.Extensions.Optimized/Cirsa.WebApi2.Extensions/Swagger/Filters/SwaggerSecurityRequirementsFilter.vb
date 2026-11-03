Imports System
Imports System.Collections.Generic
Imports System.Web.Http.Description
Imports Swashbuckle.Swagger
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration
Imports Cirsa.WebApi2.Extensions.Authentication.Policies

Namespace Swagger.Filters
    ''' <summary>Documents the effective policy using Swagger 2.0 security requirements.</summary>
    Public NotInheritable Class SwaggerSecurityRequirementsFilter
        Implements IOperationFilter

        ''' <summary>Uses the same action/controller policy resolver as the authentication filter.</summary>
        Public Sub Apply(operation As Operation, schemaRegistry As SchemaRegistry, apiDescription As ApiDescription) Implements IOperationFilter.Apply
            If operation Is Nothing Then Throw New ArgumentNullException(NameOf(operation))
            If apiDescription Is Nothing Then Throw New ArgumentNullException(NameOf(apiDescription))
            Dim mode = AuthenticationPolicyResolver.Resolve(apiDescription.ActionDescriptor)
            Dim requirements As New List(Of IDictionary(Of String, IEnumerable(Of String)))()
            Select Case mode
                Case AuthenticationMode.None
                    ' An empty list explicitly declares this operation public.
                Case AuthenticationMode.ApiKey
                    requirements.Add(Requirement("ApiKey"))
                Case AuthenticationMode.Jwt
                    requirements.Add(Requirement("Bearer"))
                Case AuthenticationMode.Both
                    requirements.Add(Requirement("ApiKey", "Bearer"))
                Case AuthenticationMode.Either
                    requirements.Add(Requirement("ApiKey"))
                    requirements.Add(Requirement("Bearer"))
                Case Else
                    Throw New InvalidOperationException("Unsupported authentication mode.")
            End Select
            operation.security = requirements
            If mode <> AuthenticationMode.None Then
                If operation.responses Is Nothing Then operation.responses = New Dictionary(Of String, Response)()
                If Not operation.responses.ContainsKey("401") Then
                    operation.responses.Add("401", New Response With {.description = "Required credentials are missing, invalid or expired."})
                End If
            End If
        End Sub

        Private Shared Function Requirement(ParamArray names As String()) As IDictionary(Of String, IEnumerable(Of String))
            Dim result As New Dictionary(Of String, IEnumerable(Of String))(StringComparer.Ordinal)
            For Each name In names
                result.Add(name, New String() {})
            Next
            Return result
        End Function
    End Class
End Namespace
