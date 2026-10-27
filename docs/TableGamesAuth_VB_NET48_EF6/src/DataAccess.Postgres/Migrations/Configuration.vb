Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Migrations
Imports TableGamesAuth.DataAccess.Ef6

Namespace TableGamesAuth.DataAccess.Postgres.Migrations
    Public Class Configuration
        Inherits DbMigrationsConfiguration(Of PostgresAuthDbContext)
        Public Sub New()
            AutomaticMigrationsEnabled = False
            AutomaticMigrationDataLossAllowed = False
            ContextKey = "TableGamesAuth.Postgres"
            MigrationsNamespace = "TableGamesAuth.DataAccess.Postgres.Migrations"
            TargetDatabase = New DbConnectionInfo(Environment.GetEnvironmentVariable("TG_DB_CONNECTION"), "Npgsql")
            SetSqlGenerator("Npgsql", New Npgsql.NpgsqlMigrationSqlGenerator())
        End Sub
        Protected Overrides Sub Seed(context As PostgresAuthDbContext)
            SeedData.Apply(context)
        End Sub
    End Class
End Namespace
