Imports System.Web.Http.Description
Imports Swashbuckle.Swagger

Namespace Filters
    ''' <summary>
    ''' Adds authentication headers to the interactive API documentation.
    ''' </summary>
    Public Class SwaggerAuthenticationHeadersFilter
        Implements IOperationFilter

        Public Sub Apply(
            operation As Operation,
            schemaRegistry As SchemaRegistry,
            apiDescription As ApiDescription
        ) Implements IOperationFilter.Apply

            If operation.parameters Is Nothing Then
                operation.parameters = New List(Of Parameter)()
            End If

            operation.parameters.Add(
                New Parameter With {
                    .name = "X-Api-Key",
                    .[in] = "header",
                    .type = "string",
                    .required = False,
                    .description =
                        "API key, when required by the endpoint."
                })

            operation.parameters.Add(
                New Parameter With {
                    .name = "Authorization",
                    .[in] = "header",
                    .type = "string",
                    .required = False,
                    .description =
                        "Enter Bearer followed by a space and the JWT."
                })
        End Sub

    End Class

End Namespace