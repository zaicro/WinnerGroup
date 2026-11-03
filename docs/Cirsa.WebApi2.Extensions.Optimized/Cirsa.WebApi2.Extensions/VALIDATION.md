# Validación de la revisión

Fecha: 2026-11-02. Versión revisada: 0.1.1.

## Comprobaciones realizadas

- Compilación de la solución en Debug y Release con .NET SDK 8.0.427 y referencias de .NET Framework 4.8: sin errores ni advertencias.
- Ejecución de las pruebas sobre los ensamblados net48 con Mono 6.8 en Linux: **12 grupos aprobados**.
- Prueba del diálogo de autorización con Node y jsdom: aprobada.
- Generación del NuGet Release y comprobación de su contenido: DLL net48, XML de documentación, README y dependencias. El JavaScript es un recurso embebido en la DLL, no un archivo externo que el consumidor deba copiar.
- El ejemplo de registro se incluye como archivo vinculado en el proyecto de pruebas para comprobar que compila.

## Cobertura de las pruebas

1. Precedencia del método, políticas de controlador, AllowAnonymous y esquemas inválidos.
2. API keys válidas, inválidas, ausentes, demasiado largas, UTF-16 inválido, nombres/valores duplicados y snapshot de configuración.
3. Emisión/validación de JWT, nombre/rol/subject, claims reservados, token excesivo, emisor/audiencia/firma incorrectos, expiración, vigencia futura y token sin firma.
4. Habilitación independiente de los mecanismos y rechazo de políticas incompatibles.
5. Matriz de 36 combinaciones: cuatro modos y tres estados por credencial (ausente/válida/inválida). También cabeceras API key duplicadas.
6. Conservación de identidades JWT y ApiKey sin exponer la clave.
7. Desafíos 401 con los esquemas correspondientes y sin duplicados, tanto al rechazar credenciales como al devolver un 401 desde una acción.
8. Requisitos Swagger por operación, AND/OR, endpoints públicos y conservación de respuestas existentes.
9. Servicios singleton, emisión desde DI y protección contra registros duplicados.
10. Error anticipado al inicializar una configuración con una política inválida.
11. HTTP en memoria mediante `HttpServer`: documento Swagger, interfaz HTML, carga del JavaScript embebido y un endpoint Both que rechaza sin credenciales y acepta con API key/JWT válidos.
12. Presencia del recurso JavaScript en el ensamblado compilado.

El test JavaScript comprueba Apply, Clear, prefijo Bearer, foco, Escape, instalación única, cliente aún no disponible y ausencia de acceso a localStorage/sessionStorage. Comprueba el contrato del registro de autorizaciones de Swagger UI 2 mediante un doble de prueba; la carga real del recurso se comprueba en el test HTTP. No sustituye una prueba visual en navegador.

## Límites

Esta ejecución no utilizó Windows ni IIS/IIS Express. Mono necesitó sus bibliotecas de framework, una fachada netstandard y el runtime Microsoft.VisualBasic 10.3.0 para ejecutar los binarios; estos auxiliares **no** se incluyen en el código, en el NuGet ni en el ZIP. El proyecto mantiene su referencia normal al runtime de VB de .NET Framework.

No se validaron configuración de IIS, publicación de la API consumidora, HTTPS, CORS, bindings del anfitrión, políticas CSP del sitio ni el ciclo de vida de su resolver DI. Tampoco se hizo una auditoría formal de seguridad ni un benchmark. No se actualizó el conjunto de dependencias funcionales a versiones nuevas.

## Siguiente comprobación en el entorno del usuario

1. Ejecutar `Verify.ps1` en Windows.
2. Referenciar el proyecto o instalar el NuGet en una API de prueba y aplicar la sección de migración del README.
3. Probar en IIS Express/DEV: público, ApiKey, Bearer, Both y Either, incluyendo credenciales inválidas/vencidas y el diálogo Authorize.
4. Confirmar en DEV que el proveedor raíz y los scopes por petición del anfitrión se eliminan correctamente antes de publicar el paquete en el feed interno.
