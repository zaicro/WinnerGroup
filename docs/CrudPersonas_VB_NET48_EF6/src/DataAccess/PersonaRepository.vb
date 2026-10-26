Imports System.Data.Entity
Imports System.Collections.Generic
Imports System.Linq
Imports CrudPersonas.Application
Imports CrudPersonas.Domain

Public Class PersonaRepository
    Implements IPersonaRepository

    Public Function Listar() As List(Of Persona) Implements IPersonaRepository.Listar
        Using context As New PersonasContext()
            Return context.Personas.AsNoTracking().OrderBy(Function(p) p.Nombre).ThenBy(Function(p) p.Id).ToList()
        End Using
    End Function

    Public Function Obtener(id As Integer) As Persona Implements IPersonaRepository.Obtener
        Using context As New PersonasContext()
            Return context.Personas.AsNoTracking().SingleOrDefault(Function(p) p.Id = id)
        End Using
    End Function

    Public Function Agregar(persona As Persona) As Integer Implements IPersonaRepository.Agregar
        Using context As New PersonasContext()
            context.Personas.Add(persona)
            context.SaveChanges()
            Return persona.Id
        End Using
    End Function

    Public Sub Actualizar(persona As Persona) Implements IPersonaRepository.Actualizar
        Using context As New PersonasContext()
            Dim existente = context.Personas.Find(persona.Id)
            If existente Is Nothing Then Throw New InvalidOperationException("La persona ya no existe.")
            existente.Nombre = persona.Nombre
            context.SaveChanges()
        End Using
    End Sub

    Public Sub Eliminar(id As Integer) Implements IPersonaRepository.Eliminar
        Using context As New PersonasContext()
            Dim existente = context.Personas.Find(id)
            If existente Is Nothing Then Throw New InvalidOperationException("La persona ya no existe.")
            context.Personas.Remove(existente)
            context.SaveChanges()
        End Using
    End Sub
End Class
