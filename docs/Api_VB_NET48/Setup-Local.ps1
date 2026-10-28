# Ejecutar en Windows PowerShell 5.1 con .NET Framework 4.8.
[CmdletBinding()]
param([string]$Username = 'demo')
$ErrorActionPreference = 'Stop'
$target = Join-Path $PSScriptRoot 'ApiVb48\LocalSecrets.config'
if (Test-Path $target) { throw 'LocalSecrets.config ya existe. Consérvelo o retírelo manualmente para regenerarlo.' }
if ([string]::IsNullOrWhiteSpace($Username) -or $Username.Length -gt 100) { throw 'Usuario inválido.' }
function New-RandomBase64([int]$size) {
    $bytes = New-Object byte[] $size
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes); return [Convert]::ToBase64String($bytes) } finally { $rng.Dispose() }
}
$secure = Read-Host 'Contraseña demo (12 a 256 caracteres)' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($secure)
try { $plain = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer) }
finally { [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer) }
try {
    if ($plain.Length -lt 12 -or $plain.Length -gt 256) { throw 'Longitud inválida.' }
    $salt = New-RandomBase64 16
    $pbkdf = [Security.Cryptography.Rfc2898DeriveBytes]::new($plain, [Convert]::FromBase64String($salt), 600000, [Security.Cryptography.HashAlgorithmName]::SHA256)
    try { $hash = [Convert]::ToBase64String($pbkdf.GetBytes(32)) } finally { $pbkdf.Dispose() }
} finally { $plain = $null; $secure.Dispose() }
$values = [ordered]@{
    ApiKey = (New-RandomBase64 32)
    JwtSigningKeyBase64 = (New-RandomBase64 32)
    DemoUsername = $Username
    DemoPasswordSalt = $salt
    DemoPasswordHash = $hash
    DemoLoginEnabled = 'true'
    SwaggerEnabled = 'true'
}
$doc = New-Object System.Xml.XmlDocument
$root = $doc.CreateElement('appSettings')
[void]$doc.AppendChild($root)
foreach ($entry in $values.GetEnumerator()) {
    $node = $doc.CreateElement('add')
    $node.SetAttribute('key', $entry.Key)
    $node.SetAttribute('value', $entry.Value)
    [void]$root.AppendChild($node)
}
$doc.Save($target)
Write-Host 'Configuración local creada. Consulte ApiKey en ApiVb48/LocalSecrets.config para probar Swagger.'
Write-Host 'No comparta ni suba ese archivo a Git. Reinicie IIS Express si ya estaba abierto.'
