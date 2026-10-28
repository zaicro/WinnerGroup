# API VB.NET · .NET Framework 4.8

Proyecto ASP.NET Web API 2 para Windows / IIS Express / IIS, con Swagger 2.0, JWT HS256 y API key. Código VB.NET con Option Strict On.

## Abrir y ejecutar

Requisitos: Visual Studio 2022 con carga de trabajo **ASP.NET y desarrollo web**, soporte de VB.NET, .NET Framework 4.8 targeting pack e IIS Express. Use **Windows PowerShell 5.1**, con .NET Framework 4.8 instalado.

Desde la carpeta que contiene la solución:

```powershell
.\Setup-Local.ps1 -Username demo
.\Build.ps1
```

El primer script pide una contraseña de 12 a 256 caracteres. Genera una API key, una clave de firma distinta y el hash de contraseña PBKDF2-SHA256 con salt aleatorio y 600.000 iteraciones. No incluye secretos predeterminados. No sobrescribe configuraciones existentes.

Abra `ApiVb48.sln`, establezca el proyecto como inicio y ejecute IIS Express. La URL configurada es `https://localhost:44380/swagger`. Confíe en el certificado de desarrollo de IIS Express cuando Visual Studio lo solicite. Si el puerto está ocupado, cambie el puerto SSL en las propiedades del proyecto y adapte la URL de las pruebas.

`Build.ps1` restaura NuGet, compila y aplica a Web.config los binding redirects generados por MSBuild. Si modifica paquetes, vuelva a ejecutarlo antes de arrancar. No es un proyecto ASP.NET Core y no se ejecuta con `dotnet run`.

## Rutas y credenciales

| Método | Ruta | Requisitos |
| --- | --- | --- |
| POST | `/api/v1/auth/token` | HTTPS + API key + usuario/contraseña; proveedor demo habilitado |
| GET | `/api/v1/personas` | HTTPS + API key + JWT |
| GET | `/api/v2/personas` | HTTPS + API key + JWT |
| GET | `/swagger` | Disponible solo con SwaggerEnabled=true |
| GET | `/swagger/docs/v1` | Documento de v1 |
| GET | `/swagger/docs/v2` | Documento de v2 |

Las rutas de negocio exigen **ambas credenciales**. No son alternativas. La API key identifica al cliente consumidor; el JWT identifica al usuario. Esta base utiliza una única API key; no implementa un registro de clientes ni rotación gradual de claves.

```http
X-Api-Key: TU_API_KEY
Authorization: Bearer TU_JWT
```

El JWT dura 15 minutos y se validan firma, algoritmo HS256, emisor, audiencia y expiración, con 30 segundos de tolerancia de reloj. No hay refresh tokens ni revocación anticipada. Un token sirve para ambas versiones porque la audiencia es la misma. Invalidar la clave de firma invalida todos los tokens.

## Probar con Swagger

1. Consulte `ApiKey` en `ApiVb48/LocalSecrets.config`.
2. En los campos superiores de Swagger, pegue esa API key y pulse **Aplicar credenciales**.
3. Abra el documento v1 y ejecute `POST /api/v1/auth/token` con:

```json
{
  "Username": "demo",
  "Password": "LA_CONTRASEÑA_QUE_CONFIGURASTE"
}
```

4. Copie `access_token`, péguelo en el campo JWT **sin** el prefijo `Bearer` y vuelva a aplicar.
5. Ejecute personas en v1 y luego en v2. Si cambia el documento, vuelva a aplicar las credenciales.

El script de Swagger mantiene las credenciales en memoria; no usa localStorage. Swagger está fuera del filtro de controladores y sus documentos no exigen autenticación cuando está habilitado. Déjelo deshabilitado o restrinja su acceso por IIS/red al desplegar.

## Versionado de controladores

Se usa **versionado explícito por rutas**, sin una librería de negociación de versiones:

- `Controllers/V1/PersonasV1Controller.vb`: `<RoutePrefix("api/v1/personas")>`.
- `Controllers/V2/PersonasV2Controller.vb`: `<RoutePrefix("api/v2/personas")>`.
- `App_Start/SwaggerConfig.vb`: documentos filtrados por el prefijo de cada ruta.

Los namespaces organizan el código; **RoutePrefix es lo que publica la versión**. Los nombres de clase son distintos para evitar ambigüedades. Solo se registran rutas por atributos: no existe una ruta convencional sin versión.

Para añadir v3, cree el controlador con prefijo `api/v3/...` y registre `versions.Version("v3", ...)` en SwaggerConfig. No cambie el contrato de v1 al crear v2; mueva la lógica compartida a servicios para no duplicarla. Las respuestas de ejemplo son diferentes deliberadamente para mostrar cómo evolucionar un contrato. No existe persistencia ni CRUD en esta base.

Si necesita negociación por cabeceras, publicación de versiones soportadas/deprecadas o políticas automáticas de versiones, conviene integrar una biblioteca de API Versioning; esta plantilla no implementa esas funciones.

## Seguridad y configuración

- `Security/ApiSecurityFilter.vb` se registra globalmente y protege por defecto todos los controladores. Una acción nueva exige API key y JWT.
- `<ApiKeyOnly>` es una excepción explícita para el login: omite JWT, conserva la API key. `[AllowAnonymous]` no omite este filtro propio.
- El filtro asigna el principal validado a RequestContext.Principal; `User.Identity.Name` queda disponible en el controlador.
- No se autorizan permisos ni roles de negocio en esta plantilla. Añada esas políticas antes de exponer operaciones sensibles.
- Se rechaza HTTP en las acciones de la API con 403. En producción, configure también el binding y la restricción HTTPS en IIS, cubriendo Swagger y contenido estático. Para esta base, termine TLS en IIS; detrás de un proxy, la conexión proxy-IIS también debe ser HTTPS.
- No hay CORS habilitado. Un cliente WinForms no necesita CORS.
- Una API key distribuida dentro de una aplicación WinForms puede extraerse: no es prueba fuerte de identidad del usuario ni sustituye el JWT.
- `LocalSecrets.config` está ignorado por Git y tiene extensión bloqueada por ASP.NET. No lo publique como archivo estático ni lo copie a repositorios. Restrinja sus permisos de lectura a la identidad de la aplicación y administradores.
- Cada ajuste admite una variable de entorno `APIVB48_` + nombre, por ejemplo `APIVB48_ApiKey`, `APIVB48_JwtSigningKeyBase64`, `APIVB48_JwtIssuer`, `APIVB48_JwtAudience`. La variable tiene precedencia sobre appSettings. Reinicie el proceso de IIS cuando cambie variables.

`Setup-Local.ps1` habilita explícitamente Swagger y el proveedor de login demo **solo para desarrollo**. Web.config los deja deshabilitados por defecto. La aplicación falla al iniciar si faltan la API key o la clave JWT.

Para producción, sustituya la validación del usuario demo en AuthController por su proveedor real de usuarios (base de datos con hashes adecuados, o proveedor de identidad) y retire la dependencia de DemoLoginEnabled. Configure limitación de intentos y auditoría del login en la aplicación o gateway antes de exponerlo a Internet. El PBKDF2 evita almacenar contraseñas en claro, pero no evita ataques de fuerza bruta ni saturación del login. Nunca registre JWT, API keys o contraseñas en logs.

Si ya dispone de un proveedor OAuth/OIDC, use su emisión y validación de tokens en lugar de mantener otro emisor local. No cambie simplemente issuer/audience de esta plantilla: el código actual solo acepta HS256 con su propia clave simétrica.

## Verificación

Con IIS Express iniciado y su certificado confiable:

```powershell
.\Test-Smoke.ps1
# Si usa otra URL:
.\Test-Smoke.ps1 -BaseUrl https://localhost:OTRO_PUERTO
```

Prueba login válido; rechazo por credenciales ausentes o incorrectas; JWT malformado, expirado, con emisor o audiencia incorrectos; ambas versiones válidas; v3 inexistente; separación de documentos Swagger y seguridad AND. Requiere las credenciales generadas localmente y los valores predeterminados de issuer/audience. No omite validación TLS. La interfaz visual de Swagger y la incorporación de cabeceras deben comprobarse manualmente en el navegador.

**Validación realizada al generar esta entrega:** revisión estática de configuración, estructura y recursos, y sintaxis JavaScript. No se compiló ni se ejecutaron las pruebas HTTP: el entorno de generación no tiene Windows, MSBuild ni .NET Framework 4.8. Ejecute Build.ps1 y Test-Smoke.ps1 en Windows antes de integrarlo.

## Paquetes fijados y referencias

- Microsoft.AspNet.WebApi.WebHost 5.3.0: https://www.nuget.org/packages/Microsoft.AspNet.WebApi.WebHost/5.3.0
- Swashbuckle.Core 5.6.0: https://www.nuget.org/packages/Swashbuckle.Core/5.6.0
- System.IdentityModel.Tokens.Jwt 8.23.0: https://www.nuget.org/packages/System.IdentityModel.Tokens.Jwt/8.23.0
- Newtonsoft.Json 13.0.3: https://www.nuget.org/packages/Newtonsoft.Json/13.0.3
- Swashbuckle Web API / documentos múltiples: https://github.com/domaindrivendev/Swashbuckle.WebApi
- Rutas por atributos en Web API 2: https://learn.microsoft.com/en-us/aspnet/web-api/overview/web-api-routing-and-actions/attribute-routing-in-web-api-2

Swashbuckle.Core es la integración clásica de Web API y genera Swagger 2.0; no confundir con Swashbuckle.AspNetCore. Sus dependencias son antiguas: revise actualizaciones y vulnerabilidades al integrar y desplegar. Las versiones fijadas son una referencia reproducible, no una garantía permanente de seguridad.
