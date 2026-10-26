Imports System.Data.Entity
Imports System.Data.Entity.Migrations
Imports System.IO

Public NotInheritable Class DatabaseInitializer
    Private Sub New()
    End Sub

    Public Shared Function PrepararDirectorio() As String
        Dim carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                                   "CrudPersonas", "Datos")
        Directory.CreateDirectory(carpeta)
        AppDomain.CurrentDomain.SetData("DataDirectory", carpeta)
        Return carpeta
    End Function

    Public Shared Sub Actualizar()
        PrepararDirectorio()
        ' Evita inicializadores implícitos; solo las migraciones explícitas cambian el esquema.
        Database.SetInitializer(Of PersonasContext)(Nothing)
        Dim migrator As New DbMigrator(New Migrations.Configuration())
        migrator.Update()
    End Sub
End Class
