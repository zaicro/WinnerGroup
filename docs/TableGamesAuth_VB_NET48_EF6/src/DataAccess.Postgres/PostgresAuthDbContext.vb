Imports System.Data.Entity
Imports Npgsql
Imports TableGamesAuth.DataAccess.Ef6

Namespace TableGamesAuth.DataAccess.Postgres
    Public Class PostgresAuthDbContext
        Inherits AuthDbContext
        Shared Sub New()
            Database.SetInitializer(Of PostgresAuthDbContext)(Nothing)
        End Sub
        Public Sub New()
            Me.New(Environment.GetEnvironmentVariable("TG_DB_CONNECTION"))
        End Sub
        Public Sub New(connectionString As String)
            MyBase.New(New NpgsqlConnection(connectionString))
        End Sub
    End Class
End Namespace
