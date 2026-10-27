# Validación de la entrega

Fecha: 26 de octubre de 2026.

| Comprobación | Resultado |
|---|---|
| Compilación de las ocho capas y las dos suites | Correcta contra ensamblados de referencia de .NET Framework 4.8 |
| Reglas Option Strict / Explicit | Activadas |
| Pruebas unitarias de aplicación, hash, JWT y logger | 45 comprobaciones correctas |
| Modelo y snapshot EF de SQL Server | Construidos y descomprimidos correctamente, sin conexión a DB |
| Modelo y snapshot EF de PostgreSQL | Construidos y descomprimidos correctamente, sin conexión a DB |
| Generación SQL inicial para ambos motores | Correcta: 14 sentencias por proveedor, claves foráneas con columnas explícitas |
| XML de proyectos, referencias entre proyectos y JSON OpenAPI | Comprobados |
| MSBuild completo de la solución en Windows | Pendiente |
| Ejecución nativa .NET Framework 4.8 / HttpListener | Pendiente |
| Migración y semilla contra SQL Server real | Pendiente |
| Migración y semilla contra PostgreSQL real | Pendiente |
| Integración HTTP y rollback contra DB real | Suite compilada; ejecución pendiente |
| Auditoría completa de dependencias NuGet | Pendiente |

Se utilizó el compilador Visual Basic de Roslyn con referencias de Framework 4.8 para las diez salidas. Se comprobaron los modelos y generadores reales de EF6 para ambos proveedores sin servidor de datos.

Las pruebas unitarias ejecutadas utilizaron las mismas fuentes enlazadas de Domain, Application, Security y Logging en un runtime auxiliar .NET 8. Esto comprueba reglas y comportamiento criptográfico, pero no sustituye ejecutar las suites compiladas para net48 en Windows. Los detalles están en `unit-checks.txt`.

El entorno no dispone de Windows ni servidores SQL Server/PostgreSQL configurados. MSBuild no pudo inicializarse por una limitación del acceso del runtime a la información de procesos; se verificaron las fuentes mediante compilación directa. No se presentan estos resultados como una certificación de despliegue o una ejecución de integración real.

## Verificación pendiente en su equipo

1. `dotnet restore .\TableGamesAuth.sln` y `dotnet build .\TableGamesAuth.sln -c Release`.
2. `.\scripts\Test.ps1` para ejecutar nativamente las unitarias.
3. Configurar ambas conexiones `_tests` y ejecutar `.\scripts\Test.ps1 -Integration`.
4. Ejecutar el host y comprobar login, HTTPS, logs y cierre del proceso.
5. Auditar dependencias, revisar y guardar los lockfiles generados antes de CI/despliegue.

Las pruebas de integración rechazan nombres de DB que no terminen en `_tests`. Agregan datos aislados mediante sufijos únicos y no borran bases de datos.
