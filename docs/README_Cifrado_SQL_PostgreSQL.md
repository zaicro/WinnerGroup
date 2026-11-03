# Prueba de cifrado: SQL Server y PostgreSQL — VB.NET / .NET Framework 4.8

Preparado el 3 de noviembre de 2026. Laboratorio para Windows, con datos ficticios.

## 1. Qué vas a comprobar

El objetivo es que alguien que obtiene **los archivos de datos, sin las claves**, no pueda leer el dato de prueba en otra instancia. La aplicación debe seguir consultándolo normalmente en la instancia autorizada.

Se incluyen:

- Una solución de consola VB.NET `net48`, con un menú para crear y consultar una tabla de dos columnas.
- SQL Server: creación con EF6, consultas con ADO.NET y scripts de TDE.
- PostgreSQL: creación y consultas con Npgsql, tablas `tde_heap` y WAL cifrado con Percona `pg_tde`.
- Pruebas de copia física, sin clave y con clave.

**No se utiliza cifrado de columnas ni se implementa criptografía en VB.** Lo hace el motor. Las claves de TDE no van en la cadena de conexión.

Este proyecto no incorpora todavía un actualizador productivo ni migraciones de versiones de la aplicación: crea el esquema inicial. No elimina ni recrea bases existentes. No lo conectes a las bases reales.

## 2. Diferencia importante entre las dos pruebas

| Aspecto | SQL Server | PostgreSQL de este laboratorio |
|---|---|---|
| Motor | SQL Server 2022 Developer | Percona Distribution for PostgreSQL 17.11 con `pg_tde` |
| Ejecución | Servicio Windows | Contenedor Linux en Docker Desktop/WSL2 |
| Cifrado | TDE, datos y log | `tde_heap` para tabla/índice y cifrado WAL activado por separado |
| Clave | Certificado y clave privada administrados por SQL Server | Principal key en un archivo externo a PGDATA |
| Archivos copiados | MDF + LDF, o un backup físico | Copia consistente del volumen con PGDATA |
| Aplicación | EF6 + `System.Data.SqlClient` | Npgsql 8.0.8 |

**PostgreSQL comunitario instalado normalmente en Windows no trae este TDE.** La prueba usa la distribución de Percona; no presupone que se pueda instalar esta extensión en cualquier PostgreSQL. Docker aquí facilita el laboratorio; no es una recomendación de instalar Docker en los 15 equipos.

Si producción exige PostgreSQL nativo en Windows, este laboratorio valida el mecanismo criptográfico de Percona, pero **no valida ese despliegue nativo**. Una alternativa distinta a evaluar es EDB Postgres Advanced/Extended Server con TDE, con su disponibilidad y licencia correspondientes. El instalador comunitario publicado por EDB no equivale a esos productos.

Fuentes: [Percona en Docker](https://docs.percona.com/postgresql/17/docker.html), [instalación de pg_tde](https://docs.percona.com/pg-tde/install.html), [TDE de EDB](https://www.enterprisedb.com/docs/edb-postgres-ai/platforms-and-tools/security/tde/).

## 3. Qué instalar en el PC

### Para desarrollar y compilar

1. Windows compatible con las herramientas elegidas.
2. Visual Studio 2022, con **Desarrollo de escritorio de .NET**, soporte Visual Basic y herramientas de destino .NET Framework 4.8. Una versión de Visual Studio más nueva compatible también sirve.
3. Acceso a NuGet.org para restaurar los paquetes. El proyecto incluye las referencias de compilación de .NET Framework como paquete.
4. PowerShell 5.1 o posterior para los scripts de PostgreSQL.

### Para SQL Server

1. SQL Server **2022 Developer**, instalación personalizada, instancia con nombre `LABSQL`.
2. Agrega tu usuario Windows como administrador de SQL Server durante la instalación. Para esta prueba usa autenticación Windows.
3. SQL Server Management Studio (SSMS), instalado por separado.
4. Una segunda instancia **independiente**, `LABSQLCOPY`, de la misma versión/edición para la copia. Puedes instalarla en el mismo PC con el instalador SQL Server y elegir otra instancia con nombre.

**No elijas Express ni LocalDB para TDE.** Developer permite el laboratorio, pero su licencia no permite usarlo como servidor de producción. Para llevar TDE a producción debes revisar una edición licenciada compatible.

Fuente: [ediciones de SQL Server 2022](https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-2022?view=sql-server-ver16).

### Para PostgreSQL cifrado de esta prueba

1. Docker Desktop para Windows, configurado para **contenedores Linux**, con backend WSL2 y virtualización habilitada. Reinicia si su instalación lo pide.
2. Docker Compose, incluido en Docker Desktop.
3. Acceso al registro Docker Hub para descargar la imagen de Percona fijada en `compose.yaml`.

No necesitas instalar PostgreSQL comunitario ni pgAdmin en Windows para esta ruta. `psql` viene dentro del contenedor. Si ya tienes PostgreSQL en el puerto 5432, esta prueba usa **55432** en Windows para evitar el conflicto.

Descargas: [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads), [SSMS](https://learn.microsoft.com/en-us/ssms/install/install), [Visual Studio](https://visualstudio.microsoft.com/downloads/), [Docker Desktop con WSL2](https://docs.docker.com/desktop/setup/install/windows-install/).

## 4. Qué necesita la aplicación para conectar

| En un equipo que solo ejecuta el EXE | Requisito |
|---|---|
| Runtime | .NET Framework 4.8 instalado. No necesita Visual Studio ni el SDK. |
| Archivos | **Todo** el contenido de la carpeta de salida, incluidas DLL, `.exe.config` y `Connections.local.config`. No copiar solo el EXE. |
| SQL Server local | Servicio de la instancia funcionando, base accesible y usuario con permisos. |
| PostgreSQL local | Servidor funcionando; en esta prueba Docker Desktop y el contenedor deben estar activos. |
| Bibliotecas | Se despliegan con la aplicación, obtenidas por NuGet al compilar. No es necesario registrar Npgsql globalmente. |
| Claves TDE | Accesibles al motor autorizado. No se envían al EXE. |

Paquetes fijados en el proyecto:

- `EntityFramework` **6.5.1** para crear el esquema SQL Server.
- `Npgsql` **8.0.8**, ADO.NET para PostgreSQL; su destino .NET Standard 2.0 es compatible con .NET Framework 4.8.
- `Microsoft.NETFramework.ReferenceAssemblies` **1.0.3**, solo para compilar.
- `System.Data.SqlClient` y `System.Configuration` son referencias del framework.

Npgsql 9 o posterior no es sustituto directo en este proyecto: la rama 8 es la última que soporta .NET Framework mediante .NET Standard 2.0. Se usa ADO.NET para PostgreSQL porque su proveedor EF6 es heredado y ya no se mantiene activamente; esto no impide automatizar scripts de esquema posteriormente.

Fuentes: [Npgsql 8](https://www.npgsql.org/doc/release-notes/8.0.html), [paquete Npgsql 8.0.8](https://www.nuget.org/packages/Npgsql/8.0.8), [EF6 para PostgreSQL](https://www.npgsql.org/ef6/).

## 5. Abrir y ejecutar la consola

1. Descomprime en una carpeta de trabajo, por ejemplo `C:\Labs\DbEncryptionLab`.
2. Abre `DbEncryptionLab.sln` en Visual Studio.
3. Revisa `src\DbEncryptionLab\Connections.local.config`.
4. La conexión SQL predeterminada apunta a `.\LABSQL`, base `DemoCifradoSql`, con tu usuario Windows.
5. La conexión PG apunta a `127.0.0.1:55432`, base `demo_cifrado_pg`, usuario `postgres`. Deja `CAMBIAR_EN_CONSOLA`: la consola pedirá la contraseña sin mostrarla y la conservará solo durante el proceso.
6. Restaura NuGet, compila en `Release` y ejecuta con **Ctrl+F5**.

Salida habitual: `src\DbEncryptionLab\bin\Release\net48\DbEncryptionLab.exe`.

Alternativa desde Developer PowerShell de Visual Studio, en la carpeta raíz:

```powershell
msbuild .\DbEncryptionLab.sln /restore /p:Configuration=Release
.\src\DbEncryptionLab\bin\Release\net48\DbEncryptionLab.exe
```

Las variables de entorno `LAB_SQL_CONNECTION` y `LAB_PG_CONNECTION`, si existen, tienen prioridad sobre el archivo. Si algo conecta a un servidor inesperado, compruébalas. La consola exige los nombres de base de laboratorio indicados arriba.

Los ajustes `TrustServerCertificate=True` de SQL y `SSL Mode=Disable` de PG son para estas conexiones locales de laboratorio. **No activan TDE** ni sustituyen el cifrado del almacenamiento. Si cambias a conexiones remotas, configura TLS y certificados apropiados.

## 6. Prueba SQL Server: crear, cifrar y consultar

### A. Crear el dato

1. Comprueba en SSMS que conectas a `.\LABSQL`.
2. Ejecuta en la consola **1**. EF6 crea `DemoCifradoSql`, tabla `dbo.DatoPrueba`, y un dato si la tabla está vacía.
3. Ejecuta **2**. Debe mostrar `DATO-LAB-7319: prueba de cifrado transparente`.
4. Ejecuta **3**. Antes de activar TDE puede no existir DEK. Consultar correctamente todavía no demuestra cifrado.

### B. Preparar carpetas y permisos

En Windows crea estas carpetas:

```powershell
New-Item -ItemType Directory -Force C:\DbEncryptionLab\SqlKeys
New-Item -ItemType Directory -Force C:\DbEncryptionLab\SqlBackups
New-Item -ItemType Directory -Force C:\DbEncryptionLab\SqlCopyData
```

Consulta en SQL Server Configuration Manager la cuenta que ejecuta cada servicio. Si usaste las cuentas virtuales habituales, en PowerShell **como administrador**:

```powershell
icacls C:\DbEncryptionLab\SqlKeys /grant 'NT SERVICE\MSSQL$LABSQL:(OI)(CI)M'
icacls C:\DbEncryptionLab\SqlBackups /grant 'NT SERVICE\MSSQL$LABSQL:(OI)(CI)M'
icacls C:\DbEncryptionLab\SqlKeys /grant 'NT SERVICE\MSSQL$LABSQLCOPY:(OI)(CI)R'
icacls C:\DbEncryptionLab\SqlBackups /grant 'NT SERVICE\MSSQL$LABSQLCOPY:(OI)(CI)R'
icacls C:\DbEncryptionLab\SqlCopyData /grant 'NT SERVICE\MSSQL$LABSQLCOPY:(OI)(CI)M'
```

Si tus servicios usan otras cuentas, reemplázalas. Estas rutas las accede el **servicio SQL**, no solamente tu usuario de SSMS. No concedas permisos a `Everyone` para resolverlo.

### C. Activar TDE

1. Abre `sqlserver\01-Enable-Tde.sql` en SSMS, conectado a `.\LABSQL`.
2. Cambia `CAMBIAR_MASTER_PASSWORD` y `CAMBIAR_EXPORT_PASSWORD` por contraseñas de laboratorio distintas. Conserva la de exportación para la prueba positiva.
3. Ejecuta el script. Respalda el certificado/clave antes de habilitar TDE.
4. Ejecuta `02-Status.sql` hasta que `encryption_state` sea **3**. Un porcentaje 0 con estado 3 es normal: el proceso ya terminó.
5. Repite **2** y **3** en la consola. El mismo dato debe seguir siendo legible, sin añadir claves TDE a VB ni cambiar la consulta.

Si repites el script 01, los archivos `.cer`/`.pvk` ya existentes provocan error. Usa nuevos nombres de salida de forma consistente; no elimines tu única copia de claves.

Fuente: [TDE de SQL Server y conservación del certificado](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/transparent-data-encryption?view=sql-server-ver16).

## 7. Prueba SQL Server: alguien obtiene el MDF y el LDF

**Haz primero esta prueba negativa antes de importar el certificado a LABSQLCOPY.** Otro nombre de base dentro de LABSQL no sirve: la instancia original ya tiene la clave.

### A. Obtener una copia consistente

1. Cierra la consola para liberar conexiones.
2. En `.\LABSQL` ejecuta `02-Status.sql` y anota las rutas reales de MDF/LDF.
3. En una ventana SSMS conectada a `.\LABSQL`, con contexto `master`:

```sql
USE master;
ALTER DATABASE DemoCifradoSql SET OFFLINE WITH ROLLBACK IMMEDIATE;
```

4. Copia **ambos archivos**, sin mover los originales, a:
   - `C:\DbEncryptionLab\SqlCopyData\DemoCifradoSqlCopia.mdf`
   - `C:\DbEncryptionLab\SqlCopyData\DemoCifradoSqlCopia_log.ldf`
5. Devuelve enseguida la base original a ONLINE, incluso si no pudiste copiar:

```sql
USE master;
ALTER DATABASE DemoCifradoSql SET ONLINE;
```

### B. Intento sin certificado

1. Abre una nueva conexión SSMS a `.\LABSQLCOPY`.
2. Confirma que no has ejecutado allí el script 05 y que esa instancia no tiene el certificado original.
3. Ejecuta `06-Attach-Copied-Files.sql`.
4. Debe fallar por no encontrar el certificado/clave necesario para descifrar. Guarda el mensaje completo.

Un error de permisos de carpeta, versión de SQL o archivo en uso **no demuestra cifrado**. Corrige esa causa antes de interpretar la prueba.

### C. Control con certificado correcto

1. En `.\LABSQLCOPY`, abre `05-Import-Certificate.sql`.
2. Cambia la contraseña de la master key destino por una nueva. La contraseña de exportación debe ser la misma del script 01.
3. Ejecuta el script 05.
4. Repite el script 06: ahora debe adjuntar y consultar el dato.

Conclusión válida: los mismos archivos no se abren en una instancia compatible sin clave, pero sí se abren con ella. A partir de aquí LABSQLCOPY ya no es un destino limpio para otra prueba negativa.

### Alternativa: probar con un backup físico

En vez de los pasos MDF/LDF puedes usar `03-Backup.sql` y después `04-Restore-Copy.sql`, primero sin importar el certificado y luego importándolo con el script 05. Usa un destino limpio y un backup nuevo, no un archivo con varios juegos anteriores. Antes de la restauración positiva ejecuta `RESTORE FILELISTONLY` y ajusta los nombres lógicos del script 04 a los que muestre; no tienen por qué coincidir con los nombres físicos. `WITH FILE=1` selecciona el primer juego del archivo.

No pruebes ambas variantes negativas consecutivamente en una instancia a la que ya importaste la clave. Para repetir desde cero usa otra instancia de laboratorio sin el certificado.

Fuente: [mover una base protegida con TDE](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/move-a-tde-protected-database-to-another-sql-server?view=sql-server-ver16).

## 8. Prueba PostgreSQL: preparar motor y claves

Desde PowerShell, en la carpeta `postgres`:

```powershell
Copy-Item .env.example .env
notepad .env
```

1. Reemplaza el valor de `POSTGRES_PASSWORD` por una contraseña de prueba. Para simplificar el archivo usa letras, números y guion bajo. No uses datos reales.
2. Guarda `.env`. No lo subas a Git.
3. Inicia Docker Desktop y comprueba `docker version`.
4. Ejecuta:

```powershell
docker compose up -d
docker compose ps
docker compose logs --tail 60
```

Espera a que el contenedor aparezca `healthy`. Si la imagen no se pudo descargar, resuelve primero conectividad o disponibilidad del tag; no cambies a una imagen comunitaria esperando que tenga `pg_tde`.

Prepara la clave y activa WAL antes de insertar el dato:

```powershell
.\Setup-PgTde.ps1
```

El script copia y ejecuta `00-Global-Key.sql` con `ON_ERROR_STOP`, configura un proveedor global en `/keys/lab-global.per`, activa WAL y reinicia el contenedor. Al final debe mostrar **on**.

La clave se guarda en `db-encryption-lab-pgkeys`; los datos, en `db-encryption-lab-pgdata`. **Son volúmenes distintos.** El archivo de claves es un mecanismo de laboratorio; no está cifrado. No lo copies junto con los datos para la prueba negativa.

Si PowerShell bloquea scripts descargados, revisa los archivos y desbloquea específicamente los que vas a ejecutar con `Unblock-File .\Setup-PgTde.ps1` y `Unblock-File .\Test-Copied-Data.ps1`, sujeto a la política de tu equipo. No necesitas deshabilitar globalmente la política de ejecución.

Fuentes: [proveedor de archivo](https://docs.percona.com/pg-tde/global-key-provider-configuration/keyring.html), [clave principal global](https://docs.percona.com/pg-tde/global-key-provider-configuration/set-principal-key.html), [WAL](https://docs.percona.com/pg-tde/wal-encryption.html).

## 9. PostgreSQL: crear y consultar desde VB

1. Ejecuta la consola VB.
2. Selecciona **4** e introduce la contraseña de `.env` cuando la pida.
3. La consola crea `demo_cifrado_pg`, habilita la extensión en esa base, crea `public.dato_prueba` con `USING tde_heap` y agrega un dato.
4. Selecciona **5**: debe mostrar el mensaje.
5. Selecciona **6**: debe mostrar versión de extensión, WAL **on** y tabla cifrada **True**.

La consola comprueba WAL antes de insertar y rechaza una tabla existente que no esté cifrada. No hay que pasar el archivo de claves a Npgsql. La contraseña de conexión autentica al usuario; no es la clave TDE.

Puedes verificar desde el motor:

```powershell
docker exec --user postgres db-encryption-lab-pg psql -X -U postgres -d demo_cifrado_pg -c "SELECT pg_tde_is_encrypted('public.dato_prueba');"
docker exec --user postgres db-encryption-lab-pg psql -X -U postgres -d demo_cifrado_pg -c "SHOW pg_tde.wal_encrypt;"
```

Fuente: [validar tablas cifradas](https://docs.percona.com/pg-tde/test.html).

## 10. PostgreSQL: probar una copia sin claves y otra con claves

Cierra la consola VB. En PowerShell, dentro de `postgres`:

```powershell
.\Test-Copied-Data.ps1
```

El script:

1. Consulta el dato original y muestra su estado de cifrado.
2. Detiene limpiamente el contenedor original para hacer una copia consistente.
3. Copia el volumen de datos a **dos volúmenes nuevos** y la clave a otro volumen de control.
4. Reinicia la instancia original en un bloque `finally`.
5. Abre una copia con la clave correcta y verifica que consulta el dato.
6. Intenta abrir otra copia con un directorio de claves vacío. Según el punto donde se necesite la clave, puede fallar el arranque o la consulta.
7. Muestra los logs; solo informa el resultado esperado si el fallo tiene evidencia relacionada con claves. Un fallo genérico se marca **inconcluso**.

Los contenedores de copia usan el mismo motor y no publican puertos. Los datos originales y sus claves no se borran. La copia positiva tiene su propia copia de claves: no modifica el volumen original.

Comprueba al terminar que el original vuelve a funcionar con la opción **5** de VB. La pérdida de claves **no** es una solución para impedir acceso: también te impide recuperar tus propios datos.

### Qué demuestra y qué no demuestra pg_tde

La tabla y sus índices están cifrados y el laboratorio habilita WAL desde antes de escribir el dato. No es cifrado de todos los bytes del cluster: los catálogos, metadatos y estadísticas internas no están cifrados; estadísticas pueden contener muestras de valores. Los archivos temporales por desbordamientos de consultas tampoco están cubiertos por esta versión. Un `pg_dump` lógico puede exportar datos legibles si el usuario tiene acceso.

Por tanto, un fallo de lectura sin clave valida esta prueba concreta, pero no basta para aprobar la confidencialidad completa de todas las copias, estadísticas, temporales y exportaciones de una aplicación productiva.

Fuentes: [alcance](https://docs.percona.com/pg-tde/index/tde-encrypts.html), [limitaciones](https://docs.percona.com/pg-tde/index/tde-limitations.html).

## 11. Errores habituales y cómo continuar

| Error | Qué revisar |
|---|---|
| No encuentra `.\LABSQL` | Nombre real de instancia y servicio iniciado. SSMS debe conectar al mismo nombre. |
| No puede crear la base | Usa el usuario administrador de laboratorio. En producción se separan permisos de actualización y consulta. |
| TDE no soportado | Comprueba versión/edición con `02-Status.sql`; Express/LocalDB no sirven para esta prueba. |
| SQL: acceso denegado al archivo | Permisos de la cuenta del servicio sobre la carpeta, no solo de tu usuario Windows. |
| SQL: certificado de exportación ya existe | Script 01 ya se ejecutó. Conserva claves y cambia nombres de salida si necesitas otro respaldo. |
| SQL: falta permiso para DMV | Usa el administrador del laboratorio; no concedas administración permanente al usuario de la aplicación. |
| Restauración: nombre lógico incorrecto | Ejecuta FILELISTONLY después de importar el certificado; ajusta los MOVE. |
| `pg_tde` no existe | Imagen equivocada, extensión no precargada o versión distinta. Consulta logs y versión. |
| No encuentra las funciones `pg_tde_*` | Revisa extensión/versión. El laboratorio está escrito para la API documentada de pg_tde 2.2.x, no ejemplos antiguos 1.x. |
| PG: password authentication failed | Usa la contraseña original del volumen. Cambiar `.env` no cambia la contraseña de un cluster ya inicializado. |
| Setup dice que la clave ya existe | No la borres. Revisa si terminó la configuración; ver comandos de recuperación debajo. |
| Falta una DLL/System.Memory al ejecutar | Copia la salida completa; limpia bin/obj, restaura y recompila. El proyecto habilita binding redirects automáticos. |
| `ExecutionPolicy` corporativa bloquea PS | Sigue la política de tu organización o ejecuta manualmente los SQL y comandos Docker revisados; no la evadas. |

Recuperación si el setup quedó parcialmente ejecutado: conserva el volumen de claves, revisa qué sentencias del script 00 terminaron y completa solo las pendientes. No vuelvas a generar una clave con el mismo nombre a ciegas. Para revisar WAL:

```powershell
docker exec --user postgres db-encryption-lab-pg psql -X -U postgres -d postgres -c "SHOW pg_tde.wal_encrypt;"
docker logs db-encryption-lab-pg --tail 100
```

## 12. Detener y limpiar sin perder el original

Para parar el laboratorio PG y conservar datos/claves:

```powershell
docker compose stop
```

Para reanudarlo:

```powershell
docker compose start
```

Para repetir **solo** la prueba de copias, elimina sus contenedores/volúmenes después de revisar los resultados. Estos nombres NO corresponden al original:

```powershell
docker rm -f db-encryption-lab-copy-with-keys db-encryption-lab-copy-no-keys
docker volume rm db-encryption-lab-copy-data-with-keys db-encryption-lab-copy-data-no-keys db-encryption-lab-copy-keys db-encryption-lab-empty-keys
```

No uses `docker compose down -v` para simplemente detener: elimina también los volúmenes de datos y claves de la definición. No se incluye limpieza automática de SQL Server para evitar perder accidentalmente bases o certificados.

## 13. Resultado a registrar mañana

| Comprobación | Resultado esperado | Resultado observado |
|---|---|---|
| SQL: consulta antes de TDE | Dato legible | |
| SQL: estado después de habilitar TDE | encryption_state = 3 | |
| SQL: misma consulta después de TDE | Dato legible sin cambiar VB | |
| SQL: archivos en instancia sin certificado | Falla por certificado/clave | |
| SQL: mismos archivos con certificado | Dato legible | |
| PG: estado de tabla y WAL | True y on | |
| PG: consulta original | Dato legible | |
| PG: copia con clave | Dato legible | |
| PG: copia sin clave | Fallo relacionado con claves, no un error de red | |
| PG: original después de la prueba | Dato legible | |

## 14. Alcance y validación de la entrega

Se revisaron estructura del proyecto, referencias, XML de configuración, scripts y consistencia de nombres/rutas. **No se compiló ni se ejecutó el proyecto en este entorno:** aquí no hay .NET SDK/MSBuild, Windows, SQL Server ni Docker. Las pruebas de integración descritas quedan para tu PC. No se incluye un EXE sin verificar.

Para producción faltaría diseñar custodia/recuperación de claves, despliegue del motor y migraciones probadas. Los usuarios con acceso autorizado al motor siguen pudiendo consultar; estas pruebas tratan la copia de archivos sin claves. En PostgreSQL el archivo de claves externo es suficiente para comparar los controles del laboratorio, pero un KMS es una opción más adecuada para separar la custodia en producción, según conectividad y operación requerida.

**Siguiente paso recomendado:** ejecuta primero SQL Server y registra la prueba MDF/LDF. Después ejecuta PostgreSQL y compara tanto el resultado criptográfico como el coste operativo de mantenerlo localmente en Windows. No elijas el motor solo por haber conseguido una consulta exitosa.
