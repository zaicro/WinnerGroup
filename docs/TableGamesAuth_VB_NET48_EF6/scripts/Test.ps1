param([switch] $Integration)
$ErrorActionPreference = 'Stop'
$root = Join-Path $PSScriptRoot '..'
dotnet build (Join-Path $root 'TableGamesAuth.sln') -c Release
if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación.' }
& (Join-Path $root 'tests/TableGamesAuth.Tests/bin/Release/net48/TableGamesAuth.Tests.exe')
if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas unitarias.' }
if ($Integration) {
    & (Join-Path $root 'tests/TableGamesAuth.IntegrationTests/bin/Release/net48/TableGamesAuth.IntegrationTests.exe')
    if ($LASTEXITCODE -ne 0) { throw 'Fallaron las pruebas de integración.' }
}
