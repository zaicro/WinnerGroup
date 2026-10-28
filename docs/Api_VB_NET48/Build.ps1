[CmdletBinding()]
param([ValidateSet('Debug','Release')][string]$Configuration = 'Debug')
$ErrorActionPreference = 'Stop'
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
if (!(Test-Path $vswhere)) { throw 'Instale Visual Studio con ASP.NET y desarrollo web y el targeting pack de .NET Framework 4.8.' }
$msbuild = & $vswhere -latest -products '*' -requires Microsoft.Component.MSBuild -find 'MSBuild\**\Bin\MSBuild.exe' | Select-Object -First 1
if (!$msbuild) { throw 'No se encontró MSBuild.' }
& $msbuild (Join-Path $PSScriptRoot 'ApiVb48.sln') /restore /t:Build /m "/p:Configuration=$Configuration" /nologo
if ($LASTEXITCODE -ne 0) { throw 'La compilación falló.' }
# ASP.NET clásico lee Web.config, no ApiVb48.dll.config.
$generated = Join-Path $PSScriptRoot 'ApiVb48\bin\ApiVb48.dll.config'
$webPath = Join-Path $PSScriptRoot 'ApiVb48\Web.config'
if (Test-Path $generated) {
    [xml]$source = Get-Content -LiteralPath $generated -Raw
    $bindings = $source.SelectSingleNode("/*[local-name()='configuration']/*[local-name()='runtime']/*[local-name()='assemblyBinding']")
    if ($bindings) {
        [xml]$web = Get-Content -LiteralPath $webPath -Raw
        $runtime = $web.configuration.SelectSingleNode('runtime')
        if (!$runtime) { $runtime = $web.CreateElement('runtime'); [void]$web.configuration.AppendChild($runtime) }
        $existing = $runtime.SelectSingleNode("*[local-name()='assemblyBinding']")
        if ($existing) { [void]$runtime.RemoveChild($existing) }
        [void]$runtime.AppendChild($web.ImportNode($bindings, $true))
        $web.Save($webPath)
        Write-Host 'Binding redirects aplicados a Web.config.'
    }
}
Write-Host 'Compilación terminada. Abra la solución e inicie IIS Express con HTTPS.'
