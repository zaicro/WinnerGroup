Imports System.Security.Cryptography
Imports System.Text

Namespace Security
    Public Module SecretComparer
        Public Function EqualsSecret(left As String, right As String) As Boolean
            Using sha = SHA256.Create()
                Dim a = sha.ComputeHash(Encoding.UTF8.GetBytes(left))
                Dim b = sha.ComputeHash(Encoding.UTF8.GetBytes(right))
                Dim difference As Integer = 0
                For i As Integer = 0 To a.Length - 1
                    difference = difference Or (CInt(a(i)) Xor CInt(b(i)))
                Next
                Return difference = 0
            End Using
        End Function
    End Module
End Namespace
