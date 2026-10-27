Imports System.Collections.Concurrent
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Security.Cryptography
Imports System.Text
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Microsoft.IdentityModel.Tokens
Imports TableGamesAuth.Application
Imports TableGamesAuth.Security

Namespace TableGamesAuth.Api
    Public Class ErrorHandler
        Inherits DelegatingHandler
        Private ReadOnly logger As IAppLogger
        Public Sub New(logger As IAppLogger)
            Me.logger = logger
        End Sub
        Protected Overrides Async Function SendAsync(request As HttpRequestMessage, ct As CancellationToken) As Task(Of HttpResponseMessage)
            Dim timer = System.Diagnostics.Stopwatch.StartNew()
            Dim correlationId = Guid.NewGuid().ToString("N")
            request.Properties("CorrelationId") = correlationId
            Dim response As HttpResponseMessage
            Try
                response = Await MyBase.SendAsync(request, ct)
            Catch ex As AppException
                response = request.CreateResponse(CType(ex.Status, HttpStatusCode), New With {.error = ex.Message, .correlationId = correlationId})
            Catch ex As OperationCanceledException When ct.IsCancellationRequested
                Throw
            Catch ex As Exception
                logger.Error("http_failed correlation=" & correlationId, ex)
                response = request.CreateResponse(HttpStatusCode.InternalServerError, New With {.error = "Error interno.", .correlationId = correlationId})
            End Try
            response.Headers.TryAddWithoutValidation("X-Correlation-ID", correlationId)
            response.Headers.TryAddWithoutValidation("X-Content-Type-Options", "nosniff")
            response.Headers.CacheControl = New Headers.CacheControlHeaderValue With {.NoStore = True}
            Dim summary = "http_completed correlation=" & correlationId & " method=" & request.Method.Method & " status=" & CInt(response.StatusCode).ToString() & " elapsedMs=" & timer.ElapsedMilliseconds.ToString()
            If CInt(response.StatusCode) >= 400 Then
                logger.Warn(summary)
            Else
                logger.Info(summary)
            End If
            Return response
        End Function
    End Class
    Public Class SecurityHandler
        Inherits DelegatingHandler
        Private ReadOnly settings As Settings
        Private ReadOnly tokens As ITokenService
        Private ReadOnly counters As New ConcurrentDictionary(Of String, RateWindow)
        Private Class RateWindow
            Public Started As DateTime = DateTime.UtcNow
            Public Count As Integer
        End Class
        Public Sub New(settings As Settings, tokens As ITokenService)
            Me.settings = settings : Me.tokens = tokens
        End Sub
        Private Sub CheckRate(key As String, limit As Integer)
            Dim window = counters.GetOrAdd(key, Function(k) New RateWindow())
            SyncLock window
                If DateTime.UtcNow - window.Started >= TimeSpan.FromMinutes(1) Then
                    window.Started = DateTime.UtcNow : window.Count = 0
                End If
                window.Count += 1
                If window.Count > limit Then Throw New AppException(429, "Límite de peticiones alcanzado. Reintente en un minuto.")
            End SyncLock
        End Sub
        Protected Overrides Async Function SendAsync(request As HttpRequestMessage, ct As CancellationToken) As Task(Of HttpResponseMessage)
            If request.RequestUri.Scheme <> "https" AndAlso Not (settings.AllowInsecureLoopback AndAlso request.RequestUri.IsLoopback) Then Throw New AppException(403, "HTTPS obligatorio.")
            Dim values As IEnumerable(Of String) = Nothing
            If Not request.Headers.TryGetValues("X-Api-Key", values) OrElse values.Count() <> 1 Then Throw New AppException(401, "API key inválida.")
            Dim rawKey = values.Single()
            If rawKey.Length < 32 OrElse rawKey.Length > 256 Then Throw New AppException(401, "API key inválida.")
            Dim digest As Byte()
            Using sha = SHA256.Create()
                digest = sha.ComputeHash(Encoding.UTF8.GetBytes(rawKey))
            End Using
            Dim keyAppId = Guid.Empty
            For Each entry In settings.ApiKeys
                Dim expected = Enumerable.Range(0, 32).Select(Function(i) Convert.ToByte(entry.Sha256.Substring(i * 2, 2), 16)).ToArray()
                If Pbkdf2PasswordHasher.SecureEquals(expected, digest) Then keyAppId = entry.AppId
            Next
            If keyAppId = Guid.Empty Then Throw New AppException(401, "API key inválida.")
            request.Properties("KeyAppId") = keyAppId
            CheckRate(keyAppId.ToString() & ":all", 600)
            Dim path = request.RequestUri.AbsolutePath.TrimEnd("/"c)
            Dim isLogin = String.Equals(path, "/api/v1/auth/login", StringComparison.OrdinalIgnoreCase) AndAlso request.Method = HttpMethod.Post
            If isLogin Then CheckRate(keyAppId.ToString() & ":login", 30)
            If request.Content IsNot Nothing Then
                If request.Content.Headers.ContentLength.HasValue AndAlso request.Content.Headers.ContentLength.Value > 32768 Then Throw New AppException(413, "Cuerpo demasiado grande.")
                Try
                    Await request.Content.LoadIntoBufferAsync(32768)
                Catch ex As HttpRequestException
                    Throw New AppException(413, "Cuerpo demasiado grande.")
                End Try
            End If
            If Not isLogin Then
                Dim auth = request.Headers.Authorization
                If auth Is Nothing OrElse Not String.Equals(auth.Scheme, "Bearer", StringComparison.OrdinalIgnoreCase) OrElse String.IsNullOrEmpty(auth.Parameter) OrElse auth.Parameter.Length > 8192 Then Throw New AppException(401, "Token requerido.")
                Dim principal As System.Security.Claims.ClaimsPrincipal
                Try
                    principal = tokens.Validate(auth.Parameter)
                Catch ex As SecurityTokenException
                    Throw New AppException(401, "Token inválido o vencido.")
                Catch ex As ArgumentException
                    Throw New AppException(401, "Token inválido.")
                End Try
                Dim service = CType(request.GetDependencyScope().GetService(GetType(AuthService)), AuthService)
                principal = Await service.ValidateSession(principal, keyAppId, ct)
                If principal Is Nothing Then Throw New AppException(401, "Sesión revocada o aplicación no autorizada.")
                request.GetRequestContext().Principal = principal
            End If
            Return Await MyBase.SendAsync(request, ct)
        End Function
    End Class
End Namespace
