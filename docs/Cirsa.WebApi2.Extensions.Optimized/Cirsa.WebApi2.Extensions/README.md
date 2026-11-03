# Cirsa.WebApi2.Extensions

Extensiones reutilizables de autenticación y Swagger para **ASP.NET Web API 2, VB.NET y .NET Framework 4.8**. Versión 0.1.1.

El paquete valida API keys con nombre y JWT HS256, emite JWT desde un identificador y claims genéricos, y documenta las políticas con Swagger 2.0. La aplicación anfitriona obtiene la configuración, crea el contenedor, instala su `IDependencyResolver` y administra su ciclo de vida.

## Contenido

- `Authentication/Attributes`: declaración de políticas mediante `ApiAuthorizeAttribute`.
- `Authentication/Configuration`: opciones y modos de autenticación.
- `Authentication/Policies`: resolución compartida entre ejecución y documentación.
- `Authentication/Validation`: validación de credenciales y configuración.
- `Authentication/Filters`: integración con el pipeline y desafíos HTTP 401.
- `Authentication/Tokens`: emisión de tokens sin modelos de negocio.
- `Authentication/Extensions`: registro de servicios y filtros.
- `Swagger`: opciones, registro, filtro de requisitos y JavaScript embebido.
- `tests`: ejecutable de pruebas net48 y pruebas del diálogo JavaScript.
- `examples`: ejemplo de registro completo, sin configuración ni secretos reales.

## Dependencias

Web API Core 5.2.9, DependencyInjection.Abstractions 8.0.0, IdentityModel 8.23.0 y Swashbuckle.Core 5.6.0. Se conservaron las versiones del proyecto recibido; no se realizó una actualización general de dependencias. El proyecto de pruebas agrega la implementación del contenedor de Microsoft. No se requiere que la biblioteca construya contenedores internos.

## Registro

La aplicación necesita una implementación de `IServiceCollection`/contenedor compatible; con Microsoft puede instalar `Microsoft.Extensions.DependencyInjection` 8.0.0. Las opciones se validan y copian al registrar los servicios. Cambiar el objeto original después del registro no rota las credenciales: hay que reconstruir la configuración/reiniciar la aplicación.

```vb
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration
Imports Cirsa.WebApi2.Extensions.Authentication.Extensions
Imports Cirsa.WebApi2.Extensions.Swagger.Configuration
Imports Cirsa.WebApi2.Extensions.Swagger.Extensions

' Los valores proceden del mecanismo de configuración del anfitrión.
services.AddApiAuthentication(New ApiAuthenticationOptions With {
    .EnableApiKey = True,
    .EnableJwt = True,
    .ApiKeys = apiKeysByName,
    .Issuer = issuer,
    .Audience = audience,
    .SigningKeyBase64 = signingKeyBase64
})

' Opcional: solamente si esta API también emite JWT.
services.AddJwtTokenIssuer(New JwtTokenIssuerOptions With {
    .Issuer = issuer,
    .Audience = audience,
    .SigningKeyBase64 = signingKeyBase64,
    .LifetimeMinutes = 15
})

' Después de construir UNA vez el proveedor raíz de la aplicación:
config.UseApiAuthentication(provider)
config.UseSwaggerDocumentation(New SwaggerOptions With {
    .Title = "Identity API",
    .Version = "v1",
    .IncludeAuthenticationSchemes = True
})

' Registrar todas las rutas antes de inicializar la configuración.
config.MapHttpAttributeRoutes()
config.EnsureInitialized()
```

El ejemplo de `examples/RegistrationExample.vb` separa el registro de servicios del registro HTTP. La biblioteca no construye ni elimina el proveedor recibido. Mantén el proveedor raíz durante la vida de la aplicación y conserva el scope por petición de tu `IDependencyResolver` existente.

`UseApiAuthentication` agrega un único filtro global y valida las políticas de los controladores descubiertos cuando se inicializa `HttpConfiguration`. Las políticas también se validan al procesar cada petición. Los controladores incorporados dinámicamente después de inicializar no tienen garantía de validación anticipada. Los registros duplicados lanzan `InvalidOperationException`.

### Solo API key

```vb
services.AddApiAuthentication(New ApiAuthenticationOptions With {
    .EnableApiKey = True,
    .EnableJwt = False,
    .ApiKeys = apiKeysByName
})
```

### Solo validación JWT

```vb
services.AddApiAuthentication(New ApiAuthenticationOptions With {
    .EnableApiKey = False,
    .EnableJwt = True,
    .Issuer = issuer,
    .Audience = audience,
    .SigningKeyBase64 = signingKeyBase64
})
```

Ambos mecanismos están habilitados por defecto para conservar el comportamiento anterior. Debe habilitarse al menos uno. Una política que mencione un mecanismo deshabilitado provoca error de configuración, incluso para `Either`.

## Políticas de endpoints

```vb
Imports System.Web.Http
Imports Cirsa.WebApi2.Extensions.Authentication.Attributes

<ApiAuthorize(AuthenticationSchemes:="ApiKey")>
Public Class ExampleController
    Inherits ApiController

    ' Hereda ApiKey del controlador.
    Public Function GetStatus() As IHttpActionResult
        Return Ok("ready")
    End Function

    <ApiAuthorize(AuthenticationSchemes:="Bearer")>
    Public Function GetProfile() As IHttpActionResult
        Return Ok(User.Identity.Name)
    End Function

    <ApiAuthorize(AuthenticationSchemes:="ApiKey,Bearer", RequireAllSchemes:=True)>
    Public Function GetRestricted() As IHttpActionResult
        Return Ok()
    End Function

    <ApiAuthorize(AuthenticationSchemes:="ApiKey,Bearer")>
    Public Function GetEither() As IHttpActionResult
        Return Ok()
    End Function

    <AllowAnonymous>
    Public Function GetPublic() As IHttpActionResult
        Return Ok()
    End Function
End Class
```

| Declaración efectiva                       | Comportamiento                                    |
| ------------------------------------------ | ------------------------------------------------- |
| Sin atributo                               | Público                                           |
| `ApiKey`                                   | API key válida                                    |
| `Bearer`                                   | JWT válido                                        |
| `ApiKey,Bearer`, `RequireAllSchemes=False` | Al menos uno válido (`Either`)                    |
| `ApiKey,Bearer`, `RequireAllSchemes=True`  | Los dos válidos (`Both`)                          |
| `AllowAnonymous` en método o controlador   | Público, incluso si hay política en el otro nivel |

El atributo del método **reemplaza**, no combina, el del controlador. `AuthenticationSchemes` no puede estar vacío; solo admite `ApiKey` y `Bearer`, sin distinguir mayúsculas y con espacios alrededor permitidos. Los valores desconocidos y las entradas vacías entre comas son errores.

En `Either`, una credencial válida permite entrar aunque la otra sea inválida. En modos individuales solo se evalúa el mecanismo indicado. Las respuestas de autenticación fallida son 401 con los `WWW-Authenticate` correspondientes, sin duplicados. La biblioteca no asigna permisos de negocio ni produce automáticamente un 403 por roles.

### Identidades y claims

La API key se envía en `X-Api-Key`. Cada valor configurado debe tener entre 32 y 512 caracteres, proceder de una fuente aleatoria segura y estar asociado a un nombre único. No se permiten claves repetidas entre nombres ni nombres repetidos ignorando mayúsculas. Se guarda un hash SHA-256 en el validador; se comparan todos los hashes configurados sin salida temprana. Esto no equivale a una demostración formal de tiempo constante del proceso completo.

Una API key válida agrega una identidad `ApiKey` con `api_key_name` y `credential_type=api_key`; nunca se agrega su valor al principal. Su `Identity.Name` es el nombre configurado, por ejemplo `baccarat`.

Si ambas credenciales son válidas en `Both` o `Either`, se preservan ambas identidades. La identidad JWT queda primero, por lo que `User.Identity.Name` sigue siendo el nombre del usuario del token. Para obtener el cliente, consulta **la identidad ApiKey**, no un claim arbitrario del conjunto combinado:

```vb
Dim principal = DirectCast(User, ClaimsPrincipal)
Dim clientIdentity = principal.Identities.FirstOrDefault(
    Function(identity) identity.AuthenticationType = "ApiKey")
Dim clientName = clientIdentity?.Name
```

Una API key identifica al cliente que posee el secreto; no prueba por sí sola qué usuario está operando. Tampoco se vincula automáticamente el cliente con `application_code` del JWT: esa autorización corresponde al anfitrión.

## JWT

```vb
Dim issued = tokenIssuer.Create(userId.ToString("D"), New Claim() {
    New Claim("name", userName),
    New Claim("role", "Administrator"),
    New Claim("application_code", applicationCode)
})
```

Las credenciales del usuario deben haberse validado antes de llamar a `Create`. El emisor protege `sub`, `jti`, `iat`, `iss`, `aud`, `nbf` y `exp`: no pueden reemplazarse mediante claims adicionales. La vida útil admitida es de 1 a 1440 minutos. La respuesta contiene `AccessToken` y `ExpiresAtUtc`.

La validación exige firma HS256, emisor, audiencia, expiración y vigencia; tolera 30 segundos de diferencia de reloj. La clave de firma es Base64 de **al menos 32 bytes aleatorios**, independiente de las API keys. Los tokens mayores al límite configurado de 8192 se rechazan; el emisor también evita generar tokens que excedan ese límite. Las entradas inválidas devuelven `False`, no detalles de las claves ni del token. Las opciones inválidas fallan al construir el servicio.

No incluye refresh tokens, revocación, rotación automática ni un servidor OAuth/OIDC. Configura HTTPS en el anfitrión y conserva los secretos fuera del código y de archivos publicados.

## Swagger y diálogo Authorize

Rutas predeterminadas de Swashbuckle: `/swagger` y `/swagger/docs/v1`. Si se incluye autenticación:

- Se agregan `securityDefinitions` para `ApiKey` y `Bearer` y requisitos por operación.
- `Both` se documenta con dos esquemas en un objeto (AND); `Either`, con dos objetos (OR).
- Las operaciones públicas tienen `security: []`.
- No se agregan parámetros de cabecera manuales a cada operación.
- Un botón **Authorize** abre el diálogo de API key/JWT. Acepta JWT con o sin prefijo `Bearer`.
- Apply reemplaza las credenciales guardadas: un campo vacío elimina ese mecanismo. Close conserva las credenciales aplicadas. Clear credentials elimina ambas.
- Las credenciales permanecen en el registro en memoria de Swagger UI, sin almacenamiento persistente ni parámetros en URL. Recargar la página las elimina. No se utiliza CDN.

Swagger 2.0 representa `Bearer` como un esquema `apiKey` en la cabecera Authorization; no dispone del esquema HTTP bearer de OpenAPI 3. Esto es documentación de autenticación, no validación. La UI usa los requisitos para adjuntar las credenciales seleccionadas; el servidor es quien exige `Both` o `Either`.

`IncludeAuthenticationSchemes` controla documentación y diálogo; no instala el filtro de autenticación. Si se deja `False`, el registro de Swagger es independiente. Swagger muestra ambos esquemas cuando se habilita esta opción; solo las operaciones que declaran cada esquema lo requieren.

`XmlDocumentationPath` es opcional. Si se proporciona una ruta inexistente, el registro falla explícitamente. Genera el XML del **proyecto API**, cópialo con la publicación y pasa su ruta desde el anfitrión.

## Compilación, pruebas y paquete

Desde PowerShell en Windows, con .NET SDK y .NET Framework 4.8 instalado:

```powershell
.\Verify.ps1
```

El script compila Release, ejecuta las pruebas net48 y, si Node/npm están disponibles, ejecuta las pruebas del diálogo. Informa si omite estas últimas. La compilación Release genera `bin\packages\Release\Cirsa.WebApi2.Extensions.0.1.1.nupkg`. No lo publica.

Comandos equivalentes:

```powershell
dotnet build .\Cirsa.WebApi2.Extensions.sln -c Release
& .\tests\Cirsa.WebApi2.Extensions.Tests\bin\Release\net48\Cirsa.WebApi2.Extensions.Tests.exe
Push-Location .\tests\ui
npm ci --ignore-scripts
npm test
Pop-Location
```

Las pruebas son un ejecutable con código de salida 0/1, sin framework de pruebas externo. `dotnet test` no las ejecuta. Consulta `VALIDATION.md` para los resultados reales y las limitaciones de esta revisión.

## Migración desde 0.1.0

1. Conserva los registros `AddApiAuthentication`, `AddJwtTokenIssuer`, `UseApiAuthentication` y `UseSwaggerDocumentation`.
2. Cambia `IncludeAuthenticationHeaders` por `IncludeAuthenticationSchemes`. La propiedad anterior sigue como alias obsoleto para facilitar la transición.
3. Elimina registros manuales de `SwaggerAuthenticationHeadersFilter`. Fue reemplazado por `SwaggerSecurityRequirementsFilter` y se registra desde `UseSwaggerDocumentation`.
4. No agregues manualmente un segundo `AuthenticationFilter` ni dupliques los registros de servicios.
5. Corrige políticas vacías, desconocidas o que hagan referencia a mecanismos deshabilitados: ahora se detectan durante la inicialización.
6. Si habilitabas ambos mecanismos pero solamente necesitabas uno, deshabilita el otro explícitamente en las opciones.
7. Si dependías de que una API key desapareciera al validar JWT, ajusta la lectura de identidades: ahora ambas se conservan.

Se conservan el namespace público de `AuthenticationMode` (`Authentication.Configuration`), el nombre `AuthorizeValidator` y su constructor anterior. El archivo del enum se movió a Configuration para que la carpeta coincida con su namespace. El paquete no migra a ASP.NET Core ni cambia el lenguaje o el framework.
