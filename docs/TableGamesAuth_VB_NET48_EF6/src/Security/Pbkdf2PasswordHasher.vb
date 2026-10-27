Imports System.Globalization
Imports System.Security.Cryptography
Imports TableGamesAuth.Application

Namespace TableGamesAuth.Security
    Public NotInheritable Class Pbkdf2PasswordHasher
        Implements IPasswordHasher
        Public Const Iterations As Integer = 600000
        Public Function Hash(password As String) As String Implements IPasswordHasher.Hash
            If password Is Nothing OrElse password.Length < 12 OrElse password.Length > 128 Then Throw New AppException(400, "La contraseña debe tener entre 12 y 128 caracteres.")
            Dim salt(15) As Byte
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(salt)
            End Using
            Using kdf As New Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256)
                Return String.Join("$", "pbkdf2-sha256", Iterations.ToString(CultureInfo.InvariantCulture), Convert.ToBase64String(salt), Convert.ToBase64String(kdf.GetBytes(32)))
            End Using
        End Function
        Private Shared Function Parse(encoded As String, ByRef count As Integer, ByRef salt As Byte(), ByRef hash As Byte()) As Boolean
            If String.IsNullOrEmpty(encoded) OrElse encoded.Length > 512 Then Return False
            Dim parts = encoded.Split("$"c)
            If parts.Length <> 4 OrElse parts(0) <> "pbkdf2-sha256" OrElse Not Integer.TryParse(parts(1), NumberStyles.None, CultureInfo.InvariantCulture, count) OrElse count < Iterations OrElse count > 2000000 Then Return False
            Try
                salt = Convert.FromBase64String(parts(2))
                hash = Convert.FromBase64String(parts(3))
                Return salt.Length = 16 AndAlso hash.Length = 32
            Catch ex As FormatException
                Return False
            End Try
        End Function
        Public Function IsSupportedHash(encoded As String) As Boolean Implements IPasswordHasher.IsSupportedHash
            Dim count As Integer, salt As Byte() = Nothing, hash As Byte() = Nothing
            Return Parse(encoded, count, salt, hash)
        End Function
        Public Function NeedsRehash(encoded As String) As Boolean Implements IPasswordHasher.NeedsRehash
            Dim count As Integer, salt As Byte() = Nothing, hash As Byte() = Nothing
            Return Not Parse(encoded, count, salt, hash) OrElse count < Iterations
        End Function
        Public Function Verify(password As String, encoded As String) As Boolean Implements IPasswordHasher.Verify
            If password Is Nothing OrElse password.Length > 128 Then Return False
            Dim count As Integer, salt As Byte() = Nothing, hash As Byte() = Nothing
            If Not Parse(encoded, count, salt, hash) Then Return False
            Using kdf As New Rfc2898DeriveBytes(password, salt, count, HashAlgorithmName.SHA256)
                Return SecureEquals(hash, kdf.GetBytes(32))
            End Using
        End Function
        Public Shared Function SecureEquals(a As Byte(), b As Byte()) As Boolean
            If a Is Nothing OrElse b Is Nothing OrElse a.Length <> b.Length Then Return False
            Dim diff As Integer
            For i = 0 To a.Length - 1
                diff = diff Or (CInt(a(i)) Xor CInt(b(i)))
            Next
            Return diff = 0
        End Function
    End Class
End Namespace
