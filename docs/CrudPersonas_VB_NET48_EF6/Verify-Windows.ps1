# Ejecutar desde Developer PowerShell for Visual Studio 2022.
# Crea y elimina exclusivamente una base temporal con nombre aleatorio.
$ErrorActionPreference = 'Stop'
$root = $PSScriptRoot
$work = Join-Path ([IO.Path]::GetTempPath()) ('CrudPersonasSmoke_' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $work | Out-Null
$catalog = 'CrudPersonasSmoke_' + [Guid]::NewGuid().ToString('N')
$cleanupOk = $true
try {
    $msbuild = Get-Command msbuild.exe -ErrorAction Stop
    & $msbuild.Source (Join-Path $root 'CrudPersonas.sln') /restore /t:Build /p:Configuration=Debug /verbosity:minimal
    if ($LASTEXITCODE -ne 0) { throw 'La compilación falló.' }
    $output = Join-Path $root 'src\UI\bin\Debug\net48'
    Copy-Item (Join-Path $output '*') $work -Recurse
    [xml]$config = Get-Content (Join-Path $output 'CrudPersonas.UI.exe.config')
    $builder = New-Object System.Data.SqlClient.SqlConnectionStringBuilder
    $builder.DataSource = '(LocalDB)\MSSQLLocalDB'
    $builder.InitialCatalog = $catalog
    $builder.AttachDBFilename = Join-Path $work 'Smoke.mdf'
    $builder.IntegratedSecurity = $true
    $config.configuration.connectionStrings.add.connectionString = $builder.ConnectionString
    $config.Save((Join-Path $work 'Smoke.exe.config'))
    $source = @'
using System;
using System.Linq;
using CrudPersonas.Application;
using CrudPersonas.DataAccess;
using System.Data.Entity.Migrations;

public static class Smoke {
    static void Check(bool ok, string message) {
        if (!ok) throw new Exception(message);
    }
    public static int Main() {
        try {
            DatabaseInitializer.Actualizar();
            var migrations = new DbMigrator(new CrudPersonas.DataAccess.Migrations.Configuration());
            Check(migrations.GetDatabaseMigrations().Count() == 1, "Debe existir una migración inicial.");
            Check(!migrations.GetPendingMigrations().Any(), "Quedan migraciones pendientes.");
            using (var context = new PersonasContext()) {
                Check(context.Database.CompatibleWithModel(false), "El snapshot inicial no coincide con el modelo.");
            }
            var service = new PersonaService(new PersonaRepository());
            Check(service.Listar().Count == 0, "La base temporal debe estar vacía.");
            bool rejected = false;
            try { service.Crear("   "); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "No rechazó un nombre vacío.");
            rejected = false;
            try { service.Crear(new string('X', 151)); } catch (ArgumentException) { rejected = true; }
            Check(rejected, "No rechazó un nombre demasiado largo.");
            int id = service.Crear("  Andrés Sierra  ");
            Check(id > 0, "No generó el Id.");
            Check(service.Listar().Single().Nombre == "Andrés Sierra", "No normalizó el nombre.");
            service.Editar(id, "Ana María");
            Check(service.Listar().Single().Id == id, "Cambió el Id al editar.");
            Check(service.Listar().Single().Nombre == "Ana María", "No guardó la edición.");
            DatabaseInitializer.Actualizar();
            Check(service.Listar().Single().Nombre == "Ana María", "No conservó los datos al reinicializar.");
            Check(migrations.GetDatabaseMigrations().Count() == 1, "Repitió la migración.");
            service.Eliminar(id);
            Check(service.Listar().Count == 0, "No eliminó la persona.");
            Console.WriteLine("OK: migración, modelo, validación, CRUD y persistencia.");
            return 0;
        } catch (Exception error) {
            Console.Error.WriteLine(error);
            return 1;
        }
    }
}
'@
    $sourcePath = Join-Path $work 'Smoke.cs'
    [IO.File]::WriteAllText($sourcePath, $source, [Text.Encoding]::UTF8)
    $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
    if (-not (Test-Path $csc)) { $csc = Join-Path $env:WINDIR 'Microsoft.NET\Framework\v4.0.30319\csc.exe' }
    $references = @('CrudPersonas.Domain.dll', 'CrudPersonas.Application.dll', 'CrudPersonas.DataAccess.dll', 'EntityFramework.dll') |
        ForEach-Object { '/reference:' + (Join-Path $work $_) }
    & $csc /nologo /target:exe ('/out:' + (Join-Path $work 'Smoke.exe')) @references $sourcePath
    if ($LASTEXITCODE -ne 0) { throw 'Falló la compilación del verificador.' }
    & (Join-Path $work 'Smoke.exe')
    if ($LASTEXITCODE -ne 0) { throw 'Falló la comprobación de integración.' }
}
finally {
    # El nombre contiene únicamente prefijo fijo y hexadecimal generado aquí.
    try {
        $connection = New-Object System.Data.SqlClient.SqlConnection('Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True')
        try {
            $connection.Open()
            $command = $connection.CreateCommand()
            $command.CommandText = "IF DB_ID(@name) IS NOT NULL BEGIN ALTER DATABASE [$catalog] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [$catalog]; END"
            $command.Parameters.AddWithValue('@name', $catalog) | Out-Null
            $command.ExecuteNonQuery() | Out-Null
        }
        finally { $connection.Dispose() }
    }
    catch {
        $cleanupOk = $false
        Write-Warning "No se pudo limpiar el catálogo temporal $catalog. Archivos: $work. $($_.Exception.Message)"
    }
    if ($cleanupOk) { Remove-Item $work -Recurse -Force }
}
