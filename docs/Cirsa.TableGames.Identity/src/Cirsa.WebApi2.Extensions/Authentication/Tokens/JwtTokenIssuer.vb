Imports System.Globalization
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports Microsoft.IdentityModel.Tokens

Namespace Authentication.Tokens

    ''' <summary>
    ''' Issues signed JWTs for authenticated users and their application roles.
    ''' </summary>
    Public NotInheritable Class JwtTokenIssuer

        Private ReadOnly _issuer As String
        Private ReadOnly _audience As String
        Private ReadOnly _lifetime As TimeSpan
        Private ReadOnly _signingCredentials As SigningCredentials
        Private Shared ReadOnly ReservedClaimTypes As New HashSet(Of String)(
            StringComparer.Ordinal) From {
                JwtRegisteredClaimNames.Sub,
                JwtRegisteredClaimNames.Jti,
                JwtRegisteredClaimNames.Iat,
                JwtRegisteredClaimNames.Iss,
                JwtRegisteredClaimNames.Aud,
                JwtRegisteredClaimNames.Nbf,
                JwtRegisteredClaimNames.Exp
            }

        Public Sub New(issuer As String, audience As String, signingKeyBase64 As String, lifetimeMinutes As Integer)

            If String.IsNullOrWhiteSpace(issuer) Then Throw New ArgumentException("The JWT issuer is required.", NameOf(issuer))
            If String.IsNullOrWhiteSpace(audience) Then Throw New ArgumentException("The JWT audience is required.", NameOf(audience))
            If String.IsNullOrWhiteSpace(signingKeyBase64) Then Throw New ArgumentException("The JWT signing key is required.", NameOf(signingKeyBase64))
            ' Local policy: access tokens cannot exceed 24 hours.
            If lifetimeMinutes < 1 OrElse lifetimeMinutes > 1440 Then Throw New ArgumentOutOfRangeException(NameOf(lifetimeMinutes), "The JWT lifetime must be between 1 and 1440 minutes.")

            Dim signingKey As Byte()

            Try
                signingKey = Convert.FromBase64String(signingKeyBase64)
            Catch ex As FormatException
                Throw New ArgumentException("The JWT signing key must be valid Base64.", NameOf(signingKeyBase64), ex)
            End Try

            If signingKey.Length < 32 Then Throw New ArgumentException("The JWT signing key must contain at least 32 bytes.", NameOf(signingKeyBase64))

            _issuer = issuer
            _audience = audience
            _lifetime = TimeSpan.FromMinutes(lifetimeMinutes)

            _signingCredentials = New SigningCredentials(New SymmetricSecurityKey(signingKey), SecurityAlgorithms.HmacSha256)
        End Sub

        ''' <summary>
        ''' Creates a token from a successfully authenticated user.
        ''' Roles must belong to the specified application.
        ''' </summary>
        Public Function Create(subject As String, Optional additionalClaims As IEnumerable(Of Claim) = Nothing) As IssuedJwtToken

            If String.IsNullOrWhiteSpace(subject) Then Throw New ArgumentException("The token subject is required.", NameOf(subject))

            Dim now = DateTime.UtcNow
            Dim claims As New List(Of Claim) From {
                New Claim(JwtRegisteredClaimNames.Sub, subject),
                New Claim(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString("N")),
                    New Claim(
                        JwtRegisteredClaimNames.Iat,
                        EpochTime.GetIntDate(now).ToString(CultureInfo.InvariantCulture),
                        ClaimValueTypes.Integer64)
            }

            If additionalClaims IsNot Nothing Then
                For Each claim In additionalClaims
                    If claim Is Nothing Then Throw New ArgumentException("The claims collection must not contain null entries.", NameOf(additionalClaims))
                    If ReservedClaimTypes.Contains(claim.Type) Then Throw New ArgumentException("The claim is managed by the token issuer: " & claim.Type, NameOf(additionalClaims))
                    claims.Add(claim)
                Next
            End If

            Dim token As New JwtSecurityToken(
                issuer:=_issuer,
                audience:=_audience,
                claims:=claims,
                notBefore:=now,
                expires:=now.Add(_lifetime),
                signingCredentials:=_signingCredentials)

            Dim handler As New JwtSecurityTokenHandler()

            Return New IssuedJwtToken(handler.WriteToken(token), token.ValidTo)
        End Function
    End Class
End Namespace