Imports CrudPersonas.Domain
Imports System.Collections.Generic

' El contrato pertenece a Application. Su implementación pertenece a DataAccess.
Public Interface IPersonaRepository
    Function Listar() As List(Of Persona)
    Function Obtener(id As Integer) As Persona
    Function Agregar(persona As Persona) As Integer
    Sub Actualizar(persona As Persona)
    Sub Eliminar(id As Integer)
End Interface
