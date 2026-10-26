Imports System.Data.Entity.ModelConfiguration
Imports CrudPersonas.Domain

Public Class PersonaConfiguration
    Inherits EntityTypeConfiguration(Of Persona)

    Public Sub New()
        ToTable("Personas", "dbo")
        HasKey(Function(p) p.Id)
        [Property](Function(p) p.Nombre).IsRequired().HasMaxLength(Persona.LongitudMaximaNombre).IsUnicode()
    End Sub
End Class
