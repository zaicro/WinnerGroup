# Limpia la terminal
cls

# Moverse a la carpeta del proyecto
Set-Location "D:\Newshore\so-dmt\NS.DMT.Booking - hk"

# Apagar servidores de compilación de .NET
dotnet build-server shutdown

# Cerrar procesos VBCSCompiler.exe (pero sin cerrar Visual Studio si está ejecutándose)
Write-Host "Apagando VBCSCompiler si está en ejecución..."
try {
    taskkill /F /IM VBCSCompiler.exe 2>$null | Out-Null
} catch {}
Get-Process -Name VBCSCompiler -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue


# Buscar y eliminar carpetas bin y obj
$folders = Get-ChildItem -Path . -Directory -Recurse | Where-Object { $_.Name -match "^(bin|obj)$" }

if ($folders) {
    $deleted = 0
    foreach ($folder in $folders) {
        if (Test-Path $folder.FullName) {
            $maxAttempts = 3
            $attempt = 0
            $success = $false

            do {
                try {
                    Remove-Item -Path $folder.FullName -Recurse -Force -ErrorAction Stop
                    $success = $true
                    $deleted++
                } catch {
                    Start-Sleep -Milliseconds 500
                    $attempt++
                    if ($attempt -eq $maxAttempts) {
                        Write-Warning "⚠️ No se pudo eliminar: $($folder.FullName)"
                    }
                }
            } while (-not $success -and $attempt -lt $maxAttempts)
        }
    }

    Write-Host "✅ Se eliminaron $deleted carpetas 'bin' y 'obj'." -ForegroundColor Green
} else {
    Write-Host "⚠️ No se encontraron carpetas 'bin' u 'obj' para eliminar." -ForegroundColor Yellow
}
