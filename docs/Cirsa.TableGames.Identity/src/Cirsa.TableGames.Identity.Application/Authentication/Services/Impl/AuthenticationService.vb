Imports Cirsa.TableGames.Identity.Application.Authentication.Contracts
Imports Cirsa.TableGames.Identity.Domain.Repositories

Namespace Authentication.Services.Impl

    ''' <summary>
    ''' Coordinates identity retrieval and password verification.
    ''' </summary>
    Public Class AuthenticationService
        Implements IAuthenticationService

        Private ReadOnly _userRepository As IUserRepository
        Private ReadOnly _passwordHasher As IPasswordHasher

        Public Sub New(userRepository As IUserRepository, passwordHasher As IPasswordHasher)
            If userRepository Is Nothing Then Throw New ArgumentNullException(NameOf(userRepository))
            If passwordHasher Is Nothing Then Throw New ArgumentNullException(NameOf(passwordHasher))
            _userRepository = userRepository
            _passwordHasher = passwordHasher
        End Sub

        Public Async Function AuthenticateAsync(request As AuthenticationRequest) As Task(Of AuthenticationResult) _
            Implements IAuthenticationService.AuthenticateAsync

            If request Is Nothing Then _
                Throw New ArgumentNullException(NameOf(request))

            If String.IsNullOrWhiteSpace(request.UserName) OrElse
                String.IsNullOrWhiteSpace(request.ApplicationCode) OrElse
                String.IsNullOrEmpty(request.Password) Then _
                Return Failed()

            Dim user = Await _userRepository.FindForApplicationAsync(request.UserName, request.ApplicationCode)

            If user Is Nothing Then _
                Return Failed()

            If Not _passwordHasher.Verify(request.Password, user.PasswordHash) Then _
                Return Failed()

            If user.UserRoles Is Nothing OrElse user.UserRoles.Count = 0 Then _
                Return Failed()

            Dim roleCodes = user.UserRoles _
                .Select(Function(assignment) assignment.Role.Code) _
                .Distinct() _
                .ToList()

            Return New AuthenticationResult With {
                .Succeeded = True,
                .user = New AuthenticatedUser With {
                    .UserId = user.UserId,
                    .UserName = user.UserName,
                    .DisplayName = user.DisplayName,
                    .Roles = roleCodes
                }
            }
        End Function

        Private Shared Function Failed() As AuthenticationResult
            Return New AuthenticationResult With {
                .Succeeded = False,
                .User = Nothing
            }
        End Function
    End Class
End Namespace