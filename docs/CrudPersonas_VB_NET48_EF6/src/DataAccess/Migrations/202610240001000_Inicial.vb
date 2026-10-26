Imports System.Data.Entity.Migrations

Namespace Migrations
    Public Partial Class Inicial
        Inherits DbMigration

        Public Overrides Sub Up()
            CreateTable("dbo.Personas",
                Function(c) New With {
                    .Id = c.Int(nullable:=False, identity:=True),
                    .Nombre = c.String(nullable:=False, maxLength:=150)
                }).PrimaryKey(Function(t) t.Id)
        End Sub

        Public Overrides Sub Down()
            DropTable("dbo.Personas")
        End Sub
    End Class
End Namespace
