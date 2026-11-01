Imports System.Runtime.CompilerServices
Imports Cirsa.TableGames.Identity.Application.Authentication.Services
Imports Cirsa.TableGames.Identity.Application.Authentication.Services.Impl
Imports Microsoft.Extensions.DependencyInjection

''' <summary>
''' Registers identity application services.
''' </summary>
Public Module DependencyInjection

    <Extension()>
    Public Function AddIdentityApplication(services As IServiceCollection) As IServiceCollection

        If services Is Nothing Then
            Throw New ArgumentNullException(NameOf(services))
        End If

        services.AddSingleton(Of IPasswordHasher, Pbkdf2PasswordHasher)()
        services.AddScoped(Of IAuthenticationService, AuthenticationService)()

        Return services
    End Function

End Module
