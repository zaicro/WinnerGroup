Imports Microsoft.AspNetCore.Cryptography.KeyDerivation
Imports System.Globalization
Imports System.Security.Cryptography

Namespace Authentication.Services.Impl

    ''' <summary>
    ''' Generates and verifies versioned PBKDF2-HMAC-SHA256 password hashes.
    ''' </summary>
    Public NotInheritable Class Pbkdf2PasswordHasher
        Implements IPasswordHasher

        Private Const FormatVersion As String = "v1"
        Private Const Algorithm As String = "pbkdf2-sha256"

        Private Const IterationCount As Integer = 600000
        Private Const SaltSize As Integer = 16
        Private Const HashSize As Integer = 32

        ' Prevents malformed stored values from requesting excessive work.
        Private Const MaxIterationCount As Integer = 2000000
        Private Const MaxEncodedLength As Integer = 256

        Public Function Hash(password As String) As String _
            Implements IPasswordHasher.Hash

            If String.IsNullOrEmpty(password) Then _
                Throw New ArgumentException("The password must not be null or empty.", NameOf(password))

            Dim salt(SaltSize - 1) As Byte

            Using random = RandomNumberGenerator.Create()
                random.GetBytes(salt)
            End Using

            Dim derivedHash = Derive(password, salt, IterationCount)

            Return String.Join(
                "$",
                New String() {
                    FormatVersion,
                    Algorithm,
                    IterationCount.ToString(
                        CultureInfo.InvariantCulture),
                    Convert.ToBase64String(salt),
                    Convert.ToBase64String(derivedHash)
                })
        End Function

        Public Function Verify(password As String, storedHash As String) As Boolean _
            Implements IPasswordHasher.Verify

            If String.IsNullOrEmpty(password) OrElse
               String.IsNullOrEmpty(storedHash) OrElse
               storedHash.Length > MaxEncodedLength Then _
               Return False

            Dim parts = storedHash.Split("$"c)

            If parts.Length <> 5 Then Return False

            If Not String.Equals(parts(0), FormatVersion, StringComparison.Ordinal) Then _
                Return False

            If Not String.Equals(parts(1), Algorithm, StringComparison.Ordinal) Then _
                Return False

            Dim iterations As Integer

            If Not Integer.TryParse(parts(2), NumberStyles.None, CultureInfo.InvariantCulture, iterations) Then _
                Return False

            If iterations < 1 OrElse iterations > MaxIterationCount Then _
                Return False

            Dim salt As Byte()
            Dim expectedHash As Byte()

            Try
                salt = Convert.FromBase64String(parts(3))
                expectedHash = Convert.FromBase64String(parts(4))
            Catch ex As FormatException
                Return False
            End Try

            If salt.Length <> SaltSize OrElse expectedHash.Length <> HashSize Then _
                Return False

            Dim actualHash = Derive(password, salt, iterations)

            Return CompareHashes(actualHash, expectedHash)
        End Function

        Private Shared Function Derive(password As String, salt As Byte(), iterations As Integer) As Byte()

            Return KeyDerivation.Pbkdf2(
                password:=password,
                salt:=salt,
                prf:=KeyDerivationPrf.HMACSHA256,
                iterationCount:=iterations,
                numBytesRequested:=HashSize)
        End Function

        Private Shared Function CompareHashes(actualHash As Byte(), expectedHash As Byte()) As Boolean

            ' Both arrays have the fixed HashSize length.
            ' Do not stop at the first differing byte.
            Dim difference As Integer = 0

            For index As Integer = 0 To HashSize - 1
                difference = difference Or
                    (CInt(actualHash(index)) Xor
                     CInt(expectedHash(index)))
            Next

            Return difference = 0
        End Function

    End Class
End Namespace