# SensitiveData48 — protección de secretos en VB.NET / .NET Framework 4.8

Esta solución es un laboratorio ejecutable y una biblioteca reutilizable. Usa consolas para probar cada alternativa sin mezclar la gestión de secretos con Swagger, IIS o autenticación HTTP. Los proyectos de biblioteca se pueden referenciar desde una API Web API 2, WinForms o un servicio Windows. No modifica el proyecto de API anterior.

## Decisión recomendada

| Tipo de dato | Tratamiento recomendado | Ejemplo implementado |
| --- | --- | --- |
| Cadena SQL con autenticación integrada | Configuración normal, sin contraseña; identidad dedicada y permisos mínimos en SQL | DatabaseConnectionFactory.CreateIntegrated |
| Cadena SQL con contraseña | Key Vault; mantener solo el nombre del secreto en configuración | CreateFromSecret; lectura opcional en Demo.Azure |
| Llave HMAC de integridad | Secreto aleatorio propio, independiente de JWT | hmac-v1 + HmacService |
| Llave de firma JWT HS256 | Key Vault, llave independiente y nombres inmutables por generación | jwt-signing-v1 + JwtTokenService y kid |
| API key de clientes que tu aplicación valida | Conservar su huella SHA-256, no el valor original, si no necesitas recuperarlo | ApiKeyFingerprint / VerifyApiKey |
| API key de un tercero que debes enviar | Es necesario recuperar el original: Key Vault | ISecretStore.GetSecret con el nombre que asignes |
| Contraseña de usuario | Hash de contraseña con salt mediante una biblioteca/proveedor mantenido; nunca SHA-256 directo ni HMAC como sustituto | Se explica la distinción; no se implementa un sistema de usuarios |
| URL, timeout, nombres de secretos, issuer, audience, IDs de Azure | App.config / Web.config; App Configuration solo si se necesita centralización | App.config de ambas demos |
| Secretos locales sin acceso a Azure | DPAPI CurrentUser fuera del proyecto y ACL restringidas | DpapiSecretStore y Demo.Local |

Key Vault es mi elección para producción si la infraestructura y la identidad permiten usarlo. DPAPI es una alternativa local explícita, no un fallback oculto ante errores de Azure. App Configuration y DevOps Library no son dependencias del proyecto: no aportan por sí solos una mejora a este ejemplo. DevOps puede desplegar identificadores, URIs y opciones públicas sin transportar secretos.

## Abrir y empezar en Windows

Requisitos: Visual Studio 2022 o Build Tools con compilador Visual Basic y targeting pack de .NET Framework 4.8. Windows PowerShell 5.1 para los scripts. Acceso a nuget.org para restaurar paquetes.

1. Descomprime y abre SensitiveData48.sln.
2. Establece Demo.Local como proyecto de inicio. No requiere Azure ni base de datos para ejecutarse.
3. Desde la carpeta de la solución:

```powershell
.\Build.ps1 -Test
.\src\Demo.Local\bin\Debug\Demo.Local.exe init
.\src\Demo.Local\bin\Debug\Demo.Local.exe demo
```

`init` pide una API key aleatoria propia de al menos 32 caracteres mediante entrada oculta. Debe ser una clave aleatoria, no una contraseña humana larga. Genera internamente dos llaves distintas de 32 bytes: JWT y HMAC. Solo almacena la huella de la API key; no conserva su original. No se imprimen llaves ni tokens. No hay credenciales predeterminadas.

Los secretos DPAPI quedan en `%LOCALAPPDATA%\SensitiveData48\Secrets`, bajo el usuario que ejecuta la consola. Los archivos contienen texto XML y blobs cifrados, no valores en claro. `init` se usa una vez: rechaza sobrescribir llaves existentes. No borres esa carpeta para "arreglar" un error si ya protege datos o tokens en uso.

Para guardar un secreto adicional, sin pasarlo por argumentos o historial:

```powershell
.\src\Demo.Local\bin\Debug\Demo.Local.exe set-secret db-connection-string
# Sobrescritura explícita de un secreto conocido:
.\src\Demo.Local\bin\Debug\Demo.Local.exe set-secret nombre replace
```

La entrada es oculta. Para cambiar llaves JWT usa nombres por generación, no reemplaces jwt-signing-v1 mientras aún haya tokens que lo necesiten.

Para depurar init/demo desde Visual Studio, configura el argumento de línea de comandos del proyecto en Propiedades > Depurar. Sin argumentos muestra la ayuda. Las excepciones se ocultan de la salida por defecto; pon un breakpoint en el Catch principal e inspecciona el error sin compartir secretos.

## Proyectos y flujo de lectura

| Proyecto / clase | Qué hace |
| --- | --- |
| SensitiveData.Core / ISecretStore | Contrato para recuperar secretos sin acoplar la lógica de negocio al origen |
| DpapiSecretStore | Protege con Windows DPAPI CurrentUser y escribe de forma atómica sin un archivo temporal en claro |
| CachedSecretStore | Cache de 60 segundos en las demos, solo en memoria; no sirve datos vencidos si falla el origen |
| JwtTokenService | Emite y valida HS256, con emisor, audiencia, firma, algoritmo, duración y kid controlados |
| HmacService | Detecta alteraciones en un mensaje usando una llave independiente |
| CryptoUtilities | Generación aleatoria, comparación de digests y huellas de API keys |
| DatabaseConnectionFactory | Construye conexiones sin imprimirlas; exige TLS y validar certificado |
| SensitiveData.Azure / KeyVaultSecretStore | Lee secretos del vault; no permite escribirlos desde el contrato de ejecución |
| CredentialFactory | Selecciona Azure CLI de desarrollo, identidad administrada o certificado de una aplicación Entra |
| Demo.Local | Aprovisionamiento y pruebas locales; no se conecta a Azure |
| Demo.Azure | Consulta Key Vault al ejecutarse; no usa archivos secretos locales ni fallback |

Lectura recomendada: SecretStores.vb, DpapiSecretStore.vb, Program.vb de Demo.Local, JwtTokenService.vb, KeyVaultSecretStore.vb y CredentialFactory.vb.

## Qué queda en disco y qué no

En las demos, App.config contiene opciones públicas. El archivo que lee la consola compilada es Demo.Local.exe.config o Demo.Azure.exe.config, junto al EXE. No se guardan claves allí.

DPAPI sí guarda los valores cifrados localmente. Key Vault conserva los secretos de Azure y la aplicación los recupera al ejecutarse. En ambos casos, el valor descifrado termina en memoria porque se necesita para usarlo. SecretValue.ToString devuelve una redacción para evitar impresiones accidentales, pero esto NO impide serializar la propiedad Value por reflexión: no devuelvas objetos SecretValue desde una API, no los registres en logs y no los incluyas en telemetría.

No se garantiza el borrado de strings del heap administrado. Se limpian algunos buffers byte[] cuando es viable, pero un administrador, un depurador o código que controla el proceso puede recuperar material sensible. No uses estos patrones para prometer ocultación de secretos a quien administra la máquina.

DPAPI CurrentUser requiere el mismo usuario y un contexto/perfil Windows utilizable. No se deben copiar los blobs entre usuarios o servidores esperando que funcionen. La carpeta se crea con ACL para el usuario actual y SYSTEM. No se usa LocalMachine, que permitiría descifrar a otras cuentas del equipo si acceden al blob. Para un proceso IIS/servicio, aprovisionar bajo tu cuenta de desarrollador no sirve: necesita la identidad y el perfil correctos. Para ese escenario prefiero Key Vault antes de forzar DPAPI en el Application Pool.

## Azure: configuración sin una contraseña adicional

Compila la solución completa:

```powershell
.\Build.ps1 -IncludeAzure -Test
```

Configura `src/Demo.Azure/App.config` antes de compilar (o el EXE.config del resultado después):

- KeyVaultUri: URI real del vault.
- AzureAuthentication: AzureCli, ManagedIdentity o ClientCertificate.
- Environment: Development para permitir AzureCli. En producción usa Production y una identidad concreta.

### Desarrollo

1. El operador crea un vault de pruebas o selecciona uno autorizado.
2. Concede a tu identidad permisos mínimos de lectura de los secretos requeridos, por ejemplo el rol Key Vault Secrets User con el alcance adecuado.
3. Inicia sesión con `az login`, eligiendo el tenant/suscripción adecuados.
4. Configura AzureAuthentication=AzureCli y Environment=Development.
5. Crea las llaves con un operador autorizado. El script opcional `Provision-KeyVault.ps1` requiere Az.KeyVault y una sesión Connect-AzAccount; crea jwt-signing-v1 y hmac-v1 con valores Base64 aleatorios independientes. No configura Azure, no concede permisos y no se ejecuta automáticamente. El operador necesita permisos de escritura de secretos; la identidad de ejecución solo lectura.
6. Ejecuta Demo.Azure. No imprime tokens ni secretos, y no abre conexiones SQL.

Los nombres jwt-signing-v1 y hmac-v1 deben contener exactamente 32 bytes aleatorios codificados como Base64. No escribas frases como llave. Si habilitas UseSqlSecret=true, crea db-connection-string con la cadena SQL completa y permisos adecuados. Mantén UseSqlSecret=false si usas autenticación integrada o solo quieres probar criptografía.

### Alojamiento Azure compatible con identidad administrada

Selecciona AzureAuthentication=ManagedIdentity. Activa la identidad en el recurso y dale permisos de lectura en el vault. Deja ManagedIdentityClientId vacío para identidad asignada por el sistema o indica el client ID de una identidad asignada por el usuario. No hay client secret en archivos. La aplicación usa ManagedIdentityCredential explícitamente; no recurre a una sesión de desarrollador si la identidad falla.

### IIS propio sin identidad administrada

Selecciona ClientCertificate. Registra una aplicación en Entra, registra su certificado público y configura TenantId, ClientId y CertificateThumbprint. Instala el certificado con clave privada en LocalMachine/My y concede acceso a esa clave a la identidad que ejecuta el proceso. Prefiere claves no exportables cuando el método de aprovisionamiento lo permita. No incluyas PFX ni su contraseña en el repositorio o paquete de despliegue. El certificado de acceso a Azure es otra credencial: requiere renovación y protección; no elimina por arte de magia toda credencial local.

No se incluye un script de creación de recursos o asignación de roles, para no asumir tenant, suscripción, identidades ni permisos. Confirma conectividad, firewall/private endpoint y políticas de acceso del vault antes de desplegar.

## API keys, contraseñas, hash y HMAC

No todas las "llaves para hash" son lo mismo:

- SHA-256 no tiene llave. La huella de una API key funciona aquí porque la API key debe ser impredecible, generada aleatoriamente y con suficiente entropía. No hagas lo mismo con contraseñas humanas.
- Para una API key entrante, en una aplicación real guarda un ID público de cliente, la huella de la parte secreta, estado, permisos y expiración. Esta demo verifica una sola huella, no implementa un registro de clientes ni HTTP.
- Una API key de otro proveedor debe recuperarse para enviarla: solo su hash no sirve.
- HMAC usa una llave para autenticar un mensaje. El ejemplo verifica integridad, pero no cifra el mensaje ni evita reenvíos por sí solo. Si lo conviertes en protocolo HTTP, necesitas especificación canónica, timestamp/nonce, límites y control de replay.
- Las contraseñas de usuarios deben ir a un proveedor de identidad o a una biblioteca de hashing de contraseñas mantenida. Esta solución no crea un hasher de contraseñas propio ni inventa un algoritmo.

## JWT y rotación

El ejemplo HS256 tiene sentido si emisor y validadores pertenecen al mismo límite de confianza. Cualquiera con acceso a la llave HS256 puede emitir tokens. Si varios sistemas independientes solo deben validar, prefiero un proveedor de identidad o firma asimétrica (RSA/ECDSA) con distribución de claves públicas. Este ejemplo no implementa OIDC, JWKS ni firma remota con Key Vault Keys: usa secretos de Key Vault para HS256.

No incluyas contraseñas, cadenas de conexión ni datos confidenciales en claims: un JWT firmado no es un JWT cifrado.

Para rotar:

1. Crea jwt-signing-v2 con una llave nueva, independiente. No sobrescribas v1.
2. En todos los validadores configura JwtAcceptedKids=v1,v2 y reinicia/recarga según tu aplicación.
3. Cuando todos acepten v2, cambia JwtCurrentKid a v2 en el emisor. La demo combina emisor/validador, por eso tiene ambas opciones.
4. Espera a que expiren los últimos tokens v1 (15 minutos más tolerancia de reloj desde su última emisión; ten en cuenta nodos antiguos).
5. Retira v1 de JwtAcceptedKids en los validadores y luego revoca/retira el secreto cuando corresponda.

kid solo selecciona nombres de una lista pública permitida. No se acepta una URL, ruta o nombre arbitrario enviado por el cliente. La cache introduce hasta 60 segundos de retraso en la lectura de nuevos valores. No se conserva un valor vencido en cache para ocultar una caída de Key Vault. El administrador debe definir el compromiso disponibilidad/seguridad para producción.

HmacService usa hmac-v1 para demostrar separación de propósito; no implementa un anillo de rotación HMAC. Si conservarás MACs a largo plazo, añade un key ID al registro y mantén las llaves previas durante su período de verificación, como una política separada de JWT.

## SQL Server

CreateIntegrated permite eliminar usuario/contraseña SQL de configuración si la identidad y SQL Server lo admiten. En IIS la identidad de ejecución debe tener acceso explícito; para SQL remoto suele requerirse una identidad de dominio administrada adecuadamente. No uses sa ni db_owner como cuenta de ejecución.

Ambos constructores de conexión fuerzan Encrypt=true, TrustServerCertificate=false y Persist Security Info=false. SQL Server necesita un certificado válido y confiable cuyo nombre coincida con el servidor configurado. No se desactiva esa validación para que una demo conecte. Las demos solo construyen la conexión; no prueban conectividad, permisos SQL ni el certificado remoto. Al integrar, abre la conexión en un entorno de pruebas y verifica esos tres puntos.

## Reutilizar en una API o WinForms

Referencia SensitiveData.Core y, solo si necesitas Azure, SensitiveData.Azure. Crea un proveedor/cache por instancia de la aplicación e inyéctalo en los servicios; reutiliza SecretClient. No consultes Key Vault por cada propiedad/registro de una tabla. Evita bloquear un hilo de interfaz WinForms durante operaciones de red: la demo usa llamadas síncronas para que el flujo se lea fácilmente; para UI y API con carga, adapta el contrato a async/await y propagación de CancellationToken.

En una API no registres secretos ni devuelvas excepciones internas al consumidor. En WinForms distribuido a equipos que no controlas, no distribuyas llaves de JWT de servidor ni credenciales compartidas de SQL/Key Vault. Usa una API intermedia con autenticación de usuarios. DPAPI limita lectura del archivo por otras cuentas; no protege un secreto frente al usuario que ejecuta la aplicación.

## App Configuration y DevOps Library: dónde encajarían

App Configuration solo sería necesario para configuración central compartida, etiquetas o feature flags. Podría guardar referencias a secretos, pero Key Vault seguiría siendo el origen del secreto. Para esta solución no justifica una dependencia adicional.

DevOps Library puede proporcionar al pipeline URI del vault, IDs y configuración por ambiente. Si el pipeline recupera un secreto y lo escribe en Web.config, el secreto termina en disco; marcarlo como secreto en DevOps no cifra ese archivo. Esta solución muestra lectura de Key Vault en ejecución, sin inyectar llaves en paquetes. Evita imprimir variables sensibles, publicar archivos de configuración con secretos como artefactos o usar llaves de producción para pruebas.

## Compilación, dependencias y pruebas

Los .vbproj enumeran sus archivos explícitamente, sin comodines. Si agregas un archivo, inclúyelo y guarda el proyecto. Se fija Option Strict On. Los ejecutables habilitan AutoGenerateBindingRedirects y GenerateBindingRedirectsOutputType: distribuye el EXE.config generado junto con las DLL. Un Web.config de IIS no se actualiza por copiar ese EXE.config: al reutilizar bibliotecas en Web API debes generar/revisar los redirects del host web y probarlo. No mezcles DLL antiguas manualmente.

Dependencias fijadas: System.IdentityModel.Tokens.Jwt 8.23.0; Azure.Identity 1.21.0; Azure.Security.KeyVault.Secrets 4.11.2. Las versiones SDK seleccionadas ofrecen assets compatibles con .NET Framework 4.8 a través de .NET Standard 2.0. NuGet.Config usa nuget.org sin credenciales; adapta la fuente si tu empresa exige un feed propio. Revisa las vulnerabilidades y actualizaciones al integrar.

`Build.ps1 -Test` compila Demo.Local y sus dependencias y ejecuta self-test. Incluye comprobaciones de DPAPI, rechazo de archivos alterados, rutas no válidas, protección de API keys, HMAC alterado, JWT con firma/audiencia/emisor/expiración incorrectos, rotación, cache y endurecimiento SQL. `-IncludeAzure` compila también las bibliotecas y demo de Azure; self-test no usa una suscripción ni secretos reales.

**Verificación realizada al preparar esta entrega:** validación estática del XML, referencias a archivos/proyectos, listas de compilación, revisión de configuración pública y contenido del ZIP. **No se compiló ni se ejecutó el código VB/PowerShell ni DPAPI/Key Vault en este entorno**, que no dispone de Windows, MSBuild ni .NET Framework. Las pruebas incluidas están pendientes de ejecución en tu equipo. Tampoco se ha creado ningún recurso en Azure ni consultado tus suscripciones.

## Referencias

- DPAPI: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.protecteddata?view=netframework-4.8
- Alcances DPAPI: https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.dataprotectionscope
- SDK Key Vault: https://www.nuget.org/packages/Azure.Security.KeyVault.Secrets/4.11.2
- Azure Identity: https://www.nuget.org/packages/Azure.Identity/1.21.0
- JWT: https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/8.23.0
- Autenticación Key Vault: https://learn.microsoft.com/en-us/azure/key-vault/general/authentication
- App Configuration frente a Key Vault: https://learn.microsoft.com/en-us/azure/azure-app-configuration/faq
- DevOps y Key Vault: https://learn.microsoft.com/en-us/azure/devops/pipelines/library/link-variable-groups-to-key-vaults
- SQL: https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/connection-string-syntax
