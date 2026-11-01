Namespace Authentication.Services

    ''' <summary>
    ''' Generates and verifies encoded password hashes.
    ''' </summary>
    Public Interface IPasswordHasher
        Function Hash(password As String) As String
        Function Verify(password As String, storedHash As String) As Boolean
    End Interface
End Namespace
