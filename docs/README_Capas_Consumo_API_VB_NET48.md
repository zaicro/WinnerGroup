# Propuesta de capas y consumo de API — WinForms VB.NET / .NET Framework 4.8

## 1. Objetivo y alcance

Separar la interfaz, los casos de uso y el acceso a recursos externos en la aplicación actual. El equipo de backend construye los endpoints; el WinForms los consume mediante un contrato acordado. La base local sigue siendo accesible con ADO.NET.

Este documento propone una estructura pequeña y muestra una consulta HTTP con API key y JWT opcionales e independientes. La URL, los DTO y la operación del ejemplo son **ilustrativos**: no corresponden a un servicio existente. El contrato final determina rutas, encabezados, autenticación y respuestas.

No se requiere cambiar de ORM, motor o framework para comenzar.

## 2. Proyectos y referencias

Para la primera etapa, utiliza tres proyectos VB.NET dirigidos a **.NET Framework 4.8**, con `Option Strict On`. Mantener el mismo destino reduce cambios mientras se extrae el código existente.

| Proyecto | Contenido | Referencias a otros proyectos |
|---|---|---|
| `Demo.WinForms` | Formularios, eventos, presentación de errores y creación inicial de dependencias | Application e Infrastructure |
| `Demo.Application` | Casos de uso, interfaces y modelos usados por esos casos de uso | Ninguna de las otras dos capas |
| `Demo.Infrastructure` | Clientes HTTP, configuración de transporte y acceso ADO.NET a la base local | Application |

El formulario llama a un servicio de Application. El servicio utiliza interfaces; Infrastructure implementa esas interfaces. La referencia de WinForms a Infrastructure se usa para construir los objetos al iniciar la aplicación, no para dispersar SQL o HTTP en los eventos.

En Infrastructure puedes crear las carpetas `RemoteApi` y `LocalDatabase`. Una carpeta organiza archivos: no necesita convertirse en otro proyecto. Más adelante puedes separar infraestructura por motor si existe una necesidad concreta.

No se propone todavía una capa Domain independiente. Si aparecen entidades y reglas de negocio con suficiente contenido, se puede extraer entonces. Tampoco se requieren CQRS, repositorio genérico ni un contenedor de inyección de dependencias para empezar.

**Reglas de ubicación:**

- Formularios: controles, validación de entrada y presentación.
- Application: coordinar operaciones; no conoce `HttpClient`, `SqlConnection`, controles o `MessageBox`.
- Infrastructure: HTTP/JSON y SQL; no conoce formularios.
- Servidor API: autorización y reglas que protegen los datos compartidos. El cliente no es una frontera de seguridad confiable.

Separar capas lógicas no exige desplegarlas en máquinas distintas. La API sí constituye otro proceso y despliegue. Fuente: [arquitectura por capas y niveles](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/n-tier).

## 3. Acceso local y remoto

| Recurso | Contrato del cliente | Implementación |
|---|---|---|
| Datos compartidos | Una interfaz con operaciones concretas, como `IDatosRemotos` | Cliente HTTP hacia la API |
| Datos locales | Otra interfaz, por ejemplo `IResultadosLocales` | ADO.NET contra SQL Server o PostgreSQL local |

No fuerces ambos accesos a una interfaz genérica solo porque almacenan datos. Una llamada remota tiene fallos, latencia y permisos diferentes de una consulta local.

Si un caso de uso necesita ambos, Application coordina los dos contratos. No asumas que guardar localmente y enviar a la API es una transacción única. La sincronización, los reintentos y la prevención de duplicados se diseñan cuando se conozcan esas operaciones.

## 4. Contrato HTTP supuesto para el ejemplo

```http
GET https://api.ejemplo.invalid/api/v1/datos
Accept: application/json
X-Api-Key: <solo si corresponde>
Authorization: Bearer <solo si corresponde>
```

Respuesta supuesta: HTTP 200 con un arreglo JSON directo:

```json
[
  { "id": 1, "mensaje": "Dato compartido de prueba" }
]
```

Si el contrato real devuelve `{ "data": [...] }`, usa otro modelo de respuesta y mapea su contenido. No cambies el formulario para interpretar JSON.

| API key proporcionada | JWT proporcionado | Encabezados enviados |
|---|---|---|
| No | No | Ninguno de autenticación |
| Sí | No | `X-Api-Key` |
| No | Sí | `Authorization: Bearer ...` |
| Sí | Sí | Ambos |

**Que el cliente permita omitir credenciales no significa que el servidor acepte solicitudes anónimas.** El contrato define los requisitos. No se intenta otra combinación automáticamente si la API rechaza la solicitud.

`X-Api-Key` es una convención del ejemplo, configurable. Si el proveedor utiliza `Authorization` para la API key, hay que acordar cómo convive con Bearer; no pueden colocarse indiscriminadamente dos esquemas en ese mismo encabezado.

## 5. Preparación del ejemplo

1. Crea los tres proyectos y las referencias de la tabla anterior.
2. Establece sus espacios de nombres raíz en `Demo.Application`, `Demo.Infrastructure` y `Demo.WinForms`, respectivamente.
3. Copia cada bloque en el proyecto indicado, **sin envolverlo de nuevo en un Namespace**: Visual Basic agrega el RootNamespace automáticamente.
4. En Infrastructure agrega la referencia de framework `System.Net.Http` e instala `Newtonsoft.Json` 13.0.3:

```powershell
Install-Package Newtonsoft.Json -Version 13.0.3 -ProjectName Demo.Infrastructure
```

5. En WinForms agrega referencias a `System.Net.Http` y `System.Configuration`.
6. Utiliza .NET Framework 4.8 en todos. Los ejemplos no utilizan APIs exclusivas de .NET moderno.

La versión JSON está fijada para reproducir el ejemplo; no se afirma que sea la más reciente. Fuente: [paquete Newtonsoft.Json](https://www.nuget.org/packages/Newtonsoft.Json/13.0.3).

## 6. Application: modelo, interfaz y servicio

Archivo `DatosService.vb` en **Demo.Application**:

```vb
Imports System.Collections.Generic
Imports System.Threading
Imports System.Threading.Tasks

Public Class DatoDto
    Public Property Id As Integer
    Public Property Mensaje As String
End Class

Public Interface IDatosRemotos
    Function ObtenerAsync(ct As CancellationToken) As Task(Of List(Of DatoDto))
End Interface

Public Class DatosService
    Private ReadOnly _remotos As IDatosRemotos

    Public Sub New(remotos As IDatosRemotos)
        If remotos Is Nothing Then Throw New ArgumentNullException(NameOf(remotos))
        _remotos = remotos
    End Sub

    Public Function ObtenerAsync(ct As CancellationToken) As Task(Of List(Of DatoDto))
        Return _remotos.ObtenerAsync(ct)
    End Function
End Class
```

Este servicio delega porque el ejemplo solo consulta. No se inventan reglas para justificar la capa. En un caso de uso real puede coordinar datos locales, validaciones o transformaciones. Para una consulta aislada se podría inyectar directamente la interfaz en el formulario, pero mantener el servicio deja un punto uniforme para los futuros casos de uso.

Se usa un solo DTO para mantener el ejemplo breve. Si el contrato remoto cambia con independencia del modelo de aplicación, define DTO HTTP privados en Infrastructure y conviértelos al modelo que devuelve Application.

## 7. Infrastructure: cliente HTTP y credenciales opcionales

Archivo `DatosApiClient.vb` en **Demo.Infrastructure**:

```vb
Imports System.Collections.Generic
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading
Imports System.Threading.Tasks
Imports Demo.Application
Imports Newtonsoft.Json

' Una instantánea de las credenciales para una solicitud.
Public Class ApiCredentials
    Public ReadOnly Property ApiKey As String
    Public ReadOnly Property Jwt As String

    Public Sub New(Optional apiKey As String = Nothing,
                   Optional jwt As String = Nothing)
        Me.ApiKey = apiKey
        Me.Jwt = jwt
    End Sub
End Class

Public Class DatosApiClient
    Implements IDatosRemotos

    Private ReadOnly _http As HttpClient
    Private ReadOnly _getCredentials As Func(Of ApiCredentials)
    Private ReadOnly _apiKeyHeader As String

    Public Sub New(http As HttpClient,
                   getCredentials As Func(Of ApiCredentials),
                   Optional apiKeyHeader As String = "X-Api-Key")
        If http Is Nothing Then Throw New ArgumentNullException(NameOf(http))
        If getCredentials Is Nothing Then Throw New ArgumentNullException(NameOf(getCredentials))
        If http.BaseAddress Is Nothing OrElse http.BaseAddress.Scheme <> Uri.UriSchemeHttps Then
            Throw New ArgumentException("Configura una dirección base HTTPS.")
        End If
        If String.IsNullOrWhiteSpace(apiKeyHeader) OrElse
           apiKeyHeader.Equals("Authorization", StringComparison.OrdinalIgnoreCase) Then
            Throw New ArgumentException("La API key debe usar un encabezado propio según el contrato.")
        End If
        _http = http
        _getCredentials = getCredentials
        _apiKeyHeader = apiKeyHeader
    End Sub

    Public Async Function ObtenerAsync(ct As CancellationToken) _
        As Task(Of List(Of DatoDto)) Implements IDatosRemotos.ObtenerAsync

        Using request As New HttpRequestMessage(HttpMethod.Get, "api/v1/datos")
            request.Headers.Accept.Add(New MediaTypeWithQualityHeaderValue("application/json"))
            Dim credentials = _getCredentials()

            If credentials IsNot Nothing Then
                If Not String.IsNullOrWhiteSpace(credentials.ApiKey) Then
                    request.Headers.Add(_apiKeyHeader, credentials.ApiKey)
                End If
                If Not String.IsNullOrWhiteSpace(credentials.Jwt) Then
                    ' Jwt contiene solo el token, sin el prefijo "Bearer ".
                    request.Headers.Authorization =
                        New AuthenticationHeaderValue("Bearer", credentials.Jwt)
                End If
            End If

            Using response = Await _http.SendAsync(request, ct).ConfigureAwait(False)
                ' Incluye 3xx, 4xx y 5xx. No oculta errores como una lista vacía.
                If Not response.IsSuccessStatusCode Then
                    Throw New HttpRequestException(
                        "La API devolvió HTTP " & CInt(response.StatusCode).ToString() & ".")
                End If

                If CInt(response.StatusCode) <> 200 Then
                    Throw New InvalidOperationException("Se esperaba HTTP 200 según el contrato del ejemplo.")
                End If
                Dim contentType = response.Content.Headers.ContentType
                If contentType Is Nothing OrElse
                   Not String.Equals(contentType.MediaType, "application/json",
                                     StringComparison.OrdinalIgnoreCase) Then
                    Throw New InvalidOperationException("Se esperaba contenido application/json.")
                End If

                Dim json = Await response.Content.ReadAsStringAsync().ConfigureAwait(False)
                ct.ThrowIfCancellationRequested()
                Dim datos = JsonConvert.DeserializeObject(Of List(Of DatoDto))(json)
                If datos Is Nothing Then
                    Throw New InvalidOperationException("La respuesta no contiene el arreglo esperado.")
                End If
                Return datos
            End Using
        End Using
    End Function
End Class
```

Los encabezados se agregan a **cada HttpRequestMessage**, no se modifican las credenciales globales de `DefaultRequestHeaders`. El delegado obtiene las credenciales vigentes en cada operación, permitiendo usar un JWT renovado sin reconstruir el cliente.

La instancia `HttpClient` se recibe desde afuera y este cliente no la destruye. Sí libera cada request y response. `ConfigureAwait(False)` se utiliza dentro de Infrastructure; el evento del formulario conserva el contexto de interfaz al hacer su propio Await.

La API key y el JWT se envían tal como los entrega su fuente autorizada. Este código no obtiene, genera, firma ni renueva JWT. Eso depende del flujo de autenticación que defina el backend.

## 8. WinForms: construir dependencias una vez

Usa este bloque en el punto de inicio de tu aplicación, antes de abrir el formulario principal. Si usas el Application Framework de VB, adapta el evento Startup; **no necesitas agregar otro Sub Main ni otro Application.Run**.

Archivo `AppComposition.vb` en **Demo.WinForms**:

```vb
Imports System.Configuration
Imports System.Net.Http
Imports Demo.Application
Imports Demo.Infrastructure

Public Module AppComposition
    Private ReadOnly Handler As New HttpClientHandler With {
        .AllowAutoRedirect = False,
        .UseCookies = False
    }
    Private ReadOnly Http As New HttpClient(Handler) With {
        .BaseAddress = New Uri(ConfigurationManager.AppSettings("ApiBaseUrl")),
        .Timeout = TimeSpan.FromSeconds(20),
        .MaxResponseContentBufferSize = 1024 * 1024
    }

    Public Function CrearDatosService() As DatosService
        ' Solo para probar: valores opcionales tomados del entorno del proceso.
        ' En producción sustituir por el proveedor de configuración/sesión real.
        Dim credenciales As Func(Of ApiCredentials) =
            Function() New ApiCredentials(
                Environment.GetEnvironmentVariable("DEMO_API_KEY"),
                Environment.GetEnvironmentVariable("DEMO_API_JWT"))

        Return New DatosService(New DatosApiClient(Http, credenciales))
    End Function

    Public Sub Cerrar()
        ' Llamar una sola vez al terminar la aplicación y sus operaciones.
        Http.Dispose()
    End Sub
End Module
```

En `App.config` del WinForms:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="ApiBaseUrl" value="https://api.ejemplo.invalid/" />
  </appSettings>
</configuration>
```

La URL `.invalid` es intencional: cámbiala por la API real antes de ejecutar. Usa una URL base absoluta terminada en `/`. La ruta del cliente es relativa y no comienza con `/`; por ejemplo, una base `https://servidor/servicio/` conserva ese prefijo al agregar `api/v1/datos`.

Integra `appSettings` en el config que ya existe; no reemplaces otras secciones. La instancia HTTP se crea una vez y se reutiliza durante la aplicación. No cambies su BaseAddress/Timeout después de comenzar las solicitudes. Para otro servicio usa otro cliente configurado para ese destino, evitando enviar credenciales a hosts arbitrarios.

Se desactivan redirecciones automáticas para no reenviar una API key a un destino redirigido. Usa directamente la URL HTTPS final; no desactives validación de certificados para hacer funcionar la prueba.

El ejemplo simple no configura renovación del conjunto de conexiones ante cambios DNS durante procesos muy largos. Para producción define esa política con `IHttpClientFactory` o un manejo apropiado de conexiones en .NET Framework; no copies `PooledConnectionLifetime` de ejemplos de .NET moderno, porque no está disponible en el handler usado aquí.

Fuentes: [gestión de HttpClient](https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient-guidelines), [redirecciones y encabezados](https://learn.microsoft.com/en-us/dotnet/api/system.net.http.httpclienthandler.allowautoredirect).

## 9. WinForms: ejemplo de evento

En un formulario que ya tiene un botón `btnCargar` y un DataGridView `dgvDatos`, agrega estos miembros dentro de su clase. Para mantener el ejemplo breve se obtiene el servicio desde la composición; en una evolución puedes recibirlo por constructor.

```vb
Private ReadOnly _datosService As Demo.Application.DatosService =
    AppComposition.CrearDatosService()
Private ReadOnly _cierre As New System.Threading.CancellationTokenSource()

Private Async Sub btnCargar_Click(sender As Object, e As EventArgs) Handles btnCargar.Click
    btnCargar.Enabled = False
    Try
        Dim datos = Await _datosService.ObtenerAsync(_cierre.Token)
        If Not IsDisposed AndAlso Not Disposing Then dgvDatos.DataSource = datos
    Catch ex As OperationCanceledException
        ' En net48 un timeout también puede manifestarse como cancelación.
        If Not _cierre.IsCancellationRequested AndAlso Not IsDisposed Then
            MessageBox.Show("La solicitud superó el tiempo de espera.")
        End If
    Catch ex As Exception
        If Not IsDisposed AndAlso Not Disposing Then
            MessageBox.Show("No fue posible consultar la API. " & ex.Message)
        End If
    Finally
        If Not IsDisposed AndAlso Not Disposing Then btnCargar.Enabled = True
    End Try
End Sub

Protected Overrides Sub OnFormClosed(e As System.Windows.Forms.FormClosedEventArgs)
    _cierre.Cancel()
    MyBase.OnFormClosed(e)
End Sub
```

`Async Sub` corresponde al evento; los métodos que ejecutan trabajo devuelven Task. No uses `.Result` o `.Wait()` desde la interfaz. No necesitas `Task.Run` para estas llamadas HTTP.

Al integrar en un formulario existente, combina el cierre con su ciclo de vida actual y libera su CancellationTokenSource cuando ya hayan terminado las operaciones. No llames `AppComposition.Cerrar()` al cerrar cualquier formulario secundario: el HttpClient pertenece a toda la aplicación.

La captura general del evento evita excepciones no observadas en este ejemplo. En producción traduce errores HTTP a errores de aplicación para que UI no tenga que interpretar transporte. Implementa registro técnico con redacción de secretos; no muestres cuerpos de error ni tokens al usuario.

## 10. Cómo probar las cuatro combinaciones

El bloque de composición lee `DEMO_API_KEY` y `DEMO_API_JWT` del entorno **del proceso**. Para una prueba puedes iniciar el EXE desde PowerShell después de establecer solo las variables correspondientes. Evita pegar secretos en comandos que queden en el historial; puedes pedirlos con entrada oculta o utilizar la configuración de prueba de tu organización.

También puedes sustituir temporalmente el delegado por una de estas formas, usando exclusivamente valores ficticios hasta tener credenciales válidas:

```vb
' Sin credenciales:
Function() New ApiCredentials()

' Solo API key:
Function() New ApiCredentials(apiKey:="API_KEY_DE_PRUEBA")

' Solo JWT (sin prefijo Bearer):
Function() New ApiCredentials(jwt:="JWT_DE_PRUEBA")

' Ambos:
Function() New ApiCredentials(apiKey:="API_KEY_DE_PRUEBA", jwt:="JWT_DE_PRUEBA")
```

Estas son expresiones alternativas para asignar al delegado, no cuatro líneas para ejecutar juntas. Los valores ficticios no autentican. El éxito depende de lo que exija la API, no de que el código haya agregado el encabezado.

Una variable de entorno facilita la prueba, **no constituye una bóveda**. En producción el JWT normalmente proviene del flujo de inicio de sesión y permanece en la sesión; la API key tiene su propio aprovisionamiento y rotación. Un secreto distribuido en un cliente de escritorio puede ser extraído: la API debe aplicar permisos en el servidor.

## 11. Tratamiento de errores y credenciales

| Situación | Comportamiento a definir |
|---|---|
| 400 | Mostrar validaciones según el contrato; no reintentar sin cambiar datos |
| 401 | Revisar autenticación. Un token expirado es una posibilidad; acordar login/refresh |
| 403 | Acceso denegado según política del servidor; no intentar evadirlo cambiando encabezados |
| 404 | Recurso o ruta no encontrado, según el contrato |
| 429 | Respetar política de límites/Retry-After si el servidor lo usa |
| 5xx, red o timeout | Registrar diagnóstico y presentar una opción de recuperación |

El ejemplo conserva el código HTTP dentro del mensaje de la excepción, sin exponer el cuerpo remoto; no implementa toda esa traducción. No interpreta errores como lista vacía y no realiza reintentos ni refresh automático.

Para una futura escritura POST, acuerda un identificador de operación o mecanismo de idempotencia antes de reintentar: un timeout no demuestra que el servidor no haya guardado los datos.

## 12. Implementación gradual propuesta

1. Identifica una consulta compartida simple y conserva su comportamiento como referencia.
2. Extrae la llamada actual del formulario hacia una interfaz de Application.
3. Implementa y conecta el cliente HTTP cuando backend entregue OpenAPI, ejemplos y autenticación.
4. Verifica éxito, rechazo de credenciales, timeout, respuesta inválida y cancelación.
5. Extrae el ADO.NET local hacia Infrastructure sin cambiar todavía motor ni consultas.
6. Migra las escrituras después de acordar validaciones, permisos y prevención de duplicados.
7. Cuando ya no haya consumidores directos de la base compartida, retira sus credenciales y acceso de red de los equipos cliente.

## 13. Qué pedir al equipo de API

- OpenAPI y URL base por ambiente.
- Rutas, verbos HTTP, modelos, tipos, fechas y nulabilidad.
- Nombre/formato del encabezado API key, si aplica.
- Obtención y renovación del JWT, si aplica; confirmar si requiere una o ambas credenciales.
- Permisos por operación y cómo identifica al usuario, equipo o mesa.
- Formato de errores, paginación y límites.
- Reglas de reintento/idempotencia para escrituras y política de compatibilidad.

## 14. Validación y siguiente paso

Los ejemplos fueron revisados estáticamente para .NET Framework 4.8. **No fueron compilados ni ejecutados contra una API en este entorno**, que no dispone de MSBuild/.NET; tampoco existe todavía el contrato real. No se incluye servidor, proveedor de tokens, proyecto ejecutable ni cambios a los archivos anteriores.

Siguiente paso: crea los tres proyectos y migra una consulta mediante una interfaz. Ajusta DTO, URL y autenticación únicamente cuando el contrato esté confirmado. Mantén el refactor inicial separado de la migración de motor y del cifrado de las bases.
