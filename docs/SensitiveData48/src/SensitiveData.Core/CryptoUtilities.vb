Imports System.Security.Cryptography
Imports System.Text

Public Module CryptoUtilities
    Public Function NewRandomKeyBase64() As String
        Dim value(31) As Byte
        Using rng = RandomNumberGenerator.Create()
            rng.GetBytes(value)
        End Using
        Return Convert.ToBase64String(value)
    End Function
    Public Function SameBytes(a As Byte(), b As Byte()) As Boolean
        If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then Return False
        Dim difference As Integer = 0
        For i As Integer = 0 To a.Length - 1
            difference = difference Or (CInt(a(i)) Xor CInt(b(i)))
        Next
        Return difference = 0
    End Function
    ' Solo para API keys aleatorias de alta entropía. NO para contraseñas humanas.
    Public Function ApiKeyFingerprint(apiKey As String) As String
        If apiKey Is Nothing OrElse apiKey.Length < 32 OrElse apiKey.Length > 256 Then
            Throw New ArgumentException("API key: entre 32 y 256 caracteres, generados aleatoriamente.")
        End If
        Using sha = SHA256.Create()
            Return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(apiKey)))
        End Using
    End Function
    Public Function VerifyApiKey(candidate As String, fingerprint As String) As Boolean
        If String.IsNullOrEmpty(candidate) OrElse candidate.Length < 32 OrElse candidate.Length > 256 Then Return False
        Dim expected = Convert.FromBase64String(fingerprint)
        If expected.Length <> 32 Then Throw New InvalidOperationException("Fingerprint de API key mal configurado.")
        Return SameBytes(Convert.FromBase64String(ApiKeyFingerprint(candidate)), expected)
    End Function
    Public Function ReadKey(store As ISecretStore, name As String) As Byte()
        Dim value = Convert.FromBase64String(store.GetSecret(name).Value)
        If value.Length <> 32 Then Throw New InvalidOperationException("Se requiere una llave aleatoria de exactamente 32 bytes.")
        Return value
    End Function
End Module
