Imports System.IO
Imports System.Security.Cryptography
Imports System.Xml.Linq
Imports System.IdentityModel.Tokens.Jwt
Imports Microsoft.IdentityModel.Tokens
Imports SensitiveData48.Core

Module SelfTests
    Private _count As Integer
    Public Sub Run()
        _count = 0
        Dim directory = Path.Combine(Path.GetTempPath(), "SensitiveData48-tests-" & Guid.NewGuid().ToString("N"))
        Dim store As New DpapiSecretStore(directory)
        Try
            Dim secret = NewRandomKeyBase64()
            store.PutSecret("roundtrip", secret)
            Check(store.GetSecret("roundtrip").Value = secret, "DPAPI round trip")
            Check(Not File.ReadAllText(Path.Combine(directory, "roundtrip.secret.xml")).Contains(secret), "Sin secreto en texto plano")
            ExpectFailure(Of IOException)(Sub() store.PutSecret("roundtrip", "otro"), "No sobrescribe implícitamente")
            ExpectFailure(Of ArgumentException)(Sub() store.GetSecret("../escape"), "Rechaza rutas arbitrarias")
            Dim tamperedPath = Path.Combine(directory, "roundtrip.secret.xml")
            Dim document = XDocument.Load(tamperedPath)
            Dim bytes = Convert.FromBase64String(document.Root.Value)
            bytes(bytes.Length - 1) = CByte(CInt(bytes(bytes.Length - 1)) Xor 1)
            document.Root.Value = Convert.ToBase64String(bytes)
            document.Save(tamperedPath)
            ExpectFailure(Of CryptographicException)(Sub() store.GetSecret("roundtrip"), "DPAPI rechaza archivo alterado")
            store.PutSecret("jwt-signing-v1", NewRandomKeyBase64())
            store.PutSecret("jwt-signing-v2", NewRandomKeyBase64())
            store.PutSecret("hmac-v1", NewRandomKeyBase64())
            Dim apiKey = NewRandomKeyBase64()
            Dim fingerprint = ApiKeyFingerprint(apiKey)
            Check(VerifyApiKey(apiKey, fingerprint), "API key válida")
            Check(Not VerifyApiKey(NewRandomKeyBase64(), fingerprint), "API key incorrecta")
            Dim hmac As New HmacService(store)
            Dim tag = hmac.Sign("mensaje")
            Check(hmac.Verify("mensaje", tag), "HMAC válido")
            Check(Not hmac.Verify("alterado", tag), "HMAC rechaza contenido alterado")
            Dim jwt As New JwtTokenService(store, "issuer", "audience", "v1", New String() {"v1"})
            Dim token = jwt.Issue("subject")
            Check(jwt.Validate(token).Identity.Name = "subject", "JWT válido")
            Dim badAudience As New JwtTokenService(store, "issuer", "other-audience", "v1", New String() {"v1"})
            ExpectFailure(Of SecurityTokenException)(Sub() badAudience.Validate(token), "JWT audiencia incorrecta")
            Dim badIssuer As New JwtTokenService(store, "other-issuer", "audience", "v1", New String() {"v1"})
            ExpectFailure(Of SecurityTokenException)(Sub() badIssuer.Validate(token), "JWT emisor incorrecto")
            Dim parts = token.Split("."c)
            parts(2) = If(parts(2)(0) = "A"c, "B", "A") & parts(2).Substring(1)
            ExpectFailure(Of SecurityTokenException)(Sub() jwt.Validate(String.Join(".", parts)), "JWT firma alterada")
            Dim expired = MakeExpiredToken(store)
            ExpectFailure(Of SecurityTokenException)(Sub() jwt.Validate(expired), "JWT vencido")
            Dim rotated As New JwtTokenService(store, "issuer", "audience", "v2", New String() {"v1", "v2"})
            Check(rotated.Validate(token).Identity.Name = "subject", "Rotación conserva llave previa autorizada")
            Dim retired As New JwtTokenService(store, "issuer", "audience", "v2", New String() {"v2"})
            ExpectFailure(Of SecurityTokenException)(Sub() retired.Validate(token), "Rotación rechaza kid retirado")
            Dim current = DateTimeOffset.UtcNow
            Dim mutable As New MemoryStore()
            Dim cache As New CachedSecretStore(mutable, TimeSpan.FromSeconds(60), Function() current)
            Check(cache.GetSecret("x").Value = "first", "Carga de cache")
            mutable.Value = "second"
            Check(cache.GetSecret("x").Value = "first", "Cache dentro de TTL")
            current = current.AddSeconds(61)
            Check(cache.GetSecret("x").Value = "second", "Renovación después de TTL")
            mutable.Fail = True
            current = current.AddSeconds(61)
            ExpectFailure(Of IOException)(Sub() cache.GetSecret("x"), "Sin valor vencido al fallar el origen")
            Using connection = DatabaseConnectionFactory.CreateIntegrated("Server=localhost;Database=Example;Integrated Security=true")
                Dim builder As New System.Data.SqlClient.SqlConnectionStringBuilder(connection.ConnectionString)
                Check(builder.Encrypt AndAlso Not builder.TrustServerCertificate AndAlso Not builder.PersistSecurityInfo, "SQL: TLS y validación de certificado")
            End Using
            ExpectFailure(Of InvalidOperationException)(Sub() DatabaseConnectionFactory.CreateIntegrated("Server=localhost;Database=Example;User ID=user;Password=pass"), "Modo integrado rechaza credenciales SQL")
            Console.WriteLine(_count.ToString() & " pruebas correctas.")
        Finally
            If System.IO.Directory.Exists(directory) Then System.IO.Directory.Delete(directory, True)
        End Try
    End Sub
    Private Function MakeExpiredToken(store As ISecretStore) As String
        Dim key As New SymmetricSecurityKey(ReadKey(store, "jwt-signing-v1")) With {.KeyId = "v1"}
        Dim token As New JwtSecurityToken("issuer", "audience", Nothing, DateTime.UtcNow.AddMinutes(-10), DateTime.UtcNow.AddMinutes(-5), New SigningCredentials(key, SecurityAlgorithms.HmacSha256))
        Return New JwtSecurityTokenHandler().WriteToken(token)
    End Function
    Private Sub Check(condition As Boolean, label As String)
        If Not condition Then Throw New InvalidOperationException("Prueba fallida: " & label)
        _count += 1
        Console.WriteLine("OK " & label)
    End Sub
    Private Sub ExpectFailure(Of T As Exception)(action As Action, label As String)
        Try
            action()
        Catch ex As Exception
            If TypeOf ex Is T Then
                Check(True, label)
                Return
            End If
            Throw
        End Try
        Throw New InvalidOperationException("Se esperaba rechazo: " & label)
    End Sub
    Private Class MemoryStore
        Implements ISecretStore
        Public Property Value As String = "first"
        Public Property Fail As Boolean
        Public Function GetSecret(name As String) As SecretValue Implements ISecretStore.GetSecret
            If Fail Then Throw New IOException("Origen no disponible.")
            Return New SecretValue(Value, "test")
        End Function
    End Class
End Module
