# CRUD Personas — VB.NET / Windows Forms / .NET Framework 4.8

Ejemplo pequeño con cuatro proyectos y Entity Framework **6.5.2** (EF6, no EF Core). Permite listar, crear, editar y eliminar personas. Cada persona tiene `Id` entero autogenerado y `Nombre` obligatorio, de hasta 150 caracteres. Se recortan los espacios al principio y al final; se permiten nombres duplicados.

## Abrir y ejecutar

En **Windows**, instala Visual Studio 2022 con:

- Carga de trabajo **Desarrollo de escritorio de .NET**.
- SDK y targeting pack de **.NET Framework 4.8**.
- **SQL Server Express LocalDB**, componente individual de Visual Studio.

1. Descomprime el ZIP en una carpeta local.
2. Abre `CrudPersonas.sln`.
3. Establece **CrudPersonas.UI** como proyecto de inicio.
4. Permite restaurar los paquetes NuGet; la primera restauración necesita acceso a NuGet.
5. Ejecuta con **F5**.

Los archivos de proyecto usan el formato SDK, pero el destino de ejecución es **.NET Framework 4.8**. El SDK instalado con Visual Studio sirve para compilar; la aplicación no se ejecuta sobre .NET 10.

En el primer arranque EF6 crea la base, la tabla `Personas` y `__MigrationHistory` mediante la migración explícita `Inicial`. En los siguientes arranques solo aplica migraciones pendientes. No ejecuta `DropCreateDatabase` ni recrea la base para actualizarla. No se incluye una base MDF vacía para copiar encima de datos existentes.

La instancia esperada es `(LocalDB)\MSSQLLocalDB`. Puedes comprobar su instalación con:

```powershell
sqllocaldb info
sqllocaldb info MSSQLLocalDB
```

Si LocalDB está instalado pero no existe la instancia predeterminada:

```powershell
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

## Uso

- Escribe un nombre y pulsa **Crear**.
- Selecciona una fila: modifica el nombre y pulsa **Guardar cambios**.
- **Nuevo / cancelar** limpia la selección y vuelve al modo de creación.
- **Eliminar** solicita confirmación antes de borrar.
- **Actualizar lista** vuelve a consultar los datos y descarta la edición actual.

## Ubicación de datos

```text
%LOCALAPPDATA%\CrudPersonas\Datos\CrudPersonas.mdf
```

SQL Server gestiona también el archivo de log asociado. La aplicación define `DataDirectory` antes de conectar, tanto al crear como al actualizar la base. La ruta es estable entre compilaciones y está fuera del directorio del ejecutable. Los datos pertenecen al usuario de Windows, no se comparten automáticamente entre cuentas.

Configuración: `src/UI/App.config`. No copies un MDF en uso para respaldarlo: utiliza un respaldo consistente de SQL Server. No cambies la ruta de un MDF ya adjuntado sin gestionar antes su conexión/adjunto en LocalDB.

Los errores se guardan en:

```text
%LOCALAPPDATA%\CrudPersonas\Logs\errores.log
```

## Responsabilidades y referencias

| Proyecto | Responsabilidad | Referencias del proyecto |
|---|---|---|
| Domain | Persona y validación del nombre; sin EF ni WinForms | Ninguna |
| Application | Casos de uso, DTO y contrato `IPersonaRepository` | Domain |
| DataAccess | Contexto EF6, mapeo, repositorio y migraciones | Domain, Application |
| UI | Formulario y composición de las dependencias en `Program.Main` | Application, DataAccess |

Flujo: `MainForm` llama a `PersonaService`; el servicio valida con `Persona` y utiliza `IPersonaRepository`; `PersonaRepository` abre un contexto corto con `Using`, persiste y lo libera. El formulario nunca construye consultas SQL ni utiliza un `DbContext`.

UI conoce DataAccess únicamente en el arranque para construir el repositorio y ejecutar las migraciones. Los casos de uso no conocen la implementación del almacenamiento. No se utiliza un contenedor de inyección de dependencias: el cableado es explícito.

## Migraciones

- `Migrations/202610240001000_Inicial.vb`: cambios `Up` / `Down`.
- `Migrations/Inicial.Designer.vb`: identificador y snapshot del modelo comprimido para EF6.
- `Migrations/Inicial.edmx`: copia legible de ese snapshot histórico, no es un modelo Database First ni se carga en runtime.
- `Migrations/Configuration.vb`: configuración y clave estable del contexto.
- `DatabaseInitializer.Actualizar`: ejecuta `DbMigrator.Update` antes del formulario.

**Las migraciones automáticas están deshabilitadas.** Que la base se actualice al arrancar significa ejecutar migraciones explícitas previamente incluidas en la compilación; no inferir cambios destructivos en producción. Si el esquema no corresponde con el modelo y falta una migración, hay que corregir la entrega.

Para un cambio futuro:

1. Modifica la entidad y su mapeo cuando corresponda.
2. Compila la solución.
3. En Visual Studio abre **Herramientas → Administrador de paquetes NuGet → Consola del Administrador de paquetes**.
4. Con `CrudPersonas.UI` como proyecto de inicio, ejecuta:

```powershell
Add-Migration AgregarCampo -ProjectName CrudPersonas.DataAccess -StartUpProjectName CrudPersonas.UI -ConfigurationTypeName CrudPersonas.DataAccess.Migrations.Configuration
```

5. Revisa `Up` y `Down`, especialmente cualquier pérdida o transformación de datos. Conserva también los archivos de metadatos/snapshot generados.
6. Prueba sobre una copia respaldada y compila nuevamente. Al iniciar la nueva versión se aplicarán los cambios pendientes.

No ejecutes `Enable-Migrations`: la configuración ya existe. No edites la migración inicial ni su snapshot después de aplicarla; agrega otra migración. No apuntes este ejemplo a las bases de producción heredadas: aún no hay una estrategia de adopción de sus esquemas.

## Comprobación en Windows

Desde **Developer PowerShell for VS 2022**, situado en la carpeta de la solución:

```powershell
.\Verify-Windows.ps1
```

El script compila la solución y ejecuta una comprobación real de EF6 y LocalDB con un catálogo único y un MDF temporal. Comprueba la migración inicial, su historial, compatibilidad del modelo, rechazo de nombres vacíos/largos, creación, consulta, edición, persistencia después de otra inicialización y eliminación. Al terminar intenta eliminar exclusivamente su catálogo temporal. Si no consigue limpiar, informa dónde quedaron los archivos.

Esto no sustituye la prueba visual del formulario. Sigue `docs/Pruebas-manuales.md`.

## Alcance y límites

- Un único catálogo local; no incluye base nacional, sincronización ni ClickOnce.
- CRUD síncrono pequeño, sin tareas de fondo ni paginación.
- Una instancia por sesión de Windows para evitar migraciones simultáneas desde el propio programa. No proporciona bloqueo frente a otras herramientas o sesiones.
- No hay detección de conflictos de edición: para el ejemplo, la última edición completada prevalece.
- El arranque falla si no puede aplicar una migración: no abre el CRUD sobre un esquema incompatible.
- El ejemplo no incluye respaldo/restauración automática, control de permisos de producción ni reversión coordinada de aplicación y base. Antes de desplegar futuras migraciones con datos reales hay que incorporar y probar ese procedimiento.
- EF aísla parte del acceso, pero migraciones y proveedor siguen siendo específicos de SQL Server.

## Validación de esta entrega

Se revisaron las referencias entre proyectos, la configuración XML, la estructura de solución y el snapshot inicial. **No se compiló ni ejecutó en el entorno de creación**, que no dispone de Windows, compilador .NET ni LocalDB. `Verify-Windows.ps1` queda incluido para realizar esa validación con las herramientas necesarias. No se entregan binarios precompilados.
