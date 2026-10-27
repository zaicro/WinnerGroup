Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports Microsoft.IdentityModel.Tokens
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.Security
    Public Class JwtTokenService
        Implements ITokenService
        Private ReadOnly signingKey As SymmetricSecurityKey
        Private ReadOnly issuer As String
        Private ReadOnly audience As String
        Public Sub New(keyBase64 As String, issuer As String, audience As String)
            Dim bytes = Convert.FromBase64String(keyBase64)
            If bytes.Length < 32 Then Throw New InvalidOperationException("JWT key requiere al menos 32 bytes aleatorios.")
            signingKey = New SymmetricSecurityKey(bytes)
            Me.issuer = issuer : Me.audience = audience
        End Sub
        Public Function Issue(user As User, role As String, appId As Guid) As LoginResult Implements ITokenService.Issue
            Dim now = DateTime.UtcNow
            Dim expires = now.AddMinutes(15)
            Dim claims = New Claim() {
                New Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), New Claim(ClaimTypes.Name, user.UserName),
                New Claim(ClaimTypes.Role, role), New Claim("stamp", user.SecurityStamp.ToString()),
                New Claim("app_id", appId.ToString()), New Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())}
            Dim jwt As New JwtSecurityToken(issuer, audience, claims, now, expires, New SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256))
            Return New LoginResult With {.AccessToken = New JwtSecurityTokenHandler().WriteToken(jwt), .ExpiresAtUtc = expires}
        End Function
        Public Function Validate(token As String) As ClaimsPrincipal Implements ITokenService.Validate
            Dim parameters As New TokenValidationParameters With {
                .ValidateIssuerSigningKey = True, .IssuerSigningKey = signingKey,
                .ValidateIssuer = True, .ValidIssuer = issuer, .ValidateAudience = True, .ValidAudience = audience,
                .ValidateLifetime = True, .RequireExpirationTime = True, .RequireSignedTokens = True,
                .ValidAlgorithms = New String() {SecurityAlgorithms.HmacSha256}, .ClockSkew = TimeSpan.FromSeconds(30)}
            Dim validated As SecurityToken = Nothing
            Dim handler As New JwtSecurityTokenHandler With {.MaximumTokenSizeInBytes = 8192}
            Return handler.ValidateToken(token, parameters, validated)
        End Function
    End Class
End Namespace
