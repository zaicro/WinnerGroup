Imports System.Configuration
Imports System.IO
Imports System.Text
Imports SensitiveData48.Core

Module Program
    Function Main(args As String()) As Integer
        Try
            Dim storeDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SensitiveData48", "Secrets")
            Dim store As New DpapiSecretStore(storeDirectory)
            Dim command = If(args.Length = 0, "help", args(0).ToLowerInvariant())
            Select Case command
                Case "init"
                    For Each name In New String() {"jwt-signing-v1", "hmac-v1", "api-key-fingerprint"}
                        If File.Exists(Path.Combine(storeDirectory, name & ".secret.xml")) Then
                            Throw New IOException("Ya hay secretos locales. No se sobrescriben durante init.")
                        End If
                    Next
                    Console.WriteLine("Introduzca una API key aleatoria propia de al menos 32 caracteres. No se muestra ni se conserva en claro.")
                    Dim fingerprint = ApiKeyFingerprint(ReadHidden())
                    store.PutSecret("jwt-signing-v1", NewRandomKeyBase64())
                    store.PutSecret("hmac-v1", NewRandomKeyBase64())
                    store.PutSecret("api-key-fingerprint", fingerprint)
                    Console.WriteLine("Secretos locales creados con DPAPI CurrentUser. No se imprimieron las llaves.")
                Case "set-secret"
                    If args.Length < 2 OrElse args.Length > 3 Then Throw New ArgumentException("Uso: set-secret nombre [replace]")
                    Dim replace = args.Length = 3 AndAlso args(2) = "replace"
                    If args.Length = 3 AndAlso Not replace Then Throw New ArgumentException("Tercer argumento permitido: replace")
                    Console.WriteLine("Introduzca el valor sensible (entrada oculta; no use argumentos de línea de comandos):")
                    store.PutSecret(args(1), ReadHidden(), replace)
                    Console.WriteLine("Secreto protegido guardado.")
                Case "demo"
                    Dim cached As New CachedSecretStore(store, TimeSpan.FromSeconds(60))
                    Dim jwt As New JwtTokenService(cached, Setting("JwtIssuer"), Setting("JwtAudience"), Setting("JwtCurrentKid"), Setting("JwtAcceptedKids").Split(","c))
                    Dim token = jwt.Issue("usuario-demo")
                    Console.WriteLine("JWT emitido y validado: " & (jwt.Validate(token).Identity.Name = "usuario-demo").ToString())
                    Dim hmac As New HmacService(cached)
                    Console.WriteLine("HMAC válido: " & hmac.Verify("mensaje-demo", hmac.Sign("mensaje-demo")).ToString())
                    Console.WriteLine("Introduzca la API key configurada (entrada oculta):")
                    Console.WriteLine("API key válida: " & VerifyApiKey(ReadHidden(), cached.GetSecret("api-key-fingerprint").Value).ToString())
                    Using connection = DatabaseConnectionFactory.CreateIntegrated(ConfigurationManager.ConnectionStrings("Primary").ConnectionString)
                        Console.WriteLine("Conexión integrada construida con TLS y validación de certificado. No se abrió conexión ni se imprimió la cadena.")
                    End Using
                Case "self-test"
                    SelfTests.Run()
                Case "help"
                    Console.WriteLine("Comandos: init | demo | self-test | set-secret nombre [replace]")
                    Console.WriteLine("Local: DPAPI CurrentUser. Los datos quedan fuera del proyecto, bajo LocalApplicationData.")
                Case Else
                    Throw New ArgumentException("Comando desconocido. Use help.")
            End Select
            Return 0
        Catch ex As Exception
            ' No serializar excepciones de SDK/SQL/cripto a una respuesta pública ni imprimir valores sensibles.
            Console.Error.WriteLine("Operación fallida. Tipo: " & ex.GetType().Name & ". Revise el depurador y la guía de diagnóstico.")
            Return 1
        End Try
    End Function
    Private Function Setting(name As String) As String
        Dim value = ConfigurationManager.AppSettings(name)
        If String.IsNullOrWhiteSpace(value) Then Throw New ConfigurationErrorsException("Falta una opción pública requerida.")
        Return value
    End Function
    Private Function ReadHidden() As String
        Dim value As New StringBuilder()
        While True
            Dim key = Console.ReadKey(True)
            If key.Key = ConsoleKey.Enter Then Exit While
            If key.Key = ConsoleKey.Backspace Then
                If value.Length > 0 Then value.Length -= 1
            ElseIf Not Char.IsControl(key.KeyChar) Then
                If value.Length >= 16384 Then Throw New ArgumentException("Entrada demasiado larga.")
                value.Append(key.KeyChar)
            End If
        End While
        Console.WriteLine()
        Return value.ToString()
    End Function
End Module
