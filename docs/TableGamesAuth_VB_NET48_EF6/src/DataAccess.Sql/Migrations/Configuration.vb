Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Migrations
Imports TableGamesAuth.DataAccess.Ef6

Namespace TableGamesAuth.DataAccess.Sql.Migrations
    Public Class Configuration
        Inherits DbMigrationsConfiguration(Of SqlAuthDbContext)
        Public Sub New()
            AutomaticMigrationsEnabled = False
            AutomaticMigrationDataLossAllowed = False
            ContextKey = "TableGamesAuth.Sql"
            MigrationsNamespace = "TableGamesAuth.DataAccess.Sql.Migrations"
            TargetDatabase = New DbConnectionInfo(Environment.GetEnvironmentVariable("TG_DB_CONNECTION"), "System.Data.SqlClient")
        End Sub
        Protected Overrides Sub Seed(context As SqlAuthDbContext)
            SeedData.Apply(context)
        End Sub
    End Class
End Namespace
