[CmdletBinding()]
param([switch]$IncludeAzure,[switch]$Test,[ValidateSet('Debug','Release')][string]$Configuration='Debug')
$ErrorActionPreference='Stop'
$vswhere=Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (!(Test-Path $vswhere)) { throw 'Instale Visual Studio 2022 o Build Tools con VB y el targeting pack de .NET Framework 4.8.' }
$msbuild=& $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (!$msbuild) { throw 'MSBuild no encontrado.' }
$target=if ($IncludeAzure) { Join-Path $PSScriptRoot 'SensitiveData48.sln' } else { Join-Path $PSScriptRoot 'src\Demo.Local\Demo.Local.vbproj' }
& $msbuild $target /restore /t:Build /m /nologo "/p:Configuration=$Configuration"
if ($LASTEXITCODE -ne 0) { throw 'Compilación fallida.' }
if ($Test) {
    & (Join-Path $PSScriptRoot "src\Demo.Local\bin\$Configuration\Demo.Local.exe") self-test
    if ($LASTEXITCODE -ne 0) { throw 'Pruebas fallidas.' }
}
Write-Host 'Compilación terminada. Conserve el EXE.config y las DLL de la carpeta de salida al distribuir.'
