# Proteger web.config con DPAPI en IIS local

Guía para publicar `Cirsa.TableGames.Identity.Api` en IIS de Windows y cifrar `appSettings` y `connectionStrings` con las herramientas de .NET Framework 4.8.

**Sí, puedes reproducirlo en tu equipo.** La API seguirá leyendo mediante `ConfigurationManager`; ASP.NET descifra las secciones automáticamente. No hay que implementar cifrado en `Cirsa.WebApi2.Extensions` ni modificar sus opciones de autenticación. [1]

Este procedimiento utiliza **IIS completo**, no IIS Express. Asume una sola máquina y protección DPAPI de ámbito de máquina mediante el proveedor integrado `DataProtectionConfigurationProvider`. No es un procedimiento para copiar una configuración ya cifrada entre servidores.

## 1. Qué vamos a proteger

| Elemento | Tratamiento |
| --- | --- |
| API keys | Secretos; dentro de appSettings cifrado |
| Clave de firma JWT | Secreto; dentro de appSettings cifrado |
| Issuer, audience y duración | No son secretos; se cifran también por compartir la sección |
| Cadena de conexión | Se cifra la sección completa |
| Código fuente y plantillas | Sin secretos reales ni claves de descifrado propias |

La cadena actual usa autenticación integrada y no contiene contraseña. Cifrarla oculta su contenido almacenado, pero **no concede permisos de acceso a SQL Server**.

DPAPI protege la copia almacenada. La aplicación necesita acceder a los valores en memoria para trabajar. La protección de máquina no separa por sí sola a todos los usuarios o procesos del mismo Windows: otro proceso con acceso al archivo puede tener capacidad de descifrarlo. Por eso importan los permisos NTFS. No sustituye HTTPS ni protege frente a un administrador o una aplicación comprometida. [2]

## 2. Requisitos

- Windows con IIS disponible y .NET Framework 4.8/4.8.1 instalado.
- Permisos de administrador para preparar IIS y modificar la publicación.
- SQL Server accesible y la base `Cirsa.TableGames.Identity` ya creada mediante las migraciones.
- API compilable y un conjunto de claves **exclusivamente de desarrollo**.
- Windows PowerShell **5.1 de 64 bits**, abierto como administrador, para los comandos de esta guía.

En **Activar o desactivar las características de Windows**, habilita:

- Internet Information Services → Herramientas de administración web → Consola de administración de IIS.
- Servicios World Wide Web → Características de desarrollo de aplicaciones → ASP.NET 4.x y Extensibilidad de .NET 4.x.
- Extensiones ISAPI y Filtros ISAPI.
- Características HTTP comunes → Contenido estático, para los recursos de Swagger.

La etiqueta puede aparecer como 4.8, 4.8.1 u otra versión 4.x según Windows. Al seleccionar ASP.NET, acepta sus dependencias. No necesitas instalar el Hosting Bundle de ASP.NET Core para esta API de .NET Framework. [3]

## 3. Publicar en una carpeta separada

En Visual Studio:

1. Clic derecho en **el proyecto API** → Publicar.
2. Selecciona publicación a **Carpeta**.
3. Usa esta ruta de ejemplo:

   ```text
   C:\Sites\Cirsa.TableGames.Identity.Api
   ```

4. Publica, preferiblemente en configuración Release.
5. Comprueba que en esa carpeta están `web.config`, `bin` y los demás archivos de publicación.

**Trabaja sobre el web.config publicado, no sobre el del repositorio.** El código fuente conserva una plantilla sin secretos reales. Si usas un archivo externo con `configSource` o `appSettings file`, este ejemplo debe adaptarse: para esta primera prueba deja las dos secciones directamente en el web.config publicado.

## 4. Crear el sitio y su Application Pool

Abre `inetmgr`.

### Application Pool

Crea uno con estos valores:

| Opción | Valor |
| --- | --- |
| Nombre | `CirsaIdentityLocal` |
| Versión de .NET CLR | `v4.0` |
| Modo de canalización | `Integrated` |
| Identity | `ApplicationPoolIdentity` |
| Enable 32-Bit Applications | `False` |

Ver `v4.0` en IIS es correcto para .NET Framework 4.8: no busques una opción CLR v4.8.

### Sitio web

Crea un sitio con:

| Opción | Valor de ejemplo |
| --- | --- |
| Nombre | `CirsaIdentityLocal` |
| Application Pool | `CirsaIdentityLocal` |
| Ruta física | `C:\Sites\Cirsa.TableGames.Identity.Api` |
| Tipo | `http` |
| Dirección IP | `127.0.0.1` |
| Puerto | `8088` |
| Nombre de host | Vacío |

Usaremos `http://127.0.0.1:8088` **solo para esta prueba local de almacenamiento de configuración**. No expongas este binding a la red ni lo uses como configuración de producción. Si la API ya exige HTTPS o tiene una redirección, conserva ese requisito y prepara un binding HTTPS con certificado de desarrollo confiable.

Conserva la autenticación anónima de IIS habilitada para que las peticiones lleguen al filtro ApiKey/JWT de Web API. Esto no vuelve públicos los endpoints protegidos por nuestros atributos.

## 5. Permisos de la carpeta

En PowerShell como administrador, define las variables. Se reutilizan durante toda la guía:

```powershell
$sitePath = 'C:\Sites\Cirsa.TableGames.Identity.Api'
$siteName = 'CirsaIdentityLocal'
$poolName = 'CirsaIdentityLocal'
$webConfigPath = Join-Path $sitePath 'web.config'

if (-not (Test-Path -LiteralPath $webConfigPath)) {
    throw 'No se encontró el web.config publicado. Revisa $sitePath.'
}
```

Concede lectura y ejecución al pool:

```powershell
& icacls.exe $sitePath /grant "IIS AppPool\${poolName}:(OI)(CI)(RX)"
if ($LASTEXITCODE -ne 0) { throw 'No se pudieron asignar los permisos de lectura.' }
```

Revisa la pestaña **Seguridad** de la carpeta: conserva acceso para administradores, SYSTEM y la identidad de despliegue; evita permisos amplios innecesarios. El comando anterior **agrega** permisos, no elimina los heredados. [4]

El pool no necesita escritura sobre web.config. Si hay logs en disco, concede escritura únicamente en su carpeta específica. No concedas control total sobre toda la aplicación para solucionar un error de acceso.

## 6. Comprobar SQL Server antes de cifrar

Con `Integrated Security=True`, la identidad usada en IIS puede ser diferente a tu usuario de Visual Studio.

Para el escenario de esta guía —IIS y SQL Server en la misma máquina, sin impersonación— el principal que debes revisar es:

```text
IIS APPPOOL\CirsaIdentityLocal
```

Verifica que tenga login en SQL Server, usuario en `Cirsa.TableGames.Identity` y los permisos necesarios para las consultas de la API. El inicio de sesión que consulta las tablas puede funcionar con permisos de lectura; las operaciones de escritura requieren los permisos adicionales correspondientes. **La identidad de ejecución no necesita permisos de migración ni db_owner por defecto.**

Si SQL Server está en otro equipo, no asumas que reconocerá ese nombre de pool. Con ApplicationPoolIdentity, el acceso a recursos de red puede presentarse con la cuenta del equipo; una identidad de servicio de dominio administrada por infraestructura suele requerir otra configuración. [4]

Primero prueba desde Postman el login que ya tienes y `/swagger` en IIS. Deben funcionar antes del cifrado. Un `Login failed for user...` es un problema de autenticación/permisos de SQL Server, no una señal de que DPAPI falle.

## 7. Preparar las claves locales

En el web.config **publicado**, configura valores de desarrollo para:

- `Security.ApiKeys`, conservando el formato `baccarat:clave;crazzy_roulet:clave`.
- `Security.JwtSigningKey`, Base64 de al menos 32 bytes aleatorios.
- Issuer, audience y duración, de acuerdo con tu configuración actual.

No reutilices las claves que compartiste en el chat como credenciales de producción.

Si necesitas generar valores nuevos, este fragmento modifica únicamente esas dos entradas del web.config publicado y no imprime las claves. Ejecútalo **antes de cifrar**, con el sitio detenido y después de definir las variables del paso 5:

```powershell
Import-Module WebAdministration
Stop-Website -Name $siteName

function New-LocalSecret {
    $bytes = New-Object byte[] 32
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $rng.GetBytes($bytes)
        return [Convert]::ToBase64String($bytes)
    }
    finally {
        $rng.Dispose()
    }
}

[xml]$configuration = Get-Content -LiteralPath $webConfigPath -Raw

if ($configuration.configuration.appSettings.configProtectionProvider) {
    throw 'appSettings ya está cifrado. No lo modifiques con este fragmento.'
}

$apiKeysNode = $configuration.SelectSingleNode(
    '/configuration/appSettings/add[@key="Security.ApiKeys"]')
$jwtKeyNode = $configuration.SelectSingleNode(
    '/configuration/appSettings/add[@key="Security.JwtSigningKey"]')

if ($null -eq $apiKeysNode -or $null -eq $jwtKeyNode) {
    throw 'No se encontraron las entradas esperadas en appSettings.'
}

$apiKeysNode.SetAttribute('value',
    ('baccarat:{0};crazzy_roulet:{1}' -f (New-LocalSecret), (New-LocalSecret)))
$jwtKeyNode.SetAttribute('value', (New-LocalSecret))
$configuration.Save($webConfigPath)

Start-Website -Name $siteName
```

Actualiza las credenciales de tu cliente de prueba y vuelve a comprobar el login antes de continuar. Una clave JWT nueva invalida los tokens firmados con la anterior en nuestra configuración actual, que acepta una única clave.

## 8. Cifrar las secciones en la publicación

Ejecuta en la misma sesión de PowerShell elevada:

```powershell
$aspnetRegiis = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\aspnet_regiis.exe'

if (-not (Test-Path -LiteralPath $aspnetRegiis)) {
    throw 'No se encontró aspnet_regiis.exe. Revisa la instalación de .NET Framework.'
}
```

Esa ruta `v4.0.30319` es normal para la familia .NET Framework 4.x. Este ejemplo asume proceso de 64 bits; para un despliegue de 32 bits utiliza la herramienta de `Microsoft.NET\Framework\v4.0.30319` y la configuración de pool correspondiente.

Detén el sitio y el pool para no procesar peticiones durante el cambio:

```powershell
Import-Module WebAdministration
Stop-Website -Name $siteName
Stop-WebAppPool -Name $poolName
```

Conserva una copia recuperable del web.config anterior **fuera de la carpeta pública y del repositorio**, en una ubicación con acceso restringido a los responsables del despliegue. Esa copia sigue conteniendo secretos en claro: protégela y elimínala al concluir la prueba si ya no la necesitas. No crees `web.config.bak` dentro del sitio.

Ahora cifra, usando una línea por comando:

```powershell
& $aspnetRegiis -pef 'appSettings' $sitePath -prov 'DataProtectionConfigurationProvider'
if ($LASTEXITCODE -ne 0) { throw 'Falló el cifrado de appSettings. El sitio queda detenido para revisión.' }

& $aspnetRegiis -pef 'connectionStrings' $sitePath -prov 'DataProtectionConfigurationProvider'
if ($LASTEXITCODE -ne 0) { throw 'Falló el cifrado de connectionStrings. El sitio queda detenido para revisión.' }
```

`-pef` recibe **la carpeta física que contiene web.config**, no la ruta al archivo, el nombre del sitio ni la URL. `-prov` selecciona DPAPI explícitamente; no omitas ese argumento porque el proveedor predeterminado puede ser RSA. [5]

Este procedimiento no utiliza `aspnet_regiis -i`, no registra ASP.NET de nuevo y no ejecuta un `iisreset` que afecte a otros sitios.

## 9. Comprobar el resultado en disco

Abre el web.config publicado con un editor local. Las dos secciones deben haber cambiado a una estructura similar a esta:

```xml
<appSettings configProtectionProvider="DataProtectionConfigurationProvider">
  <EncryptedData>
    <CipherData>
      <CipherValue>CONTENIDO_CIFRADO</CipherValue>
    </CipherData>
  </EncryptedData>
</appSettings>
```

Es un ejemplo ilustrativo, **no un bloque para copiar**. La herramienta genera el XML real, con sus atributos y namespaces. Las entradas `<add key="Security.ApiKeys" ...>` ya no deben verse en claro dentro de esa sección.

También puedes comprobar solo los marcadores, sin imprimir el contenido:

```powershell
[xml]$protectedConfig = Get-Content -LiteralPath $webConfigPath -Raw

foreach ($sectionName in @('appSettings', 'connectionStrings')) {
    $section = $protectedConfig.SelectSingleNode('/configuration/' + $sectionName)
    if ($null -eq $section) { throw "No existe la sección $sectionName." }
    if ($section.GetAttribute('configProtectionProvider') -ne 'DataProtectionConfigurationProvider') {
        throw "La sección $sectionName no está protegida con el proveedor esperado."
    }
    Write-Host "$sectionName : marcador DPAPI presente."
}
```

Esta verificación solo comprueba el archivo; la prueba de ejecución del siguiente paso confirma que la API puede descifrar.

## 10. Iniciar y probar la API

```powershell
Start-WebAppPool -Name $poolName
Start-Website -Name $siteName
```

Prueba con tu configuración actual de Postman y Swagger:

| Prueba | Resultado esperado |
| --- | --- |
| Swagger, si lo mantienes público | Carga normalmente |
| Endpoint ApiKey con clave válida | Supera la autenticación |
| Endpoint ApiKey sin clave o con clave incorrecta | 401 |
| Login con credenciales válidas y API key requerida | Respuesta de login y JWT, según tu controlador |
| Endpoint Bearer con el JWT emitido | Supera la autenticación |
| Endpoint que consulta datos | Accede a SQL con la identidad de IIS |

Superar la autenticación no garantiza 200 si la operación falla por otra razón. Conserva la ruta y el cuerpo reales de tu endpoint de login; esta guía no los cambia.

El código de lectura sigue siendo el mismo:

```vb
Dim configuredApiKeys = ConfigurationManager.AppSettings("Security.ApiKeys")
Dim signingKey = ConfigurationManager.AppSettings("Security.JwtSigningKey")
Dim connectionString = ConfigurationManager.ConnectionStrings("IdentityDb").ConnectionString
```

No necesitas llamar a `UnprotectSection` en la API. **UnprotectSection y `-pdf` reescriben el archivo en claro**; no son necesarios para leerlo. Tampoco agregues endpoints que devuelvan secretos para comprobar que se descifraron. [1][6]

## 11. Cambiar una clave o editar la configuración

Para esta prueba local, realiza el cambio como mantenimiento, con el sitio detenido:

```powershell
Stop-Website -Name $siteName
Stop-WebAppPool -Name $poolName

& $aspnetRegiis -pdf 'appSettings' $sitePath
if ($LASTEXITCODE -ne 0) { throw 'No se pudo descifrar appSettings.' }
```

Edita los valores en el web.config publicado. Después vuelve a cifrar:

```powershell
& $aspnetRegiis -pef 'appSettings' $sitePath -prov 'DataProtectionConfigurationProvider'
if ($LASTEXITCODE -ne 0) { throw 'No se pudo volver a cifrar appSettings.' }

Start-WebAppPool -Name $poolName
Start-Website -Name $siteName
```

Si necesitas editar la cadena de conexión, aplica `-pdf 'connectionStrings'` y luego `-pef 'connectionStrings'` con el mismo proveedor. El resto de las secciones puede permanecer cifrado. [5][6]

El archivo queda temporalmente en claro durante la edición. Hazlo únicamente en la máquina destino y conserva sus permisos. Reiniciar el pool también reconstruye los singleton de autenticación con los nuevos valores; nuestra implementación no recarga las claves en caliente.

Si un comando falla, no continúes como si todo hubiera terminado: revisa qué sección quedó en claro antes de volver a levantar el sitio.

## 12. Publicaciones posteriores y otros ambientes

Cada nueva publicación puede reemplazar web.config por una versión en claro y perder valores específicos del servidor. Para tu flujo futuro:

1. Detener únicamente la aplicación afectada durante el despliegue.
2. Publicar los archivos de la API.
3. Inyectar los valores del ambiente mediante el mecanismo de despliegue autorizado.
4. Cifrar las secciones **en ese servidor**.
5. Comprobar que ambas secciones están protegidas.
6. Iniciar la aplicación y ejecutar una prueba funcional.

No copies el web.config cifrado de tu portátil a PROD. Con protección de máquina, el destino debe generar su propia copia protegida. Una reinstalación del sistema puede dejar inutilizable un archivo cifrado antiguo: conserva los secretos en una fuente de recuperación administrada; el archivo cifrado no es tu única copia de respaldo. [2]

En otra máquina repetirás el procedimiento con sus propios secretos. Si más adelante necesitas compartir una configuración cifrada entre servidores, evalúa RSA con administración de claves o una bóveda. El cambio del origen de secretos puede quedar en el proyecto API/despliegue; la biblioteca sigue recibiendo opciones.

## 13. Problemas frecuentes

| Síntoma | Qué revisar |
| --- | --- |
| Access denied al ejecutar aspnet_regiis | PowerShell elevado y permisos de escritura del operador sobre la publicación |
| No encuentra web.config | `-pef` debe apuntar a la carpeta publicada, no al archivo |
| Provider not found | Nombre exacto `DataProtectionConfigurationProvider` y configuración de .NET Framework |
| Key not valid for use in specified state / Failed to decrypt | Archivo copiado desde otra máquina, protección de usuario distinta o cambios en el sistema/proveedor |
| HTTP 500.19 | Detalle del error de configuración: XML, permisos, secciones inválidas o módulos faltantes; no asumir automáticamente DPAPI |
| HTTP 403.14 al abrir la raíz | No existe documento predeterminado; probar `/swagger` o una ruta real de API |
| SQL Login failed | Identidad del pool, acceso a la base y permisos; cifrar no los modifica |
| Se pierde el cifrado después de publicar | La publicación sobrescribió web.config; aplicar el paso de protección al final del despliegue |
| 401 tras cambiar una API key/JWT key | Cliente con clave antigua o token anterior; actualizar credencial y emitir token nuevo |

Para errores de carga usa el Visor de eventos y los detalles locales de IIS. No expongas errores detallados ni valores de configuración a clientes remotos.

## 14. Resultado y límites de esta guía

La prueba está completa cuando el web.config publicado tiene ambas secciones cifradas y las mismas operaciones de API siguen funcionando sin cambiar su código de lectura.

Los comandos se contrastaron con documentación de Microsoft. **No se ejecutaron en tu IIS ni se probó DPAPI en Windows desde esta sesión.** Las rutas, nombre de sitio y puerto son ejemplos que debes adaptar. Primero completa esta prueba local; después aplica el procedimiento al ambiente real con su identidad y despliegue definidos.

## Referencias oficiales

1. [ProtectedConfigurationProvider: descifrado transparente en ASP.NET](https://learn.microsoft.com/en-us/dotnet/api/system.configuration.protectedconfigurationprovider?view=netframework-4.8).
2. [DpapiProtectedConfigurationProvider y protección de máquina/usuario](https://learn.microsoft.com/en-us/dotnet/api/system.configuration.dpapiprotectedconfigurationprovider?view=netframework-4.8.1).
3. [Instalación de IIS y los módulos ASP.NET](https://learn.microsoft.com/en-us/iis/application-frameworks/scenario-build-an-aspnet-website-on-iis/configuring-step-1-install-iis-and-asp-net-modules).
4. [Identidades de los Application Pools y permisos de recursos](https://learn.microsoft.com/en-us/iis/manage/configuring-security/application-pool-identities).
5. [Protección de cadenas de conexión y configuración: comandos por directorio físico](https://learn.microsoft.com/en-us/aspnet/web-forms/overview/data-access/advanced-data-access-scenarios/protecting-connection-strings-and-other-configuration-information-vb).
6. [Cifrado y descifrado de secciones de configuración](https://learn.microsoft.com/en-us/previous-versions/aspnet/zhhddkxy(v=vs.100)).
