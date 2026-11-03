Namespace Configuration

    ''' <summary>
    ''' Provides JWT issuance settings.
    ''' </summary>
    Public NotInheritable Class JwtTokenIssuerOptions

        Public Property Issuer As String

        Public Property Audience As String

        Public Property SigningKeyBase64 As String

        Public Property LifetimeMinutes As Integer = 15

    End Class

End Namespace