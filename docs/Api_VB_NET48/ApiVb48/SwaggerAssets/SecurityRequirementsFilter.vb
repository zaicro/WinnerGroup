Imports System.Web.Http.Description
Imports ApiVb48.Security
Imports Swashbuckle.Swagger

' El namespace permanece igual aunque la carpeta física se llame SwaggerAssets.
Namespace Swagger
    Public Class SecurityRequirementsFilter
        Implements IOperationFilter

        Public Sub Apply(operation As Operation, schemaRegistry As SchemaRegistry, apiDescription As ApiDescription) Implements IOperationFilter.Apply
            Dim mode = AuthenticationPolicy.Resolve(apiDescription.ActionDescriptor)
            Dim requirements As New List(Of IDictionary(Of String, IEnumerable(Of String)))()
            Select Case mode
                Case AuthenticationMode.None
                    ' [] declara explícitamente la operación pública.
                Case AuthenticationMode.ApiKey
                    requirements.Add(Requirement("ApiKey"))
                Case AuthenticationMode.Jwt
                    requirements.Add(Requirement("Bearer"))
                Case AuthenticationMode.ApiKeyOrJwt
                    ' Dos objetos son OR.
                    requirements.Add(Requirement("ApiKey"))
                    requirements.Add(Requirement("Bearer"))
                Case AuthenticationMode.ApiKeyAndJwt
                    ' Dos esquemas en un solo objeto son AND.
                    requirements.Add(Requirement("ApiKey", "Bearer"))
                Case Else
                    Throw New InvalidOperationException("Política de autenticación desconocida.")
            End Select
            operation.security = requirements
            If operation.responses Is Nothing Then operation.responses = New Dictionary(Of String, Response)()
            If mode <> AuthenticationMode.None Then
                operation.responses("401") = New Response With {.description = "Credenciales ausentes, inválidas o vencidas."}
            End If
            operation.responses("403") = New Response With {.description = "Se requiere HTTPS."}
        End Sub

        Private Shared Function Requirement(ParamArray names As String()) As IDictionary(Of String, IEnumerable(Of String))
            Dim result As New Dictionary(Of String, IEnumerable(Of String))()
            For Each name In names
                result.Add(name, New String() {})
            Next
            Return result
        End Function
    End Class
End Namespace
