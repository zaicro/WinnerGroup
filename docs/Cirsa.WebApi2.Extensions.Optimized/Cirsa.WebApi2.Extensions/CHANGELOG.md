# Cambios en 0.1.1

## Correcciones

- Una petición válida con API key y JWT conserva ambas identidades; se mantiene el nombre del usuario JWT y el nombre del cliente API key.
- Runtime y Swagger usan el mismo resolvedor: método sobre controlador, AllowAnonymous sobre ambos y endpoints sin atributo públicos.
- Se reemplazaron cabeceras manuales de Swagger por requisitos de seguridad por operación, conservando AND/OR.
- El diálogo Authorize utiliza objetos compatibles con el registro de Swagger UI 2; no depende de un constructor global inexistente.
- Los desafíos HTTP 401 no se duplican y también se agregan cuando una acción protegida devuelve 401.

## Configuración y mantenimiento

- API key y JWT se habilitan de forma independiente, con validación anticipada de políticas.
- Las opciones se validan al registrar y se toman como snapshot; no se construyen contenedores internos.
- Se detectan registros duplicados de servicios, filtro y Swagger.
- Se centralizaron utilidades de hashing y decodificación de la clave de firma.
- Las entradas UTF-16 inválidas se rechazan al validar API keys.
- La emisión de JWT impide exceder el tamaño aceptado por el validador.
- Se eliminó la referencia duplicada de Web API y se redujo la dependencia del contenedor a Abstractions.
- Se conserva net48, VB.NET y la versión de las dependencias funcionales del proyecto recibido.
- Se agregaron documentación de uso/migración, solución, pruebas ejecutables y script de verificación.

## Compatibilidad

`IncludeAuthenticationHeaders` permanece como alias obsoleto de `IncludeAuthenticationSchemes`. Se eliminó el filtro público `SwaggerAuthenticationHeadersFilter`: los consumidores que lo registraban directamente deben quitarlo. El enum mantiene su namespace público. No se intentó mantener compatibilidad binaria con consumidores compilados que utilicen el filtro eliminado; recompila la API al actualizar.
