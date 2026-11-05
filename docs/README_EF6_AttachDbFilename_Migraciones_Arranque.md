# VB.NET Framework 4.8 + EF6: base MDF y migraciones al iniciar

## 1. Resultado buscado

La aplicación utiliza SQL Server Express o LocalDB y una conexión con `AttachDbFilename`. Al iniciar:

1. Prepara una carpeta de datos estable.
2. Configura `DataDirectory` antes de utilizar EF.
3. Ejecuta las migraciones incluidas en la versión instalada.
4. Si la base no existe, EF6 solicita al motor crearla y aplicar la estructura inicial.
5. Si existe, EF6 consulta `__MigrationHistory` y aplica las migraciones pendientes.
6. La aplicación continúa únicamente si la actualización terminó correctamente.

EF6 no instala SQL Server ni escribe directamente un archivo MDF. El motor crea y administra los archivos `.mdf` y `.ldf`. Management Studio no es necesario para ejecutar este flujo.

**Alcance:** SQL Server, VB.NET, .NET Framework 4.8 y EF6 Code First. Los ejemplos web corresponden a ASP.NET clásico: Web Forms, MVC 5 o Web API 2. No son ejemplos de ASP.NET Core ni EF Core.

## 2. Dos significados de «automática»

| Operación | Cuándo ocurre | Recomendación |
|---|---|---|
| Generar una migración VB con `Add-Migration` | Desarrollo, después de cambiar el modelo | Revisarla y versionarla con Git |
| Aplicar migraciones con `DbMigrator.Update()` | Despliegue o inicio de la aplicación | Automatizable bajo las condiciones descritas aquí |
| Deducir cambios sin migración VB mediante `AutomaticMigrationsEnabled = True` | Ejecución de EF6 | Evitarlo para este caso de producción |

**La propuesta principal genera las migraciones en desarrollo y las aplica automáticamente al arrancar.** Cambiar una entidad y publicar el EXE sin generar su migración no actualiza el esquema con esta configuración; EF puede rechazar el modelo por cambios pendientes.

## 3. Estructura de ejemplo

| Proyecto o archivo | Contenido |
|---|---|
| `MiApp.Datos/Producto.vb` | Entidad de prueba |
| `MiApp.Datos/AppDbContext.vb` | Modelo y mapeo EF6 |
| `MiApp.Datos/Migrations/Configuration.vb` | Configuración de migraciones |
| `MiApp.Datos/Migrations/...` | Migraciones generadas, diseñador y recursos |
| `MiApp.Datos/DatabaseBootstrapper.vb` | Preparación de la ruta y aplicación de migraciones |
| Proyecto WinForms, consola o web | Invoca el bootstrapper y aporta el config |

Los tres tipos de aplicación son alternativas de host para el mismo código de datos. No tienen que ejecutarse simultáneamente contra la misma base.

Instala `EntityFramework` 6.x en el proyecto de datos y en los hosts que lo necesiten. Como referencia reproducible puedes fijar `6.5.1`, manteniendo la misma versión en toda la solución:

```powershell
Install-Package EntityFramework -Version 6.5.1 -ProjectName MiApp.Datos
```

Añade referencia a `System.Configuration` en el proyecto de datos. Los hosts referencian `MiApp.Datos`. Los nombres de proyecto de los comandos deben adaptarse a tu solución.

Para evitar que la raíz de nombres de VB se duplique, los ejemplos compartidos usan `Namespace Global.MiApp.Datos` explícitamente.

## 4. Conexión en App.config / Web.config

Este ejemplo utiliza **LocalDB**, ejecutándose bajo el usuario Windows de la aplicación:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <configSections>
    <section name="entityFramework"
             type="System.Data.Entity.Internal.ConfigFile.EntityFrameworkSection, EntityFramework, Version=6.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089"
             requirePermission="false" />
  </configSections>

  <connectionStrings>
    <add name="BaseLocal"
         providerName="System.Data.SqlClient"
         connectionString="Data Source=(LocalDB)\MSSQLLocalDB;Initial Catalog=MiAppLocal;AttachDbFilename=|DataDirectory|\MiAppLocal.mdf;Integrated Security=True;MultipleActiveResultSets=True;" />
  </connectionStrings>

  <entityFramework>
    <providers>
      <provider invariantName="System.Data.SqlClient"
                type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
    </providers>
  </entityFramework>
</configuration>
```

En un Web.config existente, integra las secciones sin borrar `system.web`, `system.webServer` ni el resto de la configuración. `configSections`, si existe, debe continuar como primer elemento hijo de `configuration`. No dupliques el registro `entityFramework`.

La configuración efectiva es la **del host**: `MiApp.WinForms.exe.config`, `MiApp.Consola.exe.config` o `Web.config`. Un App.config en una biblioteca de clases no se convierte automáticamente en la configuración del proceso que la consume.

### Si ya usas SQL Server Express como servicio

Cambia únicamente la instancia de este ejemplo por la real, por ejemplo:

```text
Data Source=.\SQLEXPRESS;Initial Catalog=MiAppLocal;AttachDbFilename=|DataDirectory|\MiAppLocal.mdf;Integrated Security=True;MultipleActiveResultSets=True;
```

Diferencias prácticas:

- LocalDB y SQL Express como servicio no son la misma instancia.
- Para SQL Express, la cuenta del **servicio SQL Server** debe poder leer y escribir los archivos y la carpeta; no basta con permisos para el usuario de la aplicación.
- `Integrated Security=True` utiliza la identidad Windows del host: en IIS será la identidad configurada para la aplicación, no necesariamente tu usuario de desarrollo.
- El login necesita permisos para crear la base si no existe y para ejecutar el DDL de las migraciones. La ruta no concede esos permisos.
- No añadas `User Instance=True` por costumbre. Si tu conexión actual lo incluye, identifica y prueba ese modo antes de modificarla.
- `Initial Catalog` debe identificar la misma base y ruta en todos los arranques. Si una instancia ya tiene `MiAppLocal` registrada contra otro MDF, resuelve ese conflicto; cambiar solo la ruta no cambia automáticamente el archivo asociado.

## 5. Entidad y DbContext

### Producto.vb

```vb
Namespace Global.MiApp.Datos
    Public Class Producto
        Public Property Id As Integer
        Public Property Nombre As String
    End Class
End Namespace
```

### AppDbContext.vb

```vb
Imports System.Data.Entity

Namespace Global.MiApp.Datos
    Public Class AppDbContext
        Inherits DbContext

        Shared Sub New()
            ' Evita un inicializador implícito. El bootstrapper usa DbMigrator.
            Database.SetInitializer(Of AppDbContext)(Nothing)
        End Sub

        Public Sub New()
            MyBase.New("name=BaseLocal")
        End Sub

        Public Property Productos As DbSet(Of Producto)

        Protected Overrides Sub OnModelCreating(builder As DbModelBuilder)
            MyBase.OnModelCreating(builder)

            Dim producto = builder.Entity(Of Producto)()
            producto.ToTable("Productos", "dbo")
            producto.HasKey(Function(p) p.Id)
            producto.Property(Function(p) p.Nombre).IsRequired().HasMaxLength(100)
        End Sub
    End Class
End Namespace
```

`Id` sigue la convención de identidad de EF6 para una clave entera. No se utiliza ningún inicializador que elimine y vuelva a crear la base cuando cambia el modelo.

## 6. Configuración de migraciones

### Migrations/Configuration.vb

```vb
Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Migrations

Namespace Global.MiApp.Datos.Migrations
    Public Class Configuration
        Inherits DbMigrationsConfiguration(Of Global.MiApp.Datos.AppDbContext)

        Public Sub New()
            AutomaticMigrationsEnabled = False
            AutomaticMigrationDataLossAllowed = False

            ContextKey = "MiApp.Datos.AppDbContext"
            MigrationsAssembly = GetType(Configuration).Assembly
            MigrationsNamespace = "MiApp.Datos.Migrations"
            TargetDatabase = New DbConnectionInfo("BaseLocal")
        End Sub
    End Class
End Namespace
```

Mantén `ContextKey` estable entre versiones. El hecho de que la clase se llame `Configuration` no la convierte en el App.config: configura el comportamiento de EF6 Migrations.

Si `Enable-Migrations` ya generó esta clase, adapta la existente; no agregues una segunda configuración para el mismo flujo. La propiedad `MigrationsNamespace` debe coincidir con el espacio de nombres efectivo de las migraciones generadas, considerando el Root Namespace del proyecto VB.

## 7. Generar la primera migración: desarrollo

Con el modelo creado y la conexión de desarrollo configurada:

```powershell
Enable-Migrations -ContextTypeName MiApp.Datos.AppDbContext -ProjectName MiApp.Datos -StartUpProjectName MiApp.Consola
Add-Migration InitialCreate -ProjectName MiApp.Datos -StartUpProjectName MiApp.Consola -ConfigurationTypeName MiApp.Datos.Migrations.Configuration
```

`Enable-Migrations` es una operación inicial: omítela si ya tienes `Configuration.vb`. En ese caso utiliza la configuración de la sección anterior y ejecuta `Add-Migration`.

**La consola de NuGet no ejecuta tu `Sub Main`, `Application_Startup` ni `Application_Start`.** Por eso, para los comandos de diseño sustituye temporalmente `|DataDirectory|\MiAppLocal.mdf` en el config del host de desarrollo por una ruta absoluta de laboratorio, por ejemplo `C:\MiAppLab\MiAppLocal.mdf`. Crea la carpeta antes. No apuntes estos comandos a la base real de un usuario.

Revisa que `InitialCreate.Up()` cree `dbo.Productos`. EF genera también metadatos en `.Designer.vb` y `.resx`: conserva y versiona todos esos archivos. Compila la solución después de generarlos.

Para probar la actualización manualmente en desarrollo:

```powershell
Update-Database -ProjectName MiApp.Datos -StartUpProjectName MiApp.Consola -ConfigurationTypeName MiApp.Datos.Migrations.Configuration -Verbose
```

Los comandos corresponden a Package Manager Console de EF6. Si tu solución SDK no integra bien esos comandos, utiliza una herramienta de scaffolding EF6 compatible, como la consola del laboratorio anterior; el bootstrapper de ejecución de esta guía sigue siendo `DbMigrator`.

Antes de distribuir, restaura la conexión con `|DataDirectory|`. Lo que viaja con la aplicación es el assembly que contiene las migraciones, no la consola de NuGet. No hace falta instalar Visual Studio en el PC destino.

## 8. Bootstrapper compartido

### DatabaseBootstrapper.vb

```vb
Imports System
Imports System.Data.Entity.Migrations
Imports System.IO
Imports System.Linq

Namespace Global.MiApp.Datos
    Public NotInheritable Class DatabaseBootstrapper
        Private Sub New()
        End Sub

        Public Shared Sub Inicializar(carpetaDatos As String)
            If String.IsNullOrWhiteSpace(carpetaDatos) Then
                Throw New ArgumentException("Debes indicar la carpeta de datos.", NameOf(carpetaDatos))
            End If

            Dim ruta = Path.GetFullPath(carpetaDatos)
            Directory.CreateDirectory(ruta)

            ' Debe ocurrir antes de abrir una conexión o construir un contexto.
            AppDomain.CurrentDomain.SetData("DataDirectory", ruta)

            Dim configuracion As New Global.MiApp.Datos.Migrations.Configuration()
            Dim migrador As New DbMigrator(configuracion)

            If Not migrador.GetLocalMigrations().Any() Then
                Throw New InvalidOperationException(
                    "No hay migraciones compiladas. Genera InitialCreate y recompila.")
            End If

            migrador.Update()
        End Sub
    End Class
End Namespace
```

Esta rutina no requiere que el MDF exista previamente. Con una migración inicial compilada, una instancia accesible y los permisos apropiados, `Update()` crea la base y aplica las migraciones. No crees antes un archivo vacío llamado `.mdf`: no sería una base válida.

La rutina usa una ruta de datos por proceso. No cambies `DataDirectory` para alternar entre varias bases mientras ya existen contextos o conexiones abiertas; para ese escenario diseña conexiones explícitas por base.

**Condición del ejemplo:** un único proceso aplica migraciones sobre una base en un momento dado. El método no implementa exclusión entre procesos. `__MigrationHistory` permite saber qué se aplicó, pero no es un candado de despliegue.

## 9. Inicio de una aplicación de consola

### Program.vb

```vb
Imports System
Imports System.IO
Imports System.Linq
Imports MiApp.Datos

Module Program
    Public Function Main(args As String()) As Integer
        Try
            Dim carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MiEmpresa", "MiApp", "Datos")

            DatabaseBootstrapper.Inicializar(carpeta)

            ' A partir de aquí se permite utilizar la base.
            Using db As New AppDbContext()
                Console.WriteLine("Base lista. Productos: " & db.Productos.Count().ToString())
            End Using

            Return 0
        Catch ex As Exception
            ' En producción, utiliza el logger de la aplicación.
            Console.Error.WriteLine(ex.ToString())
            Return 1
        End Try
    End Function
End Module
```

Añade `Imports System.Linq` si tu proyecto no lo importa globalmente. Selecciona `Sub Main`/`Program` como objeto de inicio según la plantilla VB de consola.

La ubicación resultante normalmente será:

```text
C:\Users\<usuario>\AppData\Local\MiEmpresa\MiApp\Datos\MiAppLocal.mdf
```

El motor gestiona también el archivo de log; no asumas su nombre físico para implementar copias o recuperación.

## 10. Inicio de WinForms

### Opción A: plantilla con Application Framework habilitado

En Propiedades del proyecto → Aplicación → Ver eventos de aplicación, agrega el evento en `ApplicationEvents.vb`:

```vb
Imports System
Imports System.IO
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ApplicationServices
Imports MiApp.Datos

Namespace My
    Partial Friend Class MyApplication
        Private Sub MyApplication_Startup(
            sender As Object,
            e As StartupEventArgs) Handles Me.Startup

            Try
                Dim carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "MiEmpresa", "MiApp", "Datos")

                DatabaseBootstrapper.Inicializar(carpeta)
            Catch ex As Exception
                System.Diagnostics.Trace.TraceError(ex.ToString())
                MessageBox.Show(
                    "No fue posible preparar la base local. La aplicación se cerrará.",
                    "Error de base de datos",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error)

                e.Cancel = True
            End Try
        End Sub
    End Class
End Namespace
```

Integra este código en el archivo generado por Visual Studio, manteniendo su namespace `My`; no crees una segunda clase de aplicación. No abras conexiones desde un splash screen o desde inicializadores de campos antes de este evento.

### Opción B: punto de entrada propio

Si deshabilitas Application Framework y configuras `Sub Main` como objeto de inicio:

```vb
Imports System
Imports System.IO
Imports System.Windows.Forms
Imports MiApp.Datos

Module Program
    <STAThread>
    Public Sub Main()
        Application.EnableVisualStyles()
        Application.SetCompatibleTextRenderingDefault(False)

        Try
            Dim carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MiEmpresa", "MiApp", "Datos")

            DatabaseBootstrapper.Inicializar(carpeta)
        Catch ex As Exception
            System.Diagnostics.Trace.TraceError(ex.ToString())
            MessageBox.Show("No fue posible actualizar la base local.")
            Return
        End Try

        Application.Run(New Form1())
    End Sub
End Module
```

Elige A o B. No ejecutes el bootstrapper desde ambos puntos ni desde cada formulario.

El ejemplo bloquea el arranque mientras aplica los cambios. Si una migración tarda, utiliza una pantalla de progreso; no abras la pantalla operativa mientras el esquema sigue actualizándose.

## 11. Inicio de una web ASP.NET Framework 4.8

### Global.asax.vb

```vb
Imports System
Imports System.Web
Imports MiApp.Datos

Public Class Global_asax
    Inherits HttpApplication

    Protected Sub Application_Start(sender As Object, e As EventArgs)
        Try
            ' Ejemplo para desarrollo o una instalación de instancia única.
            Dim carpeta = Server.MapPath("~/App_Data")
            DatabaseBootstrapper.Inicializar(carpeta)

            ' Después registra rutas, Web API, MVC y otros componentes
            ' que pudieran utilizar la base.
        Catch ex As Exception
            System.Diagnostics.Trace.TraceError(ex.ToString())
            Throw
        End Try
    End Sub
End Class
```

Adapta el nombre a la clase indicada por `Inherits` en tu Global.asax. Si ya existe `Application_Start`, integra la llamada en él.

`Application_Start` ocurre al inicializar la aplicación ASP.NET, normalmente con la primera solicitud o el precalentamiento; no implica que el cambio se ejecute en el instante de copiar los archivos al servidor. Puede volver a ocurrir con reciclados y nuevas instancias del proceso.

**Para una web de producción con base compartida, mi recomendación es un migrador de despliegue ejecutado una sola vez**, antes de habilitar tráfico, y una identidad web sin permisos de modificar el esquema. Dos workers, un reciclado solapado o dos servidores pueden intentar migrar a la vez; este ejemplo no coordina esos procesos.

LocalDB es práctico para desarrollo bajo tu usuario. No asumas que la misma conexión funcionará bajo IIS: cambian identidad, perfil y permisos. Para una web alojada, utiliza normalmente una instancia SQL Server/Express provisionada y una base registrada; no necesitas AttachDbFilename para conservar las migraciones.

Si de todos modos utilizas un MDF con SQL Express local, la cuenta del servicio SQL Server necesita acceso a `App_Data` o a la carpeta externa elegida, además de los permisos de la aplicación. No guardes datos persistentes en una carpeta que la publicación borre o reemplace.

## 12. Qué ocurre al publicar una nueva versión

Ejemplo: añades `Descripcion` a Producto y su mapeo.

En desarrollo:

```powershell
Add-Migration AgregarDescripcion -ProjectName MiApp.Datos -StartUpProjectName MiApp.Consola -ConfigurationTypeName MiApp.Datos.Migrations.Configuration
```

Revisa `Up()` y `Down()`, prueba sobre una copia de una base anterior y recompila. Distribuye el nuevo assembly de datos y las dependencias con la aplicación.

En el PC destino:

1. Arranca la nueva versión.
2. El bootstrapper ejecuta `Update()`.
3. EF ve `InitialCreate` aplicada y `AgregarDescripcion` pendiente.
4. Ejecuta la segunda migración y registra su aplicación.
5. Continúa el arranque.

Si no hay migraciones pendientes, no vuelve a crear las tablas. Si sobrescribes el MDF en cada actualización, pierdes esa continuidad y puedes perder datos: el despliegue debe conservar los archivos de datos existentes.

`AutomaticMigrationDataLossAllowed = False` limita las migraciones automáticas deducidas por EF. **No impide que una migración explícita incluya `DropColumn`, `DropTable` o SQL destructivo**; por eso revisar el código sigue siendo necesario.

Si implementas `Seed`, recuerda que EF6 puede volver a ejecutarlo al actualizar. Debe ser repetible y no duplicar información ni restablecer datos de negocio en cada arranque.

## 13. Si la base ya existe y tiene información

No apliques `InitialCreate` que cree tablas sobre un MDF que ya contiene esas mismas tablas. Primero establece una línea base con el modelo fiel a la estructura real.

Para un parque de bases que ya tienen el mismo esquema, una opción es:

```powershell
Add-Migration BaselineExistente -IgnoreChanges -ProjectName MiApp.Datos -StartUpProjectName MiApp.Consola -ConfigurationTypeName MiApp.Datos.Migrations.Configuration
Update-Database -ProjectName MiApp.Datos -StartUpProjectName MiApp.Consola -ConfigurationTypeName MiApp.Datos.Migrations.Configuration -Verbose
```

Esta migración tiene operaciones vacías y registra el snapshot del modelo. **No crea las tablas de negocio en una instalación vacía.** Si necesitas simultáneamente instalar desde cero y adoptar bases existentes, define una estrategia específica de migración inicial y registro del historial, probada en ambos escenarios; no distribuyas una baseline vacía suponiendo que crea la base completa.

Antes de adoptar el historial, comprueba que las bases instaladas realmente coincidan: columnas, tipos, nulabilidad, claves y restricciones. `-IgnoreChanges` no valida ni corrige esas diferencias. No cambies el modelo antes de establecer la línea base de la estructura existente.

## 14. Alternativa: inicializador MigrateDatabaseToLatestVersion

EF6 ofrece un inicializador específico para aplicar migraciones. Es una alternativa al bootstrapper explícito, no una segunda llamada que debas agregar al flujo anterior.

Si eliges esta alternativa, **retira el constructor Shared que registra `SetInitializer(Nothing)` en AppDbContext**, prepara previamente `DataDirectory` y registra:

```vb
System.Data.Entity.Database.SetInitializer(
    New System.Data.Entity.MigrateDatabaseToLatestVersion(
        Of MiApp.Datos.AppDbContext,
           MiApp.Datos.Migrations.Configuration)())

Using db As New MiApp.Datos.AppDbContext()
    db.Database.Initialize(force:=False)
End Using
```

El inicializador puede ejecutarse al primer uso del contexto; `Initialize` permite forzarlo en el punto elegido del arranque. Mantén `AutomaticMigrationsEnabled = False` si quieres aplicar únicamente migraciones explícitas.

Para tu caso prefiero `DbMigrator.Update()` desde el bootstrapper: el momento del cambio y el tratamiento de fallos quedan visibles en el punto de entrada.

## 15. Si de verdad quieres migraciones automáticas deducidas

EF6 permite cambiar esta propiedad en Configuration:

```vb
AutomaticMigrationsEnabled = True
AutomaticMigrationDataLossAllowed = False
```

Después `DbMigrator.Update()` puede deducir y aplicar diferencias del modelo sin una clase VB para cada cambio compatible. Esto **no genera archivos `.vb` en el PC del usuario**, no sustituye scripts especiales y no interpreta de forma fiable la intención de renombrar datos o transformar información.

Para una aplicación distribuida que debe conservar datos, utiliza el flujo explícito de las secciones anteriores. Automatiza la aplicación de cambios revisados, no la deducción del esquema en producción.

## 16. Rutas, respaldo y concurrencia

| Situación | Decisión |
|---|---|
| Una base por usuario Windows | LocalApplicationData y LocalDB pueden encajar |
| Una base por PC compartida por varias cuentas | Carpeta común estable e instancia SQL Express con permisos definidos; no uses LocalApplicationData como si fuera común |
| Varios procesos contra una base | Un único migrador coordinado; este bootstrapper por sí solo no basta |
| Web con varios workers o servidores | Migración en despliegue, antes del tráfico |
| Nueva versión instalada | Conserva MDF/LDF y la ruta estable |
| Cambio de carpeta de una base ya registrada | Planifica mover/adjuntar con SQL Server; no basta con editar la cadena |
| Respaldo con base abierta | Usa BACKUP DATABASE y prueba restauración; no copies MDF/LDF activos como si fueran un respaldo consistente |
| Fallo de una migración | Registra el error, detén el arranque operativo y revisa el estado antes de reintentar |

No borres un LDF para «arreglar» un error de attach. Tampoco vuelvas a crear la base automáticamente ante cualquier excepción: un fallo puede ser de permisos, instancia o archivos y no una base inexistente.

El cifrado de columnas es una decisión independiente: AttachDbFilename y las migraciones no cifran los datos ni protegen por sí solos los archivos copiados.

## 17. Validación mínima antes de distribuir

- Primera ejecución en una carpeta vacía: aparecen los archivos y las tablas esperadas.
- Segundo arranque: se conservan los datos y no se repiten operaciones de creación.
- Actualización desde una versión anterior: se aplica solo lo pendiente y se conserva la información.
- Ejecución con la cuenta Windows real de destino: funciona sin depender del usuario administrador de desarrollo.
- Fallo de permisos o instancia apagada: la aplicación se detiene con registro del error.
- Respaldo y restauración: se recupera una base utilizable.
- Si puede haber varios procesos: se prueba la coordinación real del despliegue.

Estos son criterios para tus pruebas en Windows. Esta guía no afirma que sus ejemplos hayan sido ejecutados contra una instancia SQL Server en este entorno.

## 18. Fuentes oficiales

- [EF6 Code First Migrations y actualización al iniciar](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/)
- [EF6 migraciones automáticas](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/automatic)
- [EF6 con una base existente](https://learn.microsoft.com/en-us/ef/ef6/modeling/code-first/migrations/existing-database)
- [EF6 y conexión LocalDB con AttachDbFilename](https://learn.microsoft.com/en-us/aspnet/mvc/overview/getting-started/getting-started-with-ef-using-mvc/creating-an-entity-framework-data-model-for-an-asp-net-mvc-application)
- [SqlClient y LocalDB](https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/sql/sqlclient-support-for-localdb)
- [MigrateDatabaseToLatestVersion](https://learn.microsoft.com/en-us/dotnet/api/system.data.entity.migratedatabasetolatestversion-2)
