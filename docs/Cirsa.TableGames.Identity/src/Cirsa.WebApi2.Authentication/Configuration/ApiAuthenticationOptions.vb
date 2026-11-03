Namespace Configuration

    ''' <summary>
    ''' Provides API key and JWT validation settings.
    ''' </summary>
    Public NotInheritable Class ApiAuthenticationOptions

        Public Property ApiKeys As IReadOnlyDictionary(Of String, String)

        Public Property Issuer As String

        Public Property Audience As String

        Public Property SigningKeyBase64 As String

    End Class

End Namespace