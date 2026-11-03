Imports System

Namespace Swagger.Configuration
    ''' <summary>Provides documentation settings without coupling to host configuration files.</summary>
    Public NotInheritable Class SwaggerOptions
        ''' <summary>Gets or sets the API document title.</summary>
        Public Property Title As String
        ''' <summary>Gets or sets the document version; this does not version API routes.</summary>
        Public Property Version As String = "v1"
        ''' <summary>Gets or sets an optional XML documentation file belonging to the consuming API.</summary>
        Public Property XmlDocumentationPath As String
        ''' <summary>Enables credential schemes, per-operation requirements and the credentials dialog.</summary>
        Public Property IncludeAuthenticationSchemes As Boolean = False

        ''' <summary>Compatibility alias. Authentication is now documented as schemes rather than parameters.</summary>
        <Obsolete("Use IncludeAuthenticationSchemes instead. Header parameters have been replaced by security schemes.")>
        Public Property IncludeAuthenticationHeaders As Boolean
            Get
                Return IncludeAuthenticationSchemes
            End Get
            Set(value As Boolean)
                IncludeAuthenticationSchemes = value
            End Set
        End Property
    End Class
End Namespace
