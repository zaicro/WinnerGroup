Namespace Authentication.Configuration

    ''' <summary>
    ''' Provides API key and JWT validation settings.
    ''' </summary>
    Public NotInheritable Class ApiAuthenticationOptions

        Public Property ApiKeys As IReadOnlyDictionary(Of String, String)

        Public Property Issuer As String

        Public Property Audience As String

        Public Property SigningKeyBase64 As String

        ''' <summary>Enables named API keys. Defaults to True for compatibility.</summary>
        Public Property EnableApiKey As Boolean = True

        ''' <summary>Enables JWT validation. Defaults to True for compatibility.</summary>
        Public Property EnableJwt As Boolean = True

    End Class

End Namespace
