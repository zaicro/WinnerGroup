$ErrorActionPreference = "Stop"
Push-Location $PSScriptRoot
try {
    dotnet build .\Cirsa.WebApi2.Extensions.sln -c Release
    if ($LASTEXITCODE -ne 0) { throw "Build failed." }
    & .\tests\Cirsa.WebApi2.Extensions.Tests\bin\Release\net48\Cirsa.WebApi2.Extensions.Tests.exe
    if ($LASTEXITCODE -ne 0) { throw "Authentication tests failed." }
    if ((Get-Command npm -ErrorAction SilentlyContinue) -and (Get-Command node -ErrorAction SilentlyContinue)) {
        Push-Location .\tests\ui
        try {
            npm ci --ignore-scripts --no-audit --no-fund
            if ($LASTEXITCODE -ne 0) { throw "UI test dependencies failed." }
            npm test
            if ($LASTEXITCODE -ne 0) { throw "UI tests failed." }
        } finally { Pop-Location }
    } else {
        Write-Warning "Node/npm unavailable. UI tests were not executed."
    }
    Write-Host "Validation completed. Package: bin\packages\Release\Cirsa.WebApi2.Extensions.0.1.1.nupkg"
} finally { Pop-Location }
