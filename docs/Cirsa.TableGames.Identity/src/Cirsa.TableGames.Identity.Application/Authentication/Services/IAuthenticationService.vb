Imports Cirsa.TableGames.Identity.Application.Authentication.Contracts

Namespace Authentication.Services

    ''' <summary>
    ''' Authenticates users for a specific application.
    ''' </summary>
    Public Interface IAuthenticationService
        Function AuthenticateAsync(request As AuthenticationRequest) As Task(Of AuthenticationResult)
    End Interface
End Namespace