Imports System.Data.Entity.Migrations
Imports Cirsa.TableGames.Identity.DataAccess.Seeding

Namespace Migrations.Identity

    Friend NotInheritable Class Configuration
        Inherits DbMigrationsConfiguration(Of SqlDbContext)

        Public Sub New()
            AutomaticMigrationsEnabled = False
            MigrationsDirectory = "Migrations\Identity"
            ContextKey = "Identity"
        End Sub

        Protected Overrides Sub Seed(context As SqlDbContext)
            TbAppSeeder.Seed(context)
            TbUserSeeder.Seed(context)
            TbRoleSeeder.Seed(context)
            TbUserRoleSeeder.Seed(context)
        End Sub

    End Class

End Namespace
