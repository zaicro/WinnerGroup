Imports CrudPersonas.Domain
Imports System.Collections.Generic
Imports System.Linq

Public Class PersonaService
    Private ReadOnly _repository As IPersonaRepository

    Public Sub New(repository As IPersonaRepository)
        If repository Is Nothing Then Throw New ArgumentNullException(NameOf(repository))
        _repository = repository
    End Sub

    Public Function Listar() As List(Of PersonaDto)
        Return _repository.Listar().Select(Function(p) New PersonaDto With {
            .Id = p.Id, .Nombre = p.Nombre
        }).ToList()
    End Function

    Public Function Crear(nombre As String) As Integer
        Return _repository.Agregar(New Persona(nombre))
    End Function

    Public Sub Editar(id As Integer, nombre As String)
        Dim persona = _repository.Obtener(id)
        If persona Is Nothing Then Throw New InvalidOperationException("La persona ya no existe. Actualiza la lista.")
        persona.CambiarNombre(nombre)
        _repository.Actualizar(persona)
    End Sub

    Public Sub Eliminar(id As Integer)
        If id <= 0 Then Throw New ArgumentException("Selecciona una persona.", NameOf(id))
        _repository.Eliminar(id)
    End Sub
End Class
