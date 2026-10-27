Imports System.Data.Entity
Imports System.Data.SqlClient
Imports TableGamesAuth.DataAccess.Ef6

Namespace TableGamesAuth.DataAccess.Sql
    Public Class SqlAuthDbContext
        Inherits AuthDbContext
        Shared Sub New()
            Database.SetInitializer(Of SqlAuthDbContext)(Nothing)
        End Sub
        Public Sub New()
            Me.New(Environment.GetEnvironmentVariable("TG_DB_CONNECTION"))
        End Sub
        Public Sub New(connectionString As String)
            MyBase.New(New SqlConnection(connectionString))
        End Sub
    End Class
End Namespace
