param([string] $UserName = 'admin')
$ErrorActionPreference = 'Stop'
$exe = Join-Path $PSScriptRoot '../src/Api/bin/Release/net48/TableGamesAuth.Api.exe'
if (-not (Test-Path $exe)) { throw 'Compile primero en Release.' }
& $exe --migrate
if ($LASTEXITCODE -ne 0) { throw 'Falló la migración.' }
$password = Read-Host 'Contraseña inicial (12 a 128 caracteres)' -AsSecureString
$pointer = [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password)
try {
    $env:TG_BOOTSTRAP_USER = $UserName
    $env:TG_BOOTSTRAP_PASSWORD = [Runtime.InteropServices.Marshal]::PtrToStringBSTR($pointer)
    & $exe --bootstrap-admin
    if ($LASTEXITCODE -ne 0) { throw 'Falló bootstrap. Solo se permite en una base sin usuarios.' }
} finally {
    [Runtime.InteropServices.Marshal]::ZeroFreeBSTR($pointer)
    Remove-Item Env:TG_BOOTSTRAP_PASSWORD -ErrorAction SilentlyContinue
    Remove-Item Env:TG_BOOTSTRAP_USER -ErrorAction SilentlyContinue
    $password.Dispose()
}
