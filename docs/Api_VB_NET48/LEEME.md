# Corrección de autenticación por endpoint

Aplicar sobre el proyecto ApiVb48 ya configurado. Este paquete no reemplaza Web.config, LocalSecrets.config, JwtService.vb ni las correcciones de ensamblados realizadas durante la instalación.

1. Detenga IIS Express.
2. Reemplace por completo Security/ApiSecurityFilter.vb. La definición antigua de ApiKeyOnlyAttribute se elimina al reemplazar este archivo.
3. Agregue Security/AuthenticationPolicyAttribute.vb al proyecto. El vbproj original usa un wildcard de archivos VB; si su proyecto tiene inclusiones explícitas, agregue también el nuevo archivo al vbproj.
4. Reemplace el archivo SecurityRequirementsFilter.vb en su carpeta SwaggerAssets (o donde esté actualmente). No mantenga dos copias de esa clase. Su namespace sigue siendo Swagger.
5. Mantenga config.Filters.Add(New ApiSecurityFilter()) en WebApiConfig.Register. No registre además un AuthorizeAttribute global; ese otro filtro exigiría autenticación incluso a los endpoints públicos. No mezcle los atributos nuevos con Authorize sin diseñar la política resultante.
6. Recompile, reinicie IIS Express y recargue Swagger. No necesita volver a ejecutar Setup-Local.ps1 para instalar este parche.

## Contrato nuevo

| Declaración | Requiere |
| --- | --- |
| Sin atributo de seguridad ni política en el controlador | Nada: público |
| ApiKeyOnly | API key válida |
| JwtOnly | JWT válido |
| ApiKeyOrJwt | Al menos una de las dos credenciales válida |
| ApiKeyAndJwt | Ambas credenciales válidas |
| AllowAnonymous | Público, incluso si el controlador tiene una política |

Todos los endpoints conservan el requisito de transporte HTTPS. Es independiente de autenticación.

Una política en la acción reemplaza la del controlador. Si la acción no tiene política, hereda la del controlador. Use solo un atributo de política en cada nivel. AllowAnonymous tiene precedencia sobre todos ellos.

Ejemplos dentro de un ApiController con Imports ApiVb48.Security:

```vb
' Público, siempre que el controlador tampoco tenga política.
<HttpGet, Route("")>
Public Function GetAll() As IHttpActionResult
    Return Ok("Público")
End Function

<HttpGet, Route("por-key"), ApiKeyOnly>
Public Function ByKey() As IHttpActionResult
    Return Ok("API key válida")
End Function

<HttpGet, Route("por-jwt"), JwtOnly>
Public Function ByJwt() As IHttpActionResult
    Return Ok(New With {.usuario = User.Identity.Name})
End Function

<HttpGet, Route("cualquiera"), ApiKeyOrJwt>
Public Function EitherCredential() As IHttpActionResult
    Return Ok("Al menos una credencial válida")
End Function

<HttpGet, Route("ambos"), ApiKeyAndJwt>
Public Function BothCredentials() As IHttpActionResult
    Return Ok("Ambas credenciales válidas")
End Function
```

El login existente puede conservar <HttpPost, Route("token"), ApiKeyOnly>.
ApiKeyOrJwt acepta una API key válida aunque el JWT falte o sea inválido. Cuando entra solo por API key, no se establece identidad de usuario. Para acciones que necesitan un usuario identificado, use JwtOnly o ApiKeyAndJwt.
En endpoints públicos este parche no intenta validar un JWT opcional ni establecer un principal a partir de él.

## Diagnóstico del JWT

Este cambio corrige las políticas; no demuestra que el JWT se esté validando correctamente. La rama OR puede aprobar una API key válida sin examinar el JWT. Compruebe JWT por separado con JwtOnly y sin X-Api-Key.

En Swagger pegue únicamente access_token (sin comillas, sin Bearer y sin el JSON completo) y pulse Aplicar credenciales. En Postman/HTTP el encabezado debe ser Authorization: Bearer TOKEN, con un solo prefijo Bearer.

Si sigue recibiendo "JWT inválido.", ponga un breakpoint en Catch ex As ArgumentException de ApiSecurityFilter.vb e inspeccione ex.Message y ex.InnerException. No devuelve esos detalles al cliente. Puede haber errores internos de decodificación o carga de dependencias; el texto genérico no identifica la causa. No comparta el token ni las claves para diagnosticarlo.

## Comprobación en Windows

Pruebe sin ninguna cabecera de autenticación un endpoint público: 200. En endpoints protegidos: sin credenciales 401; ApiKeyOnly con solo key válida 200; JwtOnly con solo JWT válido 200; ApiKeyOrJwt con key válida y sin JWT 200 y con JWT válido y sin key 200; ApiKeyAndJwt con una sola credencial 401 y con ambas válidas 200. En cualquiera de los protegidos, si ninguna de las credenciales exigidas es válida, espere 401. Revise las políticas en el controlador si una acción sin atributo sigue protegida.

Swagger utiliza el mismo resolvedor de políticas que la ejecución. OR se documenta con dos objetos security; AND con un solo objeto con ambos esquemas; público con security: []. Los esquemas siguen siendo ApiKey y Bearer, por lo que no se modifica SwaggerConfig ni credentials.js.

No se compiló ni se ejecutó en este entorno: no dispone de Windows / MSBuild / .NET Framework 4.8. Los scripts de prueba del ZIP original asumían seguridad AND global; ese supuesto ya no se aplica. Use la matriz anterior al probar sus controladores con las políticas elegidas.
