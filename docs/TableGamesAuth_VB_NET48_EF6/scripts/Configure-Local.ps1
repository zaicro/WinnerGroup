param(
    [ValidateSet('SqlServer','Postgres')] [string] $Provider = 'SqlServer',
    [string] $ConnectionString
)
$ErrorActionPreference = 'Stop'
# Dot-source this script to keep TG_CLIENT_API_KEY available as well.
if (-not $ConnectionString) {
    if ($Provider -eq 'SqlServer') {
        $ConnectionString = 'Server=localhost;Database=TableGamesAuth;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'
    } else {
        throw 'Indique -ConnectionString para Postgres. Use credenciales de desarrollo.'
    }
}
function New-RandomKey {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes); [Convert]::ToBase64String($bytes) } finally { $rng.Dispose() }
}
$env:TG_DB_PROVIDER = $Provider
$env:TG_DB_CONNECTION = $ConnectionString
$env:TG_LISTEN_URL = 'http://localhost:9000/'
$env:TG_ALLOW_INSECURE_LOOPBACK = 'true'
$env:TG_JWT_KEY = New-RandomKey
$env:TG_CLIENT_API_KEY = New-RandomKey
$sha = [System.Security.Cryptography.SHA256]::Create()
try { $digest = -join ($sha.ComputeHash([Text.Encoding]::UTF8.GetBytes($env:TG_CLIENT_API_KEY)) | ForEach-Object { $_.ToString('x2') }) } finally { $sha.Dispose() }
$env:TG_API_KEYS = ConvertTo-Json -Compress -InputObject @(@{ appId = '20000000-0000-0000-0000-000000000001'; sha256 = $digest })
Write-Host 'Configurado solo para esta sesión de desarrollo. TG_CLIENT_API_KEY contiene la clave del cliente.'
Write-Host 'Volver a ejecutar este script rota las claves e invalida las sesiones existentes.'
