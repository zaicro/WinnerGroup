# Migraciones de Cirsa.TableGames.Identity

Guía de trabajo para Entity Framework 6.5.2, VB.NET, .NET Framework 4.8 y SQL Server. Incluye creación en Visual Studio y despliegue mediante SQL o `ef6.exe`.

## 1. Qué se genera y qué se ejecuta

| Operación | Resultado | Ejecuta `Configuration.Seed` |
| --- | --- | --- |
| `Enable-Migrations` | Crea la configuración de migraciones; se utiliza inicialmente. | No |
| `Add-Migration` | Genera código y recursos de una migración. No aplica sus cambios. | No |
| `Update-Database` | Aplica las migraciones pendientes al destino indicado. | Sí, al finalizar correctamente la actualización. |
| `Update-Database -Script` | Genera SQL para revisión y ejecución posterior. | No |
| `ef6.exe database update` | Aplica migraciones desde una terminal o agente de despliegue. | Sí, al finalizar correctamente la actualización. |
| `ef6.exe database update --script` | Escribe el SQL en la salida estándar. | No |

La migración se genera una vez, se guarda en Git y se promueve entre ambientes. No se generan versiones distintas de la misma migración para DEV, UAT y PROD.

Sin destino explícito se aplican todas las migraciones pendientes hasta la más reciente, en orden. Si la base no existe, el migrador intenta crearla, sujeto a los permisos de conexión. No es necesario ejecutar una migración por comando.

## 2. Preparación del proyecto

- Proyecto de persistencia: `Cirsa.TableGames.Identity.DataAccess`.
- Proyecto de migraciones: `Cirsa.TableGames.Identity.DataAccess.Sql`.
- Ambos usan `net48` y el paquete `EntityFramework` versión `6.5.2`.
- El proyecto SQL referencia al proyecto de persistencia.
- El proveedor SQL Server de EF6 debe estar registrado mediante configuración de código o de archivo. En este proyecto la cadena se suministra externamente y no requiere guardarse en las bibliotecas.
- El contexto debe poder construirse durante el diseño, sin abrir conexiones ni realizar consultas en su constructor. Una fábrica es opcional si existe un constructor público sin parámetros adecuado.

Esta guía utiliza el nombre de contexto indicado para el proyecto actual:

```text
Cirsa.TableGames.Identity.DataAccess.Sql.SqlDbContext
```

Si la clase está dentro de `Namespace DbContexts`, el nombre será `Cirsa.TableGames.Identity.DataAccess.Sql.DbContexts.SqlDbContext`: ajusta `-ContextTypeName` antes de habilitar las migraciones. No se debe duplicar el `RootNamespace` dentro del namespace declarado en VB.

### Recursos de las migraciones en VB con proyecto SDK

Conserva este bloque en `Cirsa.TableGames.Identity.DataAccess.Sql.vbproj`:

```xml
<ItemGroup>
  <EmbeddedResource Update="Migrations\Identity\*.resx">
    <LogicalName>$(RootNamespace).Migrations.Identity.$([System.Text.RegularExpressions.Regex]::Replace('%(Filename)', '^\d+_', '')).resources</LogicalName>
  </EmbeddedResource>
</ItemGroup>
```

La regla elimina el prefijo numérico del nombre del recurso para que coincida con la clase de migración. Por ejemplo, `202611010306269_Init.resx` debe quedar incrustado como `Cirsa.TableGames.Identity.DataAccess.Sql.Migrations.Identity.Init.resources`.

Usa `Update`, no `Include`, porque el SDK ya incluye los `.resx`. La regla corresponde a la carpeta y namespace `Migrations.Identity`; si se cambian, debe ajustarse. Versiona este bloque junto con el proyecto.

## 3. Crear migraciones desde Visual Studio

Abre la solución y utiliza **Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes**. Los comandos siguientes no están disponibles automáticamente en una terminal PowerShell normal.

Cada comando se presenta en una sola línea para evitar problemas al pegarlo. `-Verbose` muestra el seguimiento. Si decides dividir un comando en PowerShell, el carácter de continuación es el acento grave (`), sin espacios después; no es una comilla simple.

### 3.1. Definir el destino de desarrollo

Ejecuta cada línea por separado:

```powershell
$identityProject = "Cirsa.TableGames.Identity.DataAccess.Sql"
```

```powershell
$identityConnection = "Server=DESKTOP-T0SDTHB;Database=Cirsa.TableGames.Identity;Integrated Security=True;"
```

`DESKTOP-T0SDTHB` es el servidor; `Cirsa.TableGames.Identity` es la base. Si SQL Server utiliza una instancia con nombre, configura `Server=EQUIPO\INSTANCIA`.

La autenticación de Windows usa la identidad del proceso que ejecuta los comandos. Revisa siempre el mensaje `Target database is` antes de continuar.

### 3.2. Habilitar migraciones: una vez por configuración

Compila la solución y ejecuta:

```powershell
Enable-Migrations -ProjectName $identityProject -StartUpProjectName $identityProject -ContextTypeName "Cirsa.TableGames.Identity.DataAccess.Sql.SqlDbContext" -MigrationsDirectory "Migrations\Identity" -ConnectionString $identityConnection -ConnectionProviderName "System.Data.SqlClient" -Verbose
```

No repitas este comando para cada cambio. Si ya existe la configuración de migraciones, continúa con `Add-Migration`.

### 3.3. Configurar el historial y la semilla

En el constructor de la clase de configuración generada:

```vb
Public Sub New()
    AutomaticMigrationsEnabled = False
    MigrationsDirectory = "Migrations\Identity"
    ContextKey = "Cirsa.TableGames.Identity"
End Sub
```

Mantén el `ContextKey` estable. Identifica el historial de este modelo dentro de `__MigrationHistory`; no es una versión ni debe cambiar por ambiente. Otros modelos que compartan la base necesitan sus propias claves y no deben administrar las mismas tablas.

La configuración puede invocar la semilla compartida:

```vb
Protected Overrides Sub Seed(context As SqlDbContext)
    IdentitySeeder.Seed(context)
End Sub
```

La semilla debe poder ejecutarse varias veces sin duplicar registros, restablecer contraseñas ni sobrescribir información administrada.

### 3.4. Generar la migración inicial

```powershell
Add-Migration -Name "Init" -ProjectName $identityProject -StartUpProjectName $identityProject -ConnectionString $identityConnection -ConnectionProviderName "System.Data.SqlClient" -Verbose
```

Para esta guía, el nombre inicial es **`Init`**. Si tu migración realmente se llama `InitialCreate`, utiliza ese nombre en los destinos; ambos nombres no son intercambiables.

Conserva los tres archivos generados: `.vb`, `.Designer.vb` y `.resx`. Revisa `Up()` y `Down()`, y comprueba las cuatro tablas, claves GUID, índices únicos, relaciones y auditoría. No debe aparecer una tabla propia para `DbAuditable` ni discriminadores inesperados.

### 3.5. Aplicar en DEV

```powershell
Update-Database -ProjectName $identityProject -StartUpProjectName $identityProject -ConnectionString $identityConnection -ConnectionProviderName "System.Data.SqlClient" -Verbose
```

Este comando modifica la base y ejecuta `Seed`. Verifica las tablas y los datos iniciales; ejecútalo nuevamente para comprobar que la semilla no duplica registros.

### 3.6. Cambios posteriores

Modifica las entidades o configuraciones, compila y genera una nueva migración con un nombre descriptivo:

```powershell
Add-Migration -Name "AddUserContactFields" -ProjectName $identityProject -StartUpProjectName $identityProject -ConnectionString $identityConnection -ConnectionProviderName "System.Data.SqlClient" -Verbose
```

Revisa sus operaciones y aplica con el comando del paso anterior. No elimines ni regeneres migraciones ya compartidas o aplicadas en bases que deban conservarse. Agregar o cambiar solamente el código de `Seed` no requiere una migración de estructura.

## 4. Despliegue mediante un script SQL

### 4.1. Generar el script de una entrega

Define la última migración de esa entrega; este ejemplo usa `Init`:

```powershell
$identityTarget = "Init"
```

En la consola NuGet, genera el script desde el estado inicial:

```powershell
Update-Database -Script -SourceMigration $InitialDatabase -TargetMigration $identityTarget -ProjectName $identityProject -StartUpProjectName $identityProject -ConnectionString $identityConnection -ConnectionProviderName "System.Data.SqlClient" -Verbose
```

`$InitialDatabase` es la referencia especial disponible en las herramientas de EF6. Desde ese origen, EF6 genera comprobaciones de historial para las migraciones pendientes. Usa la conexión DEV para generar y revisar el artefacto; no necesitas conectarte a PROD desde Visual Studio para esta tarea.

También puedes generar un cambio para un estado de origen conocido:

```powershell
Update-Database -Script -SourceMigration "Init" -TargetMigration "AddUserContactFields" -ProjectName $identityProject -StartUpProjectName $identityProject -ConnectionString $identityConnection -ConnectionProviderName "System.Data.SqlClient" -Verbose
```

En ese caso, comprueba que el origen coincida con la base donde se aplicará. Indicar un destino anterior al estado actual puede representar una reversión; verifica siempre el orden.

### 4.2. Revisar y ejecutar

1. Guarda el resultado como un archivo SQL identificado con la versión de entrega.
2. Revisa operaciones destructivas, conversiones y posibles bloqueos.
3. Prueba el mismo archivo en UAT sobre un estado representativo del origen de producción.
4. Promueve ese mismo archivo aprobado, sin regenerarlo desde código distinto.
5. Confirma servidor, base y cuenta de ejecución. Si la base es nueva, aprovisiónala explícitamente antes de ejecutar el script; no presupongas que el SQL incluye `CREATE DATABASE`.
6. Revisa cualquier instrucción `USE` o referencia de base incluida en el archivo antes de ejecutarlo en otro ambiente.
7. Ejecuta mediante SSMS o `sqlcmd` y conserva el resultado.

Ejemplo con `sqlcmd` instalado, desde PowerShell en la carpeta donde está el script:

```powershell
$identitySqlServer = "SERVIDOR_UAT"
```

```powershell
$identitySqlDatabase = "Cirsa.TableGames.Identity"
```

```powershell
sqlcmd -S $identitySqlServer -d $identitySqlDatabase -E -b -i ".\identity-migrations.sql"
```

```powershell
if ($LASTEXITCODE -ne 0) { throw "Falló la ejecución del script de identidad." }
```

`-E` usa Windows y `-b` permite detectar errores mediante el código de salida. Usa el nombre real del servidor aprobado; el ejemplo `SERVIDOR_UAT` es un marcador. La instalación y los requisitos de conexión/certificados dependen de la versión de `sqlcmd` y del servidor.

El script generado incluye el mantenimiento del historial de migraciones. No insertes ni elimines filas de `__MigrationHistory` manualmente para forzar su ejecución.

### 4.3. Semillas y SQL explícito

`Configuration.Seed` no se convierte en SQL al generar un script. Si despliegas solo SQL, los datos obligatorios deben estar en una migración de datos o en un script versionado independiente.

Las instrucciones agregadas mediante `Sql(...)` dentro de `Up()` sí forman parte del despliegue por ambos caminos. Esto incluye inserciones, vistas y procedimientos.

Ejemplo:

```vb
Public Overrides Sub Up()
    Sql("CREATE VIEW [Identity].[ActiveUsers] AS SELECT UserId, UserName FROM [Identity].[TbUser] WHERE IsActive = 1")
End Sub

Public Overrides Sub Down()
    Sql("DROP VIEW [Identity].[ActiveUsers]")
End Sub
```

No incluyas `GO` dentro de `Sql(...)`: es un separador de herramientas, no una instrucción del motor. Para migraciones con lotes complejos de DDL, revisa el SQL emitido y prueba tanto su generación como su ejecución. EF6 no detecta cambios de vistas o procedimientos a partir de las entidades: sus modificaciones requieren código explícito en nuevas migraciones.

## 5. Ejecución mediante ef6.exe

Estos comandos se ejecutan en **PowerShell normal**, sin Visual Studio. A diferencia de la consola NuGet, no reciben `-ProjectName`: reciben la ruta de la DLL compilada.

### 5.1. Preparar el artefacto

1. Compila la entrega en `Release` con las migraciones incluidas.
2. Prepara una carpeta con la salida de `DataAccess.Sql` y sus dependencias, incluyendo `DataAccess`, `EntityFramework.dll` y `EntityFramework.SqlServer.dll`, además de cualquier otra DLL utilizada.
3. Disponibiliza la herramienta de la misma versión de EF6. Si la empaquetas, conserva los archivos auxiliares requeridos de su carpeta de herramientas; no presupongas que basta copiar un ejecutable aislado.
4. Ejecuta desde Windows con .NET Framework 4.8 disponible, acceso al servidor y los permisos necesarios. La cuenta del agente será la identidad de Windows utilizada por `Integrated Security=True`.
5. Promueve el mismo artefacto probado en UAT a PROD. No recompiles entre ambos ambientes.

No necesitas Visual Studio ni el código fuente en el servidor que ejecuta el despliegue. El registro del proveedor EF6 debe estar disponible en el artefacto, mediante el mecanismo de configuración elegido.

### 5.2. Prueba local desde la raíz de la solución

Usa las rutas de este equipo; ajústalas si el repositorio tiene otra estructura:

```powershell
$identityEfTool = "D:\NuGetPackages\entityframework\6.5.2\tools\net45\any\ef6.exe"
```

```powershell
$identityAssembly = (Resolve-Path ".\Cirsa.TableGames.Identity.DataAccess.Sql\bin\Debug\net48\Cirsa.TableGames.Identity.DataAccess.Sql.dll").Path
```

```powershell
$identityConnection = "Server=DESKTOP-T0SDTHB;Database=Cirsa.TableGames.Identity;Integrated Security=True;"
```

```powershell
& $identityEfTool database update --assembly $identityAssembly --connection-string $identityConnection --connection-provider "System.Data.SqlClient" --verbose
```

```powershell
if ($LASTEXITCODE -ne 0) { throw "Falló la actualización de la base de identidad." }
```

El operador `&` ejecuta el archivo cuya ruta está guardada en la variable. `--verbose` es el equivalente de `-Verbose` de la consola NuGet.

Para ver la ayuda de la versión instalada:

```powershell
& $identityEfTool database update --help
```

### 5.3. Fijar la versión destino

```powershell
$identityTarget = "Init"
```

```powershell
& $identityEfTool database update --assembly $identityAssembly --connection-string $identityConnection --connection-provider "System.Data.SqlClient" --target $identityTarget --verbose
```

```powershell
if ($LASTEXITCODE -ne 0) { throw "Falló la actualización de la base de identidad." }
```

Se aplican todas las migraciones pendientes hasta ese destino, no solo esa migración. Comprueba que el destino no sea anterior al estado actual salvo que estés realizando una reversión deliberada y probada.

Si hay varias configuraciones en el ensamblado, agrega `--migrations-config` con el nombre completo real de la clase. Si utilizaste la carpeta y namespace de esta guía, normalmente será `Cirsa.TableGames.Identity.DataAccess.Sql.Migrations.Identity.Configuration`; compruébalo en el código generado.

### 5.4. Generar SQL con la herramienta

La herramienta también permite producir el archivo para la alternativa de despliegue SQL:

```powershell
& $identityEfTool database update --assembly $identityAssembly --connection-string $identityConnection --connection-provider "System.Data.SqlClient" --script --source "0" --target $identityTarget | Out-File -FilePath ".\identity-migrations.sql" -Encoding utf8
```

```powershell
if ($LASTEXITCODE -ne 0) { throw "Falló la generación del script de identidad." }
```

En este comando se omite `--verbose` para no mezclar seguimiento con el SQL redirigido. No combines la salida de errores con el archivo SQL. Revisa el archivo resultante antes de ejecutarlo y no publiques el archivo parcial si falló el proceso.

`--source "0"` equivale al origen inicial usado para producir el script completo con comprobaciones de historial. `--target` fija el alcance de la entrega.

## 6. Política de semillas por ambiente

| Datos | Tratamiento recomendado |
| --- | --- |
| Aplicaciones y roles indispensables | Migraciones de datos o scripts versionados si deben funcionar también por despliegue SQL. |
| Datos de prueba | Semilla habilitada únicamente en DEV/pruebas. |
| Administrador inicial de producción | Aprovisionamiento explícito con credencial propia del ambiente. |

La semilla actual contiene un usuario de prueba con contraseña documentada en código. No la distribuyas sin cambios a producción: `ef6.exe` ejecutará el método `Seed`. El nombre de usuario no concede privilegios por sí solo; se requieren las asignaciones de roles.

Si se opta por ejecutar semillas en el despliegue, su habilitación por ambiente debe ser explícita. No dependas de recordar comentar código antes de publicar.

## 7. Flujo recomendado de entrega

1. Modificar el modelo y generar la migración en desarrollo.
2. Revisar la migración, probarla y guardar en Git todos sus archivos y cambios del `.vbproj`.
3. Compilar una versión identificable y producir el artefacto SQL o binario.
4. Probar ese artefacto en UAT, incluidas las operaciones sobre datos existentes.
5. Acordar el orden entre cambios de base y publicación de aplicación. No todos los cambios son compatibles con la versión anterior.
6. Preparar recuperación y ventana de ejecución según el impacto. `Down()` no recupera automáticamente los datos eliminados.
7. Ejecutar una sola instancia del despliegue de base mediante una cuenta autorizada, separada de la cuenta habitual de la aplicación.
8. Detener el pipeline ante errores y revisar el estado antes de reintentar.
9. Verificar esquema, historial, datos obligatorios y funcionamiento de la aplicación.

Evita aplicar cambios de producción desde un equipo personal o ejecutar migraciones automáticamente al iniciar cada WinForms. Centraliza la ejecución en el proceso de despliegue.

## 8. Diagnóstico y mantenimiento

### Comando no reconocido

`Enable-Migrations`, `Add-Migration` y `Update-Database` se usan en la consola NuGet con la solución abierta y el paquete EF6 cargado. `ef6.exe` se usa desde una terminal normal con una ruta explícita. Si las herramientas NuGet no se cargan, restaura paquetes y vuelve a abrir Visual Studio.

### MissingManifestResourceException

Comprueba el bloque `EmbeddedResource` del paso 2, la carpeta real de la migración y su namespace. Limpia y recompila. No regeneres migraciones repetidamente para resolver un problema de recursos.

### La migración ya fue aplicada

Comprueba servidor, base y `ContextKey`. Una migración aplicada no debe regenerarse para una entrega compartida: agrega una nueva. No borres filas del historial para forzarlo.

Solo en una base local desechable puede reiniciarse todo: elimina la base correcta, los tres archivos de cada migración que vas a reemplazar, limpia y recompila. Conserva `Configuration.vb` y el ajuste de recursos. No conviertas este procedimiento en una práctica de UAT o PROD.

### El script no creó los datos iniciales

Es esperado si estaban únicamente en `Configuration.Seed`. Incorpora los datos obligatorios a una migración o script versionado; no ejecutes la semilla de pruebas en producción para suplirlos.

### Inspeccionar el historial

La tabla se llama `__MigrationHistory`. Comprueba su esquema real en tu base; no presupongas que está en `dbo` o `Identity`:

```sql
SELECT s.name AS SchemaName, t.name AS TableName
FROM sys.tables AS t
INNER JOIN sys.schemas AS s ON s.schema_id = t.schema_id
WHERE t.name = N'__MigrationHistory';
```

Consulta después la tabla encontrada y filtra `ContextKey = N'Cirsa.TableGames.Identity'`. Conserva el historial de este modelo aunque existan otros modelos en la misma base.

## 9. Referencias oficiales

- [EF6: Code First Migrations](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/)
- [EF6: uso de ef6.exe](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/ef6-exe)
- [MSBuild: nombres de recursos incrustados](https://learn.microsoft.com/en-us/dotnet/core/resources/manifest-file-names)

Los comandos están adaptados al proyecto y a la conexión local utilizados durante esta implementación. Sustituye rutas, versión destino y servidor según el ambiente aprobado. Esta guía documenta el procedimiento; sus comandos no se ejecutaron contra tus servidores al crear el archivo.
