[CmdletBinding()]
param([string]$BaseUrl = 'https://localhost:44380')
$ErrorActionPreference = 'Stop'
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
$BaseUrl = $BaseUrl.TrimEnd('/')
if (!(Test-Path (Join-Path $PSScriptRoot 'ApiVb48\LocalSecrets.config'))) { throw 'Ejecute Setup-Local.ps1 primero.' }
[xml]$config = Get-Content (Join-Path $PSScriptRoot 'ApiVb48\LocalSecrets.config') -Raw
$settings = @{}
foreach ($entry in $config.appSettings.add) { $settings[$entry.key] = $entry.value }
$secure = Read-Host 'Contraseña demo' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try { $password = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
function Assert-Status([string]$Path, [hashtable]$Headers, [int]$Expected) {
    try {
        $r = Invoke-WebRequest "$BaseUrl$Path" -Headers $Headers -UseBasicParsing
        $status = [int]$r.StatusCode
    } catch {
        if (!$_.Exception.Response) { throw }
        $status = [int]$_.Exception.Response.StatusCode
    }
    if ($status -ne $Expected) { throw "$Path : se esperaba $Expected y se recibió $status" }
    Write-Host "OK $Expected $Path"
}
try {
    $keyHeaders = @{ 'X-Api-Key' = $settings.ApiKey }
    $body = @{ Username = $settings.DemoUsername; Password = $password } | ConvertTo-Json
    $login = Invoke-RestMethod "$BaseUrl/api/v1/auth/token" -Method Post -Headers $keyHeaders -Body $body -ContentType 'application/json'
} finally { $password = $null; $body = $null; $secure.Dispose() }
$both = @{ 'X-Api-Key' = $settings.ApiKey; Authorization = "Bearer $($login.access_token)" }
Assert-Status '/api/v1/personas' @{} 401
Assert-Status '/api/v1/personas' $keyHeaders 401
Assert-Status '/api/v1/personas' @{ Authorization = $both.Authorization } 401
Assert-Status '/api/v1/personas' @{ 'X-Api-Key' = 'incorrecta'; Authorization = $both.Authorization } 401
Assert-Status '/api/v1/personas' @{ 'X-Api-Key' = $settings.ApiKey; Authorization = 'Bearer inválido' } 401
Assert-Status '/api/v1/personas' $both 200
Assert-Status '/api/v2/personas' $both 200
Assert-Status '/api/v3/personas' $both 404
# Generar tokens firmados de prueba, pero con claims inválidos, sin paquetes externos.
function Base64Url([byte[]]$Bytes) { return [Convert]::ToBase64String($Bytes).TrimEnd('=').Replace('+','-').Replace('/','_') }
function New-TestToken([string]$Issuer, [string]$Audience, [long]$Expiry) {
    $now = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
    $head = Base64Url ([Text.Encoding]::UTF8.GetBytes('{"alg":"HS256","typ":"JWT"}'))
    $payload = @{ sub = 'test'; iss = $Issuer; aud = $Audience; exp = $Expiry; nbf = ($now - 600) } | ConvertTo-Json -Compress
    $encoded = Base64Url ([Text.Encoding]::UTF8.GetBytes($payload))
    $data = "$head.$encoded"
    $hmac = [Security.Cryptography.HMACSHA256]::new([Convert]::FromBase64String($settings.JwtSigningKeyBase64))
    try { $signature = Base64Url ($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($data))) } finally { $hmac.Dispose() }
    return "$data.$signature"
}
$now = [DateTimeOffset]::UtcNow.ToUnixTimeSeconds()
# Valores por defecto de Web.config. Adapte estos dos valores si cambia issuer/audience.
foreach ($token in @(
    (New-TestToken 'ApiVb48' 'ApiVb48.Client' ($now - 300)),
    (New-TestToken 'emisor-incorrecto' 'ApiVb48.Client' ($now + 300)),
    (New-TestToken 'ApiVb48' 'audiencia-incorrecta' ($now + 300))
)) {
    Assert-Status '/api/v1/personas' @{ 'X-Api-Key' = $settings.ApiKey; Authorization = "Bearer $token" } 401
}
$v1 = Invoke-RestMethod "$BaseUrl/swagger/docs/v1"
$v2 = Invoke-RestMethod "$BaseUrl/swagger/docs/v2"
if (!$v1.paths.'/api/v1/personas' -or $v1.paths.'/api/v2/personas') { throw 'Swagger v1 mezcla versiones.' }
if (!$v2.paths.'/api/v2/personas' -or $v2.paths.'/api/v1/personas') { throw 'Swagger v2 mezcla versiones.' }
$security = @($v1.paths.'/api/v1/personas'.get.security)
if ($security.Count -ne 1 -or !$security[0].PSObject.Properties['ApiKey'] -or !$security[0].PSObject.Properties['Bearer']) { throw 'Swagger debe exigir ApiKey AND Bearer.' }
$loginSecurity = @($v1.paths.'/api/v1/auth/token'.post.security)
if ($loginSecurity.Count -ne 1 -or !$loginSecurity[0].PSObject.Properties['ApiKey'] -or $loginSecurity[0].PSObject.Properties['Bearer']) { throw 'Login debe exigir solo API key.' }
Write-Host 'Pruebas terminadas: credenciales, versiones y documentos Swagger.'
