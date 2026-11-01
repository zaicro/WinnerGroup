Imports Cirsa.TableGames.Identity.Domain.Entities

Namespace Repositories
    Public Interface IUserRepository
        Function FindForApplicationAsync(userName As String, applicationCode As String) As Task(Of User)
    End Interface
End Namespace