Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Security.Cryptography
Imports System.Text
Imports Microsoft.IdentityModel.Tokens

Namespace Validation

    ''' <summary>
    ''' Validates configured API keys and signed JWT bearer tokens.
    ''' </summary>
    Public NotInheritable Class AuthorizeValidator

        Private Const MinApiKeyLength As Integer = 32
        Private Const MaxApiKeyLength As Integer = 512
        Private Const MinSigningKeyBytes As Integer = 32
        Private Const MaxTokenSize As Integer = 8192

        Private ReadOnly _apiKeyHashes As KeyValuePair(Of String, Byte())()
        Private ReadOnly _jwtParameters As TokenValidationParameters

        Public Sub New(apiKeys As IReadOnlyDictionary(Of String, String), issuer As String, audience As String, signingKeyBase64 As String)

            _apiKeyHashes = BuildApiKeyHashes(apiKeys)

            If String.IsNullOrWhiteSpace(issuer) Then Throw New ArgumentException("The JWT issuer is required.", NameOf(issuer))
            If String.IsNullOrWhiteSpace(audience) Then Throw New ArgumentException("The JWT audience is required.", NameOf(audience))

            Dim signingKey = DecodeSigningKey(signingKeyBase64)

            _jwtParameters = New TokenValidationParameters With {
                .RequireSignedTokens = True,
                .ValidateIssuerSigningKey = True,
                .IssuerSigningKey = New SymmetricSecurityKey(signingKey),
                .ValidAlgorithms = New String() {
                    SecurityAlgorithms.HmacSha256
                },
                .ValidateIssuer = True,
                .ValidIssuer = issuer,
                .ValidateAudience = True,
                .RequireAudience = True,
                .ValidAudience = audience,
                .ValidateLifetime = True,
                .RequireExpirationTime = True,
                .ClockSkew = TimeSpan.FromSeconds(30),
                .AuthenticationType = "Bearer",
                .NameClaimType = "name",
                .RoleClaimType = "role"
            }
        End Sub

        ''' <summary>
        ''' Determines whether the supplied API key matches a configured key.
        ''' </summary>
        Public Function TryValidateApiKey(apiKey As String, ByRef keyName As String) As Boolean

            keyName = Nothing

            If String.IsNullOrWhiteSpace(apiKey) OrElse
                apiKey.Length < MinApiKeyLength OrElse
                apiKey.Length > MaxApiKeyLength Then Return False

            Dim receivedHash = ComputeHash(apiKey)

            For Each entry In _apiKeyHashes
                Dim expectedHash = entry.Value
                Dim difference As Integer = 0

                For index As Integer = 0 To expectedHash.Length - 1
                    difference = difference Or
                        (CInt(expectedHash(index)) Xor
                        CInt(receivedHash(index)))
                Next

                If difference = 0 Then keyName = entry.Key
            Next

            Return keyName IsNot Nothing
        End Function

        ''' <summary>
        ''' Validates a JWT and returns its principal on success.
        ''' Returns False for invalid credentials without exposing token details.
        ''' </summary>
        Public Function TryValidateJwt(token As String, ByRef principal As ClaimsPrincipal) As Boolean

            principal = Nothing

            If String.IsNullOrWhiteSpace(token) OrElse token.Length > MaxTokenSize Then Return False

            Dim handler As New JwtSecurityTokenHandler With {
                .MapInboundClaims = False,
                .MaximumTokenSizeInBytes = MaxTokenSize
            }

            Try
                Dim validatedToken As SecurityToken = Nothing
                Dim validatedPrincipal = handler.ValidateToken(
                    token,
                    _jwtParameters,
                    validatedToken)

                If validatedPrincipal Is Nothing OrElse
                   validatedPrincipal.Identity Is Nothing OrElse
                   Not validatedPrincipal.Identity.IsAuthenticated Then Return False

                principal = validatedPrincipal

                Return True

            Catch ex As SecurityTokenException
                Return False
            Catch ex As ArgumentException
                Return False
            End Try
        End Function

        Private Shared Function BuildApiKeyHashes(apiKeys As IReadOnlyDictionary(Of String, String)) As KeyValuePair(Of String, Byte())()

            If apiKeys Is Nothing OrElse apiKeys.Count = 0 Then Throw New ArgumentException("At least one named API key is required.", NameOf(apiKeys))

            Dim entries As New List(Of KeyValuePair(Of String, Byte()))(apiKeys.Count)
            Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim hashes As New HashSet(Of String)(StringComparer.Ordinal)

            For Each entry In apiKeys
                If String.IsNullOrWhiteSpace(entry.Key) Then Throw New ArgumentException("Each API key must have a name.", NameOf(apiKeys))

                Dim name = entry.Key.Trim()
                Dim key = entry.Value

                If Not names.Add(name) Then Throw New ArgumentException("Duplicate API key name: " & name, NameOf(apiKeys))

                If String.IsNullOrWhiteSpace(key) OrElse
                    key.Length < MinApiKeyLength OrElse
                    key.Length > MaxApiKeyLength Then _
                    Throw New ArgumentException("Each API key must contain between 32 and 512 characters.", NameOf(apiKeys))

                Dim hash = ComputeHash(key)

                If Not hashes.Add(Convert.ToBase64String(hash)) Then Throw New ArgumentException("The same API key cannot be assigned to multiple names.", NameOf(apiKeys))

                entries.Add(New KeyValuePair(Of String, Byte())(name, hash))
            Next

            Return entries.ToArray()
        End Function

        Private Shared Function DecodeSigningKey(signingKeyBase64 As String) As Byte()

            If String.IsNullOrWhiteSpace(signingKeyBase64) Then Throw New ArgumentException("The JWT signing key is required.", NameOf(signingKeyBase64))

            Dim signingKey As Byte()

            Try
                signingKey = Convert.FromBase64String(signingKeyBase64)
            Catch ex As FormatException
                Throw New ArgumentException("The JWT signing key must be valid Base64.", NameOf(signingKeyBase64), ex)
            End Try

            If signingKey.Length < MinSigningKeyBytes Then
                Throw New ArgumentException("The JWT signing key must contain at least 32 bytes.", NameOf(signingKeyBase64))
            End If

            Return signingKey
        End Function

        Private Shared Function ComputeHash(value As String) As Byte()
            Using algorithm = SHA256.Create()
                Return algorithm.ComputeHash(
                    Encoding.UTF8.GetBytes(value))
            End Using
        End Function

    End Class

End Namespace