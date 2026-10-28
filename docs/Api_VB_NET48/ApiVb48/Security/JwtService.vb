Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports Microsoft.IdentityModel.Tokens

Namespace Security
    Public Module JwtService
        Public Function CreateToken(username As String) As String
            Dim now = DateTime.UtcNow
            Dim claims = New Claim() {
                New Claim(JwtRegisteredClaimNames.Sub, username),
                New Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            }
            Dim credentials = New SigningCredentials(New SymmetricSecurityKey(AuthSettings.SigningKey()), SecurityAlgorithms.HmacSha256)
            Dim token = New JwtSecurityToken(AuthSettings.Read("JwtIssuer"), AuthSettings.Read("JwtAudience"),
                                            claims, now, now.AddMinutes(15), credentials)
            Return New JwtSecurityTokenHandler().WriteToken(token)
        End Function

        Public Function ValidateToken(rawToken As String) As ClaimsPrincipal
            Dim handler = New JwtSecurityTokenHandler() With {.MapInboundClaims = False, .MaximumTokenSizeInBytes = 8192}
            Dim parameters = New TokenValidationParameters With {
                .ValidateIssuer = True, .ValidIssuer = AuthSettings.Read("JwtIssuer"),
                .ValidateAudience = True, .ValidAudience = AuthSettings.Read("JwtAudience"),
                .ValidateLifetime = True, .RequireExpirationTime = True,
                .RequireSignedTokens = True, .ValidateIssuerSigningKey = True,
                .IssuerSigningKey = New SymmetricSecurityKey(AuthSettings.SigningKey()),
                .ValidAlgorithms = New String() {SecurityAlgorithms.HmacSha256},
                .ClockSkew = TimeSpan.FromSeconds(30), .NameClaimType = JwtRegisteredClaimNames.Sub
            }
            Dim validated As SecurityToken = Nothing
            Return handler.ValidateToken(rawToken, parameters, validated)
        End Function
    End Module
End Namespace
