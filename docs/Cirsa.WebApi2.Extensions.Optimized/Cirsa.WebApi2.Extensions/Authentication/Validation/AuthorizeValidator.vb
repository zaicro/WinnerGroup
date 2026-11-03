Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration
Imports Cirsa.WebApi2.Extensions.Authentication.Internal
Imports System.Text
Imports Microsoft.IdentityModel.Tokens

Namespace Authentication.Validation

    ''' <summary>
    ''' Validates configured API keys and signed JWT bearer tokens.
    ''' </summary>
    Public NotInheritable Class AuthorizeValidator

        Private Const MinApiKeyLength As Integer = 32
        Private Const MaxApiKeyLength As Integer = 512
        Friend Const MaxTokenSize As Integer = 8192

        Private ReadOnly _apiKeyHashes As KeyValuePair(Of String, Byte())()
        Private ReadOnly _jwtParameters As TokenValidationParameters

        ''' <summary>Gets whether named API keys are enabled.</summary>
        Public ReadOnly Property SupportsApiKey As Boolean

        ''' <summary>Gets whether JWT validation is enabled.</summary>
        Public ReadOnly Property SupportsJwt As Boolean

        ''' <summary>Preserves the original constructor with both mechanisms enabled.</summary>
        Public Sub New(apiKeys As IReadOnlyDictionary(Of String, String), issuer As String, audience As String, signingKeyBase64 As String)
            Me.New(New ApiAuthenticationOptions With {
                .ApiKeys = apiKeys, .Issuer = issuer, .Audience = audience, .SigningKeyBase64 = signingKeyBase64})
        End Sub

        ''' <summary>Validates and snapshots the enabled mechanisms at construction time.</summary>
        Public Sub New(options As ApiAuthenticationOptions)
            If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
            If Not options.EnableApiKey AndAlso Not options.EnableJwt Then
                Throw New ArgumentException("Enable at least one authentication mechanism, or do not register authentication.", NameOf(options))
            End If
            SupportsApiKey = options.EnableApiKey
            SupportsJwt = options.EnableJwt
            _apiKeyHashes = If(SupportsApiKey, BuildApiKeyHashes(options.ApiKeys), New KeyValuePair(Of String, Byte())() {})
            If SupportsJwt Then
                If String.IsNullOrWhiteSpace(options.Issuer) Then Throw New ArgumentException("The JWT issuer is required.", NameOf(options))
                If String.IsNullOrWhiteSpace(options.Audience) Then Throw New ArgumentException("The JWT audience is required.", NameOf(options))
                _jwtParameters = New TokenValidationParameters With {
                    .RequireSignedTokens = True,
                    .ValidateIssuerSigningKey = True,
                    .IssuerSigningKey = CredentialUtilities.DecodeSigningKey(options.SigningKeyBase64),
                    .ValidAlgorithms = New String() {SecurityAlgorithms.HmacSha256},
                    .ValidateIssuer = True, .ValidIssuer = options.Issuer,
                    .ValidateAudience = True, .RequireAudience = True, .ValidAudience = options.Audience,
                    .ValidateLifetime = True, .RequireExpirationTime = True,
                    .ClockSkew = TimeSpan.FromSeconds(30),
                    .AuthenticationType = "Bearer", .NameClaimType = "name", .RoleClaimType = "role"}
            End If
        End Sub

        ''' <summary>Rejects policies referring to mechanisms not enabled by the host.</summary>
        Public Sub EnsureModeConfigured(mode As AuthenticationMode)
            If Not [Enum].IsDefined(GetType(AuthenticationMode), mode) Then Throw New InvalidOperationException("Unsupported authentication mode.")
            If (mode = AuthenticationMode.ApiKey OrElse mode = AuthenticationMode.Both OrElse mode = AuthenticationMode.Either) AndAlso Not SupportsApiKey Then
                Throw New InvalidOperationException("An endpoint requires API key authentication, but it is not enabled.")
            End If
            If (mode = AuthenticationMode.Jwt OrElse mode = AuthenticationMode.Both OrElse mode = AuthenticationMode.Either) AndAlso Not SupportsJwt Then
                Throw New InvalidOperationException("An endpoint requires JWT authentication, but it is not enabled.")
            End If
        End Sub

        ''' <summary>
        ''' Determines whether the supplied API key matches a configured key.
        ''' </summary>
        Public Function TryValidateApiKey(apiKey As String, ByRef keyName As String) As Boolean

            keyName = Nothing

            If Not SupportsApiKey OrElse String.IsNullOrWhiteSpace(apiKey) OrElse
                apiKey.Length < MinApiKeyLength OrElse
                apiKey.Length > MaxApiKeyLength Then Return False

            Dim receivedHash As Byte()
            Try
                receivedHash = CredentialUtilities.ComputeHash(apiKey)
            Catch ex As EncoderFallbackException
                Return False
            End Try

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

            If Not SupportsJwt OrElse String.IsNullOrWhiteSpace(token) OrElse token.Length > MaxTokenSize Then Return False

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

                Dim hash = CredentialUtilities.ComputeHash(key)

                If Not hashes.Add(Convert.ToBase64String(hash)) Then Throw New ArgumentException("The same API key cannot be assigned to multiple names.", NameOf(apiKeys))

                entries.Add(New KeyValuePair(Of String, Byte())(name, hash))
            Next

            Return entries.ToArray()
        End Function

    End Class
End Namespace
