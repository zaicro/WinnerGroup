Namespace Authentication.Tokens

    ''' <summary>
    ''' Contains a signed access token and its expiration time.
    ''' </summary>
    Public NotInheritable Class IssuedJwtToken

        Public ReadOnly Property AccessToken As String

        Public ReadOnly Property ExpiresAtUtc As DateTime

        Friend Sub New(accessToken As String, expiresAtUtc As DateTime)
            Me.AccessToken = accessToken
            Me.ExpiresAtUtc = expiresAtUtc
        End Sub

    End Class

End Namespace
