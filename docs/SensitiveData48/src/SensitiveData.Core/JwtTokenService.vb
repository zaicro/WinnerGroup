Imports System.Collections.Generic
Imports System.IdentityModel.Tokens.Jwt
Imports System.Security.Claims
Imports System.Text.RegularExpressions
Imports Microsoft.IdentityModel.Tokens

' Ejemplo HS256 para un emisor y sus validadores de confianza.
' Quien posee la llave HS256 también puede emitir tokens.
Public NotInheritable Class JwtTokenService
    Private ReadOnly _store As ISecretStore
    Private ReadOnly _issuer As String
    Private ReadOnly _audience As String
    Private ReadOnly _currentKid As String
    Private ReadOnly _accepted As HashSet(Of String)
    Public Sub New(store As ISecretStore, issuer As String, audience As String, currentKid As String, acceptedKids As IEnumerable(Of String))
        If String.IsNullOrWhiteSpace(issuer) OrElse String.IsNullOrWhiteSpace(audience) Then Throw New ArgumentException("Configure issuer y audience.")
        _store = store
        _issuer = issuer
        _audience = audience
        _currentKid = currentKid
        _accepted = New HashSet(Of String)(acceptedKids, StringComparer.Ordinal)
        If Not _accepted.Contains(currentKid) OrElse _accepted.Count > 5 Then Throw New ArgumentException("Key ring inválido.")
        For Each kid In _accepted
            If Not Regex.IsMatch(kid, "\A[a-zA-Z0-9-]{1,32}\z") Then Throw New ArgumentException("kid inválido.")
        Next
    End Sub
    Private Function LoadKey(kid As String) As SymmetricSecurityKey
        Return New SymmetricSecurityKey(ReadKey(_store, "jwt-signing-" & kid)) With {.KeyId = kid}
    End Function
    Public Function Issue(subject As String, Optional lifetime As TimeSpan = Nothing) As String
        If String.IsNullOrWhiteSpace(subject) Then Throw New ArgumentException("Subject requerido.")
        If lifetime = TimeSpan.Zero Then lifetime = TimeSpan.FromMinutes(15)
        If lifetime <= TimeSpan.Zero OrElse lifetime > TimeSpan.FromMinutes(15) Then Throw New ArgumentException("Duración máxima: 15 minutos.")
        Dim now = DateTime.UtcNow
        Dim credentials As New SigningCredentials(LoadKey(_currentKid), SecurityAlgorithms.HmacSha256)
        Dim token As New JwtSecurityToken(_issuer, _audience,
            New Claim() {New Claim(JwtRegisteredClaimNames.Sub, subject), New Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))},
            now, now.Add(lifetime), credentials)
        Return New JwtSecurityTokenHandler().WriteToken(token)
    End Function
    Public Function Validate(token As String) As ClaimsPrincipal
        If String.IsNullOrWhiteSpace(token) OrElse token.Length > 8192 Then Throw New ArgumentException("JWT ausente o demasiado largo.")
        Dim parameters As New TokenValidationParameters With {
            .ValidateIssuer = True, .ValidIssuer = _issuer,
            .ValidateAudience = True, .ValidAudience = _audience,
            .ValidateLifetime = True, .RequireExpirationTime = True,
            .RequireSignedTokens = True, .ValidateIssuerSigningKey = True,
            .ValidAlgorithms = New String() {SecurityAlgorithms.HmacSha256},
            .ClockSkew = TimeSpan.FromSeconds(30), .NameClaimType = JwtRegisteredClaimNames.Sub,
            .IssuerSigningKeyResolver = AddressOf ResolveKeys
        }
        Dim validated As SecurityToken = Nothing
        Dim handler As New JwtSecurityTokenHandler() With {.MapInboundClaims = False, .MaximumTokenSizeInBytes = 8192}
        Return handler.ValidateToken(token, parameters, validated)
    End Function
    Private Function ResolveKeys(token As String, securityToken As SecurityToken, kid As String, parameters As TokenValidationParameters) As IEnumerable(Of SecurityKey)
        ' kid viene de una cabecera no verificada: solo se admite la lista configurada.
        ' No se convierten valores arbitrarios del token en rutas o consultas a Key Vault.
        If kid Is Nothing OrElse Not _accepted.Contains(kid) Then Return New SecurityKey() {}
        Return New SecurityKey() {LoadKey(kid)}
    End Function
End Class
