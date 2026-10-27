# TableGamesAuth — VB.NET / .NET Framework 4.8 / EF6

API de administración de usuarios y autenticación. Solución completa de código fuente con diez proyectos: ocho de implementación y dos de pruebas. Empiece con SQL Server; el proveedor PostgreSQL está implementado y requiere validar su integración en su entorno.

## Decisiones que conviene conocer

- Un usuario tiene **un rol global** y acceso a cero o más aplicaciones. `UserApps` es la relación N:M. Para roles distintos en Roulette y Baccarat, mueva `RoleId` de `Users` a `UserApps` antes de empezar a cargar usuarios reales.
- Los CRUD administrativos requieren **API key Y JWT**, y rol `SystemAdmin`. Login requiere API key; `/auth/me` acepta cualquier usuario autenticado con acceso a esa aplicación. No hay registro público ni creación anónima del primer administrador.
- La API recibe `Password` por HTTPS y guarda `PasswordHash`. También admite importar `PasswordHash` en el CRUD administrativo, exclusivamente en el formato PBKDF2 definido aquí. El hash no es una contraseña de login. Un SHA256 previo del cliente no sustituye esta lógica.
- El host es **OWIN self-host con HttpListener**, ejecutable Windows de consola. No es ASP.NET Core ni un proyecto web de IIS. El alojamiento y la infraestructura HTTP se pueden reemplazar sin cambiar Application y Domain.
- Las migraciones se ejecutan explícitamente; el arranque normal no modifica el esquema.

## Organización y dependencias

| Proyecto | Responsabilidad |
|---|---|
| `Domain` | Entidades sin EF, HTTP, criptografía o logging concreto |
| `Application` | Casos de uso, DTO, validación e interfaces de repositorios, UoW, hash, tokens y logger |
| `DataAccess.Ef6` | Mapeos, repositorios y transacciones comunes a proveedores EF6 |
| `DataAccess.Sql` | Contexto SQL Server y configuración de migraciones |
| `DataAccess.Postgres` | Contexto PostgreSQL y configuración de migraciones |
| `Security` | PBKDF2 y emisión/validación JWT |
| `Logging` | Adaptador de `IAppLogger` para log4net |
| `Api` | Controllers, seguridad HTTP, host y composición de dependencias |
| `Tests` | Pruebas unitarias ejecutables, sin base de datos |
| `IntegrationTests` | Migraciones, datos reales y endpoints mediante HttpServer en memoria |

`Api` es la raíz de composición y conoce las implementaciones. `Application` solo depende de `Domain`. Los puertos de persistencia están en Application; ningún `IQueryable`, `DbContext` ni excepción de proveedor cruza hacia los servicios. Para otro motor, cree una implementación de `IUnitOfWork` y sus repositorios, y registre la implementación en `Composition.vb`. Si el motor usa EF6, puede reutilizar `DataAccess.Ef6`; si no usa EF, implemente los mismos contratos por separado.

La unidad de trabajo tiene alcance **por petición**, nunca singleton. Repositorios de una petición comparten contexto y transacción. Los servicios realizan `BeginTransaction → SaveAsync → Commit`. `Dispose` sin commit hace rollback, incluso si hubo varios `SaveChanges` exitosos antes del error. Después de un rollback se descarta el contexto; el rollback de SQL no revierte los objetos ya mutados en memoria. No se soportan transacciones anidadas ni operaciones paralelas sobre el mismo contexto.

Cada entidad usa `Version: Guid` como token de concurrencia portable. Un PUT recibe la versión del GET anterior; un DELETE usa `?version=GUID`. EF comprueba también la versión original en el UPDATE/DELETE real. Las restricciones únicas y claves foráneas protegen contra carreras que los chequeos de aplicación no pueden evitar. No hay cascadas de borrado.

## Requisitos

Windows, .NET Framework 4.8, Visual Studio 2022 con soporte VB.NET y targeting pack 4.8, o .NET SDK 8+ para compilar los proyectos SDK. Los binarios **se ejecutan con Framework 4.8 en Windows**; instalar un SDK moderno no cambia el framework de destino. Necesita SQL Server o PostgreSQL y permisos para crear/actualizar el esquema durante despliegue.

Versiones fijadas: EF 6.5.2, Web API OWIN 5.3.0, OWIN 4.2.3, DI 8.0.1, JWT 8.23.0, log4net 3.5.0, EntityFramework6.Npgsql 6.4.3 y Npgsql 4.1.14. No actualice Npgsql a una versión mayor por separado sin comprobar compatibilidad con el proveedor EF6.

## Primera ejecución: SQL Server

Desde la carpeta de la solución, en PowerShell:

```powershell
dotnet restore .\TableGamesAuth.sln
dotnet build .\TableGamesAuth.sln -c Release

. .\scripts\Configure-Local.ps1 -Provider SqlServer -ConnectionString 'Server=localhost;Database=TableGamesAuth;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'
.\scripts\Initialize-Local.ps1 -UserName admin
Start-Process .\src\Api\bin\Release\net48\TableGamesAuth.Api.exe
```

`Initialize-Local.ps1` aplica la migración, solicita una contraseña mediante entrada segura y crea el administrador. Bootstrap solo funciona cuando no hay usuarios. El administrador recibe acceso a Administración; los usuarios posteriores necesitan asignaciones explícitas en UserApps.

`Configure-Local.ps1` genera secretos nuevos en **el entorno del proceso actual**, sin escribirlos a archivos. No vuelva a ejecutarlo si quiere conservar las sesiones y claves de esa sesión. El modo HTTP está restringido a loopback para desarrollo. Si HttpListener devuelve acceso denegado, reserve la URL con una cuenta administradora una sola vez:

```powershell
netsh http add urlacl url=http://localhost:9000/ user="DOMINIO\usuario"
```

La aplicación normal no necesita ejecutarse como administradora una vez concedida esa reserva. No sustituya localhost por `+` o `*` en desarrollo.

El comando Start-Process abre el host en otra ventana heredando la configuración. Desde la consola original puede probar el login; también puede usar `requests/TableGamesAuth.http`:

```powershell
$login = Invoke-RestMethod 'http://localhost:9000/api/v1/auth/login' -Method Post -Headers @{'X-Api-Key'=$env:TG_CLIENT_API_KEY} -ContentType 'application/json' -Body (@{userName='admin';password='SU_CONTRASENA';appId='20000000-0000-0000-0000-000000000001'} | ConvertTo-Json)
$headers = @{'X-Api-Key'=$env:TG_CLIENT_API_KEY;Authorization=('Bearer '+$login.accessToken)}
Invoke-RestMethod 'http://localhost:9000/api/v1/users' -Headers $headers
```

El ejemplo de contraseña literal es ilustrativo; evite dejar contraseñas reales en el historial de PowerShell. La API no muestra claves al arrancar. El comando opcional `--generate-secrets` imprime un juego de claves para configurarlas manualmente; guarde ese resultado como secreto.

## PostgreSQL

```powershell
. .\scripts\Configure-Local.ps1 -Provider Postgres -ConnectionString 'Host=localhost;Port=5432;Database=TableGamesAuth;Username=auth_owner;Password=SU_SECRETO;SSL Mode=Require;'
.\scripts\Initialize-Local.ps1 -UserName admin
Start-Process .\src\Api\bin\Release\net48\TableGamesAuth.Api.exe
```

Use las opciones TLS y la validación de certificado admitidas por Npgsql 4.1 en su entorno; no habilite `Trust Server Certificate=true` en producción. Los GUID los genera la aplicación: esta migración no requiere `uuid-ossp`. Los nombres de tablas y columnas son sensibles a mayúsculas al escribir SQL manual en PostgreSQL: por ejemplo `auth."Users"`.

Cambiar el proveedor apunta a **otra base de datos**, no copia datos del motor anterior. Una migración entre motores requiere una transferencia y validación aparte, incluidos IDs, hashes y relaciones.

## Migración y semilla

```powershell
.\src\Api\bin\Release\net48\TableGamesAuth.Api.exe --migrate
```

Hay una `InitialCreate` por proveedor y configuraciones de migraciones independientes. Las operaciones iniciales y el modelo V1 se comparten mientras el esquema es equivalente. La tabla `__MigrationHistory` adicional es infraestructura de EF6; las tablas de negocio son las cuatro solicitadas.

| Dato | ID fijo |
|---|---|
| SystemAdmin | `10000000-0000-0000-0000-000000000001` |
| Operator | `10000000-0000-0000-0000-000000000002` |
| Administración | `20000000-0000-0000-0000-000000000001` |
| Crazzy Roulette | `20000000-0000-0000-0000-000000000002` |
| Baccarat | `20000000-0000-0000-0000-000000000003` |

La semilla inserta por ID cuando falta el registro; no duplica ni sobreescribe nombres, estados o permisos existentes. No crea usuarios ni contraseñas. Un conflicto provocado por datos existentes con los mismos códigos y otros IDs requiere reconciliar los datos; no se ignora silenciosamente.

`ModelV1.vb` es un **snapshot inmutable**: los metadatos iniciales se producen a partir de ese modelo congelado, sin conectarse a la base. Al cambiar el esquema futuro, conserve ModelV1 y la migración inicial; mueva el mapeo vigente del contexto a una clase nueva y agregue una migración por proveedor. No edite V1 para representar el modelo nuevo.

Desde Package Manager Console de Visual Studio, para migraciones posteriores:

```powershell
Add-Migration NombreCambio -ProjectName TableGamesAuth.DataAccess.Sql -StartUpProjectName TableGamesAuth.Api -ConfigurationTypeName TableGamesAuth.DataAccess.Sql.Migrations.Configuration
Add-Migration NombreCambio -ProjectName TableGamesAuth.DataAccess.Postgres -StartUpProjectName TableGamesAuth.Api -ConfigurationTypeName TableGamesAuth.DataAccess.Postgres.Migrations.Configuration
```

Configure `TG_DB_CONNECTION` para el proveedor correspondiente antes de cada comando; revise el SQL generado. Si su versión de tooling no carga los comandos EF6 mediante PackageReference, utilice las herramientas `ef6.exe` del paquete EF6; no convierta a EF Core para ejecutar migraciones. El comando `--migrate` ejecuta las migraciones incluidas sin depender de la consola EF de Visual Studio.

## Endpoints

Contrato importable en `docs/openapi.json` (OpenAPI 3.0); ejemplos en `requests/TableGamesAuth.http`. Puede importar OpenAPI en Postman. No se expone Swagger UI en el host.

| Ruta | Métodos | Seguridad |
|---|---|---|
| `/api/v1/auth/login` | POST | API key válida para appId |
| `/api/v1/auth/me` | GET | API key + JWT |
| `/api/v1/users` | GET, POST | API key + JWT + SystemAdmin |
| `/api/v1/roles` | GET, POST | API key + JWT + SystemAdmin |
| `/api/v1/apps` | GET, POST | API key + JWT + SystemAdmin |
| `/api/v1/user-apps` | GET, POST | API key + JWT + SystemAdmin |
| Las cuatro anteriores + `/{id}` | GET, PUT, DELETE | API key + JWT + SystemAdmin |

Listados: `skip=0&take=25`, máximo 100 por página. POST devuelve 201 y Location; DELETE 204; validación 400; falta de credenciales 401; rol insuficiente 403; ausente 404; versión o restricciones 409; cuerpo mayor a 32 KiB 413; rate limit 429. No se devuelven hashes, stamps, contadores de login, excepciones internas ni cadenas de conexión. Los PUT son actualizaciones completas, excepto contraseña/hash que pueden omitirse.

## Seguridad implementada

- PBKDF2-HMAC-SHA256, 600 000 iteraciones, salt aleatorio de 16 bytes y derivado de 32 bytes. Contraseñas nuevas: 12–128 caracteres; comparación sin salida temprana en el contenido. Formato `pbkdf2-sha256$600000$BASE64_SALT$BASE64_DERIVADO`. Imports rechazan factores inferiores a 600 000 o superiores a 2 000 000.
- No cifrado reversible ni hashes deterministas de contraseña. Un hash importado admite el mismo riesgo de procedencia que cualquier dato administrativo: úselo solo para migraciones de confianza. La política histórica de una contraseña no se puede verificar viendo su hash.
- JWT HS256 firmado, 15 minutos, validación de algoritmo, firma, issuer, audience y vencimiento; tolerancia de reloj de 30 segundos, jti y app_id. La clave de firma es de al menos 256 bits aleatorios y permanece solo en el servicio.
- En cada petición autenticada se comprueba usuario activo, stamp, rol actual, app activa y asignación UserApps. Actualizar administrativamente un usuario cambia su stamp e invalida sus tokens anteriores. Eliminar una asignación o desactivar la app/usuario surte efecto en la siguiente comprobación; una petición ya autorizada puede terminar. Las consultas a DB por petición son el coste de esta revocación.
- Cinco contraseñas incorrectas bloquean 15 minutos. Se ejecuta la KDF también para usuarios desconocidos/bloqueados y la respuesta de login es genérica. Los intentos incorrectos se confirman antes de responder 401.
- API keys aleatorias por aplicación, configuración por SHA256, comparación del derivado sin salida temprana. Una misma aplicación puede tener varias claves durante rotación. SHA256 es adecuado aquí porque las claves son aleatorias de alta entropía, no contraseñas elegidas por personas.
- Límite por app de 30 logins y 600 peticiones por minuto, en memoria de cada proceso. El contador no es distribuido ni sustituye límites de gateway por IP y globales, incluido tráfico con claves inválidas.
- HTTPS obligatorio salvo loopback habilitado expresamente; sin CORS abierto por defecto y respuestas `Cache-Control: no-store`.

Configuración del servidor: `TG_DB_PROVIDER` (`SqlServer` o `Postgres`), `TG_DB_CONNECTION`, `TG_JWT_KEY` (Base64), `TG_API_KEYS` (array JSON de `{appId,sha256}`), `TG_LISTEN_URL` (por defecto `https://localhost:9443/`), `TG_ALLOW_INSECURE_LOOPBACK` (solo desarrollo). Bootstrap usa temporalmente `TG_BOOTSTRAP_USER` y `TG_BOOTSTRAP_PASSWORD`.

Para añadir una API key de Roulette/Baccarat, genere 32 bytes aleatorios en el cliente de confianza y agregue al array del servidor su SHA256 hexadecimal y el ID de esa aplicación. Proporcione el valor original solo al cliente. Reinicie el host después de cambiar configuración. Un usuario necesita además una asignación en UserApps para esa app.

## Logging

`IAppLogger` evita acoplar Application y DataAccess a log4net. `Logging/Log4NetLogger.vb` implementa el adaptador; `Api/log4net.config` configura consola y archivo rotativo de 10 MiB, hasta 10 respaldos. La ruta `logs/TableGamesAuth.log` es relativa al directorio de trabajo del proceso; concédale permisos de escritura a la cuenta del servicio y restrinja lectura de los archivos.

Cada respuesta añade `X-Correlation-ID`. Se registran método, estado HTTP, duración, correlación, éxito/rechazo de login por app y errores. No se registran cuerpos, cabeceras, consultas URL, nombre de usuario o credenciales. Para excepciones se registra tipo y stack, omitiendo el mensaje y la excepción interna porque un proveedor puede incluir valores sensibles. Esto reduce detalle diagnóstico a cambio de evitar fugas. Los eventos no son una auditoría inmutable de cambios administrativos; esa ampliación sigue pendiente.

## Pruebas

```powershell
.\scripts\Test.ps1
```

Son proyectos de pruebas ejecutables que devuelven exit code 0/1 y no requieren un adaptador de Visual Studio Test Explorer. Se eligió este runner pequeño para mantener las pruebas ejecutables sin paquetes de framework de test adicionales. No use `dotnet test` para ejecutarlos: compile y ejecute los EXE, como hace el script.

Las unitarias verifican salt, contraseña incorrecta, imports inválidos, claims, firma manipulada, token mal formado, algoritmo incorrecto, issuer/audience/vencimiento, rol actualizado, revocación, bloqueo, persistencia de intentos fallidos, DTO y transacciones. Los dobles prueban la orquestación de commit/rollback; la reversión real de datos se prueba solo con integración. Incluyen log4net mediante MemoryAppender y comprueban que no se publique el mensaje sensible de una excepción.

Para integración, cree **bases exclusivas de prueba**, vacías la primera vez y terminadas en `_tests`:

```powershell
$env:TG_INTEGRATION_SQL_CONNECTION = 'Server=localhost;Database=TableGamesAuth_tests;Integrated Security=True;Encrypt=True;TrustServerCertificate=True;'
$env:TG_INTEGRATION_POSTGRES_CONNECTION = 'Host=localhost;Database=TableGamesAuth_tests;Username=auth_test;Password=SU_SECRETO;'
.\scripts\Test.ps1 -Integration
```

Se ejecuta cada proveedor configurado. Un proveedor sin conexión se marca SKIP; si no hay ninguno configurado, falla el runner. Se aplican migraciones y semilla dos veces, se verifican claves únicas y foráneas, concurrencia entre dos contextos y rollback real de una escritura exitosa cuando falla una posterior. Se prueban los endpoints mediante **HttpServer en memoria y DB real**, incluyendo login, ambas credenciales, permisos, campos sensibles, CRUD de las cuatro tablas, versión obsoleta y revocación. Esto no prueba la red HttpListener, certificados o proxy: esos requieren una prueba de despliegue adicional.

Las pruebas agregan registros con sufijos aleatorios y no borran automáticamente la base. Use bases desechables, no las de desarrollo compartidas. No se incluyen credenciales de prueba en el repositorio.

## Estado de validación de esta entrega

Consulte `docs/VALIDATION.md`. Se compilaron las diez salidas contra referencias de Framework 4.8 y se ejecutaron 45 comprobaciones unitarias sobre las mismas fuentes en un runtime auxiliar .NET 8. No se ejecutó el runtime nativo Windows/Framework 4.8, la solución mediante MSBuild completo ni la integración real con motores de datos en este entorno. La suite de integración compiló, pero requiere ejecutar ambos proveedores antes de afirmar que el conjunto está validado de extremo a extremo.

## Recomendaciones prioritarias

1. **Decidir el alcance del rol antes de usar datos reales.** Para las dos aplicaciones, un rol por usuario/aplicación probablemente sea más flexible que el rol global adoptado aquí. El cambio afecta el esquema y las reglas de autorización; no es solo renombrar una columna.
2. **Validar SQL Server primero y después PostgreSQL.** El proveedor EF6 de PostgreSQL tiene una línea antigua y su compatibilidad no equivale a la del proveedor EF Core actual. No asuma que podrá instalar la última versión de Npgsql.
3. **Para producción:** alojamiento supervisado (servicio Windows o adaptar Api a IIS/OWIN), certificado TLS y reserva HTTPS de HttpListener, cuenta de DB con mínimos permisos, secretos en un almacén seguro, copias de seguridad y un despliegue de migraciones separado con cuenta de DDL. No deje `TrustServerCertificate=True` ni HTTP habilitado. Un proxy que termine TLS debe reenviar por HTTPS al backend; este proyecto no confía automáticamente en `X-Forwarded-Proto`.
4. **Distinguir identidad de usuario de identidad de cliente.** Una API key incorporada en una aplicación de escritorio o móvil puede extraerse: no constituye un secreto fuerte del usuario final. Para servicios de confianza sí puede complementar al JWT. Nunca entregue la clave HS256 de firma a los juegos para validar tokens; quien la tenga también puede emitirlos. Si los juegos expondrán sus propias API, añada firma asimétrica, audiences específicas e intercambio/validación de tokens adecuados; el audience actual es esta API administrativa.
5. **Antes de exposición pública:** MFA para administradores, límites distribuidos por IP/app/global, auditoría de cambios, alertas de fallos de login y un flujo seguro de recuperación/cambio de contraseña. No incorpore refresh tokens persistentes sin diseñar rotación y revocación: normalmente requieren otra tabla. Evite desactivar o quitar el último acceso administrativo sin mantener una cuenta de recuperación supervisada.
6. Añadir CI en Windows, ejecutar ambas suites y revisar dependencias con `dotnet list .\TableGamesAuth.sln package --vulnerable --include-transitive`. `NuGetAudit` está habilitado; se generarán lockfiles en el primer restore y deben incorporarse a Git después de revisarlos. No se entregan lockfiles inventados ni se afirma ausencia de vulnerabilidades sin una auditoría completa.

.NET Framework 4.8 + VB.NET + EF6 es una restricción de compatibilidad válida, pero no una plataforma nueva. La estructura permite mejorar componentes sin reescribir negocio; si en el futuro necesita un salto de plataforma, mantenga los contratos y evalúe por separado la migración a .NET/ASP.NET Core y EF Core.

## Referencias técnicas

- [Npgsql: proveedor EF6 y configuración](https://www.npgsql.org/ef6/)
- [EntityFramework 6.5.2](https://www.nuget.org/packages/EntityFramework/6.5.2)
- [EntityFramework6.Npgsql 6.4.3](https://www.nuget.org/packages/EntityFramework6.Npgsql/6.4.3)
- [Npgsql 4.1.14](https://www.nuget.org/packages/Npgsql/4.1.14)
- [ASP.NET Web API OWIN 5.3.0](https://www.nuget.org/packages/Microsoft.AspNet.WebApi.Owin/5.3.0)
- [OWASP Password Storage Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Password_Storage_Cheat_Sheet.html)
- [Apache log4net: versiones](https://logging.apache.org/log4net/release-notes.html)
