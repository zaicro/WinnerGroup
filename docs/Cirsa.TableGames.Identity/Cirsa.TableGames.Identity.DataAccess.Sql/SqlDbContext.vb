Imports System.Data.Entity

''' <summary>
''' Configures identity persistence for SQL Server.
''' </summary>
Public Class SqlDbContext
    Inherits IdentityDbContext

    Public Sub New()
        MyBase.New()
    End Sub

    Public Sub New(connectionString As String)
        MyBase.New(connectionString)
    End Sub

End Class