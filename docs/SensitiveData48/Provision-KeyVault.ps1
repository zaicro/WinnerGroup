# Operación manual para un operador autorizado. El proyecto no ejecuta este script automáticamente.
# Requiere Az.Accounts y Az.KeyVault. Autentíquese con Connect-AzAccount en la suscripción correcta.
[CmdletBinding()]
param([Parameter(Mandatory=$true)][string]$VaultName)
$ErrorActionPreference='Stop'
Import-Module Az.KeyVault -ErrorAction Stop
$names=@('jwt-signing-v1','hmac-v1')
# Lista solo metadatos. Un error de permisos detiene el script; nunca se trata como "secreto ausente".
$existing=@(Get-AzKeyVaultSecret -VaultName $VaultName -ErrorAction Stop | ForEach-Object { $_.Name })
foreach($name in $names) {
    if($existing -contains $name) { throw 'Ya existe al menos una llave v1. No se sobrescribe. Use nombres nuevos para rotar.' }
}
function New-KeyBase64 {
    $bytes=New-Object byte[] 32
    $rng=[Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes); return [Convert]::ToBase64String($bytes) }
    finally { $rng.Dispose(); [Array]::Clear($bytes,0,$bytes.Length) }
}
foreach($name in $names) {
    $plain=New-KeyBase64
    $secure=ConvertTo-SecureString $plain -AsPlainText -Force
    try {
        Set-AzKeyVaultSecret -VaultName $VaultName -Name $name -SecretValue $secure -ContentType 'application/vnd.sensitivedata48.key-base64' -ErrorAction Stop | Out-Null
    } finally { $plain=$null; $secure.Dispose() }
}
Write-Host 'Llaves v1 independientes creadas. No se imprimieron sus valores.'
Write-Host 'Este script no es transaccional: ante un fallo, revise los nombres creados antes de repetirlo.'
