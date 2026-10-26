Imports System.Data.Entity
Imports CrudPersonas.Domain

Public Class PersonasContext
    Inherits DbContext

    Public Sub New()
        MyBase.New("name=PersonasDb")
        Configuration.LazyLoadingEnabled = False
        Configuration.ProxyCreationEnabled = False
    End Sub

    Public Property Personas As DbSet(Of Persona)

    Protected Overrides Sub OnModelCreating(modelBuilder As DbModelBuilder)
        modelBuilder.Configurations.Add(New PersonaConfiguration())
        MyBase.OnModelCreating(modelBuilder)
    End Sub
End Class
