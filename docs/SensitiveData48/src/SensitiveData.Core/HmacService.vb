Imports System.Security.Cryptography
Imports System.Text

' MAC = integridad/autenticidad, no cifrado ni almacenamiento de contraseñas.
Public NotInheritable Class HmacService
    Private ReadOnly _store As ISecretStore
    Public Sub New(store As ISecretStore)
        _store = store
    End Sub
    Public Function Sign(message As String) As String
        Dim key = ReadKey(_store, "hmac-v1")
        Try
            Using algorithm As New HMACSHA256(key)
                Return Convert.ToBase64String(algorithm.ComputeHash(Encoding.UTF8.GetBytes(message)))
            End Using
        Finally
            Array.Clear(key, 0, key.Length)
        End Try
    End Function
    Public Function Verify(message As String, tag As String) As Boolean
        Dim received As Byte()
        Try
            received = Convert.FromBase64String(tag)
        Catch ex As FormatException
            Return False
        End Try
        Return SameBytes(Convert.FromBase64String(Sign(message)), received)
    End Function
End Class
