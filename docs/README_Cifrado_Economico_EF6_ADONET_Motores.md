# Cifrado económico para VB.NET 4.8: EF6, ADO.NET y motores

## 1. Criterio de comparación

Se compara exclusivamente el coste de **licencias del motor, proveedor, biblioteca y plugin de cifrado**. No se incluyen mano de obra, migración, operación, hardware ni infraestructura, por petición del usuario.

`0 USD` significa que existe una ruta sin compra de licencias bajo sus términos de uso. No significa soporte comercial incluido ni ausencia de obligaciones de distribución. El soporte contratado y los servicios cloud no son requisitos para las rutas gratuitas seleccionadas.

**Conclusión:** sí hay alternativas gratuitas para cifrar columnas y también para cifrar archivos de datos. No es correcto afirmar que hace falta comprar software para conseguirlo. La compatibilidad exacta con EF6 es la restricción que distingue varias opciones.

Objetivo de referencia: alguien copia los archivos de datos a otro PC y no puede leer el contenido protegido sin obtener también las llaves. El cifrado de disco, el cifrado de conexión y una contraseña de acceso al motor no equivalen automáticamente a esa protección.

## 2. Comparación principal

| Motor / opción | Alcance | Software adicional necesario | EF6 en .NET Framework 4.8 | ADO.NET en .NET Framework 4.8 | Licencias seleccionadas |
|---|---|---|---|---|---|
| SQL Server Express + Always Encrypted | Columnas elegidas; cifrado desde el cliente | SqlClient compatible y almacén de certificados Windows | Sí, con limitaciones de consultas y parámetros; probar el modelo real | Sí | **0 USD** |
| SQL Server Express + cifrado en la aplicación | Columnas elegidas | Biblioteca criptográfica gratuita y protección de llaves | Sí, persistiendo bytes; no es cifrado automático incorporado en EF6 | Sí | **0 USD** |
| SQLite + SQLCipher Community mediante Microsoft.Data.Sqlite | Archivo de base cifrado | Microsoft.Data.Sqlite.Core + biblioteca nativa SQLCipher gratuita | No es el proveedor EF6 System.Data.SQLite; no lo trates como sustitución directa | Sí, con versiones y binarios compatibles | **0 USD** |
| SQLite + SQLite3 Multiple Ciphers | Archivo de base cifrado | Biblioteca sqlite3mc y bindings .NET gratuitos | No hay integración directa EF6 confirmada en esta guía | Sí, mediante una combinación compatible de bindings/proveedor | **0 USD** |
| PostgreSQL + pgcrypto | Valores de columnas | Extensión pgcrypto y Npgsql | Sí, mediante SQL/funciones explícitas; no transparente para LINQ | Sí | **0 USD** |
| PostgreSQL + cifrado en la aplicación | Columnas elegidas | Biblioteca criptográfica gratuita y Npgsql | Sí, persistiendo bytes | Sí | **0 USD** |
| MariaDB Community + cifrado en reposo | Tablespaces/tablas y archivos adicionales según configuración | Plugin File Key Management incluido y MySqlConnector | TDE es transparente para un proveedor EF6 que ya sea compatible; no se confirma aquí una combinación gratuita EF6/MariaDB | Sí, con MySqlConnector compatible | **0 USD por la ruta ADO.NET** |
| Oracle AI Database Free, documentación release 26 + TDE | Columnas o tablespaces | Motor Free, ODP.NET y keystore/wallet local | Sí con proveedor ODP.NET EF6 compatible; confirmar matriz de versiones | Sí con ODP.NET compatible | **0 USD**, dentro de límites y términos Free |
| Cualquier motor soportado + cifrado en la aplicación | Solo valores cifrados antes de guardarlos | Biblioteca gratuita + proveedor del motor | Depende del proveedor; el campo cifrado se mapea como binario | Sí si hay proveedor compatible | **0 USD si motor y proveedor también son gratuitos** |

Las filas son opciones, no una lista de componentes que deban instalarse juntos. Las compatibilidades describen rutas de integración; no certifican una combinación de versiones ejecutada en este entorno.

Fuentes: [SQL Server ediciones](https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-2022), [SQLite cifrado](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/encryption), [SQLite3MC .NET](https://github.com/utelle/SQLite3MultipleCiphers-NuGet), [pgcrypto](https://www.postgresql.org/docs/current/pgcrypto.html), [MariaDB File Key Management](https://mariadb.com/docs/server/security/encryption/data-at-rest-encryption/key-management-and-encryption-plugins/file-key-management-encryption-plugin), [Oracle licenciamiento release 26](https://docs.oracle.com/en/database/oracle/oracle-database/26/dblic/Licensing-Information.html).

## 3. SQL Server Express: columnas sin comprar plugins

### Always Encrypted

Es una opción incluida en Express en las versiones documentadas. El controlador del cliente cifra y descifra los valores, mientras las claves maestras de columna se custodian fuera de la base, por ejemplo en el almacén de certificados Windows. No exige Azure Key Vault ni un HSM de pago para este diseño local.

Con EF6 se mantiene el proveedor SQL Server. Deben probarse tipos, parametrización y consultas: el cifrado aleatorio restringe operaciones, y el determinista permite ciertas comparaciones pero revela igualdad de valores. No supongas que `LIKE`, rangos y ordenamientos seguirán funcionando como antes. No se presupone uso de enclaves seguros.

Fuente: [Always Encrypted](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/always-encrypted-database-engine).

La cadena incluye esta opción además de los datos de conexión habituales:

```text
Column Encryption Setting=Enabled;
```

Eso no configura por sí solo las columnas ni crea las claves. Hay que provisionar la clave maestra, la clave de cifrado de columnas y los permisos de la cuenta de ejecución. Un certificado autofirmado local puede servir como contenedor para esta clave; no necesita comprarse a una autoridad pública para este uso.

Copiar la base sin la clave privada correspondiente no debería permitir descifrar los valores. Exportar y distribuir el PFX con su contraseña junto al MDF anularía esa separación.

### Cifrado desde VB.NET

Puedes mantener SQL Express y almacenar los valores ya cifrados en `varbinary`. EF6 administra el campo `Byte()`; el servicio/repositorio aplica el cifrado antes de guardar y lo revierte después de leer.

Una biblioteca gratuita candidata es `BouncyCastle.Cryptography`, cuyo paquete incluye un destino .NET Framework 4.6.1 compatible con 4.8 y licencia basada en MIT. Permite usar mecanismos autenticados como AES-GCM. Utiliza una versión mantenida y un diseño revisado de nonce, etiqueta y llaves; instalar la biblioteca no diseña el formato por ti.

Fuente: [paquete oficial BouncyCastle.Cryptography](https://www.nuget.org/packages/BouncyCastle.Cryptography/).

Mapeo de persistencia ilustrativo, sin implementar criptografía propia:

```vb
Public Class Registro
    Public Property Id As Integer
    Public Property DatoProtegido As Byte()
End Class
```

Flujo: valor original → cifrado autenticado → bytes almacenados. Al leer: bytes → verificación y descifrado → valor utilizado por la aplicación. No añadas un getter que descifre automáticamente si puede terminar exponiéndose en logs o serializaciones.

**No equivale a TDE:** nombres de tablas, campos no cifrados y otros metadatos siguen visibles. Las consultas operan sobre el dato cifrado salvo que diseñes expresamente mecanismos de búsqueda adicionales.

### Cifrado nativo con ENCRYPTBYKEY

SQL Server también ofrece funciones de cifrado para valores. Son consumibles mediante ADO.NET o SQL explícito desde EF6. No las trates como traducción LINQ automática. Si las eliges, diseña la jerarquía de claves, contraseñas de recuperación y copias de respaldo antes de afirmar que protegen frente a la extracción del MDF.

Fuentes: [ENCRYPTBYKEY](https://learn.microsoft.com/en-us/sql/t-sql/functions/encryptbykey-transact-sql), [claves SQL Server](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/sql-server-and-database-encryption-keys-database-engine).

### Lo que Express no incluye

Express no incluye TDE en las ediciones 2022 y 2025 consultadas. No utilices Developer como solución gratuita de producción: su licencia es de desarrollo/pruebas. Para TDE de SQL Server hay ediciones de pago; no se estima aquí su importe.

Fuente: [SQL Server 2025: ediciones](https://learn.microsoft.com/en-us/sql/sql-server/editions-and-components-of-sql-server-latest).

## 4. SQLite: archivo cifrado con software gratuito y ADO.NET

### Ruta A: SQLCipher Community

SQLCipher Community admite aplicaciones comerciales cerradas, conservando los avisos exigidos. Su núcleo es gratuito; los binarios e integraciones comerciales de Zetetic son productos distintos.

Fuente: [SQLCipher Community y licencia](https://www.zetetic.net/sqlcipher/community/).

Microsoft documenta esta ruta ADO.NET:

- `Microsoft.Data.Sqlite.Core`, sin el bundle SQLite estándar.
- `SQLitePCLRaw.bundle_e_sqlcipher`, compilación comunitaria no oficial y sin soporte oficial del fabricante.
- Bibliotecas nativas para la arquitectura de ejecución.

Fuente: [versiones SQLite personalizadas](https://learn.microsoft.com/en-us/dotnet/standard/data/sqlite/custom-versions).

Selecciona versiones que restauren en net48; no instales todas «latest» sin comprobar sus frameworks y dependencias. `Microsoft.Data.Sqlite.Core` tiene versiones con ensamblados `netstandard2.0`, utilizables desde .NET Framework 4.8; eso no implica que EF Core de la misma numeración soporte net48.

Referencia de compatibilidad: [Microsoft.Data.Sqlite.Core 8.0.28](https://www.nuget.org/packages/Microsoft.Data.Sqlite.Core/8.0.28). Se cita como evidencia del framework objetivo, no como obligación de fijar esa versión para siempre.

Ejemplo de apertura ADO.NET una vez seleccionada e inicializada la combinación correcta:

```vb
Imports Microsoft.Data.Sqlite

' claveRecuperada proviene de un almacén seguro, no de una constante.
Dim builder As New SqliteConnectionStringBuilder() With {
    .DataSource = rutaBase,
    .Password = claveRecuperada
}

Using cn As New SqliteConnection(builder.ConnectionString)
    cn.Open()
    Using cmd = cn.CreateCommand()
        cmd.CommandText = "PRAGMA cipher_version;"
        Dim version = Convert.ToString(cmd.ExecuteScalar())
        If String.IsNullOrWhiteSpace(version) Then
            Throw New InvalidOperationException("No se cargó SQLCipher.")
        End If
    End Using
End Using
```

Esta comprobación detecta un motor sin SQLCipher, pero no sustituye probar una base creada, copiarla y verificar que sin la clave correcta no se puede leer. Una base existente sin cifrar requiere conversión/exportación siguiendo las instrucciones del motor; poner `Password` no convierte automáticamente un archivo anterior.

**Compatibilidad EF6:** Microsoft.Data.Sqlite no es System.Data.SQLite. Esta ruta se propone para ADO.NET; no se promete que tu DbContext EF6 existente pueda utilizarla directamente. Puedes mantener una interfaz de repositorio y una implementación ADO.NET para esta persistencia.

**Migraciones:** utiliza scripts SQL versionados o un migrador compatible con ese proveedor. Disponer de cifrado no añade soporte de migraciones EF6.

### Ruta B: SQLite3 Multiple Ciphers

Otra extensión gratuita, con licencia MIT y bindings .NET. Ofrece cifrados modernos y protección de archivos de datos/journal. Su documentación de integración muestra Microsoft.Data.Sqlite, Dapper y EF Core; no confirma una sustitución directa para EF6.

El proyecto conserva cifrados antiguos por compatibilidad, incluido RC4, que no deben elegirse para una implementación nueva. Debes comprobar también el tratamiento de temporales en la versión y compilación utilizadas.

Fuentes: [SQLite3MC](https://utelle.github.io/SQLite3MultipleCiphers/), [bindings .NET](https://github.com/utelle/SQLite3MultipleCiphers-NuGet).

No combines varios bundles nativos SQLite en el mismo proceso sin un diseño explícito: podrías ejecutar contra una biblioteca distinta de la que aporta cifrado. La configuración de llave cambia entre extensiones y versiones.

## 5. PostgreSQL: cifrado de columnas gratuito

PostgreSQL ofrece `pgcrypto`, sin compra de plugin. Puede cifrar valores y devolver `bytea`, adecuados para almacenar como bytes. Con Npgsql ADO.NET puedes invocar estas funciones mediante parámetros; con EF6 utiliza SQL explícito o una integración de funciones definida para tu modelo.

El cifrado de `pgcrypto` ocurre en el servidor: el valor original y la clave pasan por él. Evita incluir llaves en SQL literal, código de migraciones o logs; utiliza transporte seguro cuando el servidor sea remoto. Para que el servidor nunca reciba el dato original, cifra en la aplicación.

Fuentes: [pgcrypto](https://www.postgresql.org/docs/current/pgcrypto.html), [opciones de cifrado PostgreSQL](https://www.postgresql.org/docs/current/encryption-options.html).

No se propone PostgreSQL estándar como equivalente de un archivo SQLite cifrado o TDE integrado. Las opciones de cifrado de disco/sistema de archivos tienen otro alcance: con el volumen desbloqueado, una copia realizada por un proceso autorizado puede salir legible.

El proveedor `EntityFramework6.Npgsql` existe, pero el proyecto identifica su línea EF6 como legacy y no mantenida activamente. No mezcles esa línea con cualquier versión moderna del driver Npgsql. Para ADO.NET, selecciona una versión del driver compatible con net48.

Fuentes: [Npgsql EF6](https://www.npgsql.org/ef6/), [estado de los proveedores](https://www.npgsql.org/).

## 6. MariaDB Community: cifrado en reposo sin licencia adicional

MariaDB Community puede cifrar datos en reposo usando el plugin `file_key_management`. La biblioteca del plugin se distribuye con el servidor, aunque debe activarse y configurarse. Existen bibliotecas para las plataformas correspondientes, incluido Windows; confirma su presencia en tu distribución exacta.

Elige qué almacenamiento y archivos se cifran: configurar una llave no cifra automáticamente todos los componentes. Revisa tablespaces, temporales, redo logs, binary logs y backups según tu versión. El archivo de llaves puede protegerse adicionalmente; no debe acompañar sin protección a los datos extraídos.

Fuente: [File Key Management](https://mariadb.com/docs/server/security/encryption/data-at-rest-encryption/key-management-and-encryption-plugins/file-key-management-encryption-plugin).

Para ADO.NET, `MySqlConnector` es una opción gratuita MIT que soporta MariaDB y tiene versiones compatibles con .NET Framework 4.8. **MySqlConnector no es por sí mismo un proveedor EF6**. La ruta presupuestada en cero aquí es ADO.NET, sin introducir un proveedor comercial EF6.

Fuentes: [MySqlConnector](https://mysqlconnector.net/), [licencia y diferencias](https://mysqlconnector.net/tutorials/migrating-from-connector-net/), [frameworks compatibles](https://mysqlconnector.net/overview/version-history/).

Es un servicio de base de datos, no un archivo que la aplicación abra directamente como SQLite. No exige mantener el esquema `AttachDbFilename` de SQL Server.

## 7. Oracle AI Database Free: TDE disponible, con salvedades importantes

La tabla oficial de licenciamiento de **release 26** marca TDE para columnas y tablespaces como disponible en Free. No exige comprar Oracle Advanced Security para esa edición; otras ediciones tienen condiciones distintas. En Free no está soportado el método online para cifrar/recifrar/descifrar un tablespace existente.

TDE trabaja debajo de las consultas: puede utilizarse con ADO.NET y con un proveedor ODP.NET EF6 compatible sin añadir cifrado manual al modelo. Configura el keystore y su respaldo separado; verifica la combinación concreta de servidor y proveedor antes de desplegar.

Fuente: [tabla Security de licenciamiento Oracle](https://docs.oracle.com/en/database/oracle/oracle-database/26/dblic/Licensing-Information.html).

La edición Free permite uso en producción, pero Oracle declara que no recibe parches, incluidos los de seguridad, ni soporte comercial mediante Service Requests. Sus límites publicados son 2 CPU para procesos foreground, 2 GB de RAM de base y 12 GB de datos de usuario. **Por esa política de parches no sería mi primera elección para una implementación nueva cuyo objetivo principal sea seguridad**, aunque el coste de licencia sea cero.

Fuente: [Oracle Free FAQ](https://www.oracle.com/database/free/faq/).

No extrapoles esta disponibilidad de TDE a cualquier Oracle XE antiguo. Tampoco equivale a poder abrir un MDF: Oracle administra sus propios archivos y esquemas.

## 8. Protección de llaves sin comprar un servicio

En Windows, DPAPI está disponible sin comprar un plugin. Puedes proteger una clave aleatoria por instalación usando `ProtectedData` y definir el alcance de usuario o máquina.

- `CurrentUser`: asociada al contexto del usuario Windows. Encaja si siempre ejecuta la misma cuenta.
- `LocalMachine`: permite descifrar a otros procesos/cuentas de ese PC si obtienen el contenido protegido. Requiere controles de acceso al archivo; no aísla usuarios por sí sola.
- Recuperación: conserva una copia de recuperación de la clave bajo custodia separada. Un archivo protegido solo con el perfil Windows puede volverse irrecuperable al perder ese contexto.

Fuente: [DataProtectionScope](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.dataprotectionscope?view=netframework-4.8.1).

DPAPI no sustituye el cifrado del motor ni impide que un atacante que controla la sesión de ejecución extraiga información durante el uso. Su función aquí es evitar guardar la llave legible junto a la base.

Para Always Encrypted, puede utilizarse el almacén de certificados Windows en lugar de un servicio externo de pago. Para motores con keystore propio, protege ese material y comprueba si un archivo de autoapertura copiado permitiría abrir también la base en otro equipo.

## 9. Opciones que no resuelven por sí solas la copia de archivos

| Opción | Motivo |
|---|---|
| Contraseña SQL / autenticación Windows | Controla acceso al motor; no cifra por sí sola los datos del MDF |
| TLS / `Encrypt=True` | Protege la conexión, no convierte los archivos en cifrados |
| App.config cifrado | Protege configuración; no cifra las tablas |
| BitLocker o un volumen cifrado | Protege el almacenamiento bloqueado; no garantiza que una copia desde la sesión desbloqueada salga cifrada |
| Base cifrada + llave fija dentro del EXE | Quien extrae el programa puede intentar recuperar también la llave |
| Desencriptar toda la base a un archivo temporal | Reintroduce el archivo legible que querías evitar |
| Hash de un dato que necesitas recuperar | El hash no permite descifrar el valor original |

Estas distinciones describen el alcance de la protección, no un presupuesto adicional.

## 10. Qué elegir según tu requisito

| Requisito principal | Primera candidata sin compra de software |
|---|---|
| Mantener SQL Express y EF6; proteger algunos campos | Always Encrypted, si las consultas encajan |
| Mantener motor y permitir cambiar acceso a campos | Cifrado autenticado en la aplicación y almacenamiento binario |
| Archivo local completamente cifrado; aceptar ADO.NET | SQLite + SQLCipher Community |
| Archivo SQLite con otra extensión gratuita | SQLite3 Multiple Ciphers, validando la integración net48 |
| Servidor con cifrado en reposo gratuito; aceptar ADO.NET | MariaDB Community + File Key Management |
| PostgreSQL ya instalado; proteger campos | pgcrypto o cifrado desde la aplicación |
| Oracle Free ya forma parte del entorno | Evaluar TDE de la edición/version exacta, considerando su política de parches |

**Recomendación para tu caso:** primero prueba Always Encrypted si quieres conservar SQL Express y EF6. Si el requisito real es cifrar toda una base local y aceptas ADO.NET, prueba SQLite + SQLCipher Community. Ambas rutas pueden tener coste de licencia cero. Bajo este criterio, no hay motivo para descartar una solución únicamente porque la integración gratuita no venga preparada para EF6.

## 11. Prueba de aceptación

1. Crea datos de prueba y comprueba lectura/escritura desde la aplicación.
2. Cierra el proceso correctamente y copia los archivos de datos a otro PC.
3. Sin claves, intenta leer el contenido protegido: debe fallar o devolver únicamente ciphertext.
4. Copia también EXE y configuración; comprueba que no incluyen la llave legible ni acceso automático no previsto.
5. Ejecuta la recuperación autorizada con las claves custodiadas: debe funcionar.
6. Verifica actualizaciones de esquema, backups, temporales y logs.
7. Comprueba consultas e índices de negocio, no solo un INSERT/SELECT de demostración.

Esta guía compara opciones documentadas y rutas de compatibilidad. No contiene un proyecto de cifrado certificado ni afirma haber ejecutado cada combinación de proveedor/motor. Antes de fijar paquetes, selecciona una combinación mantenida compatible con net48 y registra sus versiones y licencias.
