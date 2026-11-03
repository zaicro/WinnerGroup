Namespace Configuration

    ''' <summary>
    ''' Provides Swagger documentation settings for the consuming API.
    ''' </summary>
    Public NotInheritable Class SwaggerOptions

        Public Property Title As String

        Public Property Version As String = "v1"

        Public Property XmlDocumentationPath As String

        Public Property IncludeAuthenticationHeaders As Boolean = False

    End Class

End Namespace