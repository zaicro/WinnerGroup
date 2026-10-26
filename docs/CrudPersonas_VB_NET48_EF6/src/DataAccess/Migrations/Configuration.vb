Imports System.Data.Entity.Migrations

Namespace Migrations
    Public NotInheritable Class Configuration
        Inherits DbMigrationsConfiguration(Of PersonasContext)

        Public Sub New()
            AutomaticMigrationsEnabled = False
            AutomaticMigrationDataLossAllowed = False
            ContextKey = "CrudPersonas.DataAccess.PersonasContext"
            MigrationsDirectory = "Migrations"
        End Sub
    End Class
End Namespace
