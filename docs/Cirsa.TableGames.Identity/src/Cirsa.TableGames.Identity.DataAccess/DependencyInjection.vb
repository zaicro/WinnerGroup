Imports Cirsa.TableGames.Identity.DataAccess.Repositories
Imports Cirsa.TableGames.Identity.Domain.Repositories
Imports Microsoft.Extensions.DependencyInjection
Imports System.Runtime.CompilerServices

Public Module DependencyInjection

    ''' <summary>
    ''' Registers identity repositories in the service collection.
    ''' </summary>
    ''' <param name="services">The service collection to configure.</param>
    ''' <returns>The service collection for chained registrations.</returns>
    <Extension()>
    Public Function AddIdentityDataAccess(services As IServiceCollection) As IServiceCollection

        If services Is Nothing Then
            Throw New ArgumentNullException(NameOf(services))
        End If

        services.AddScoped(Of IUserRepository, UserRepository)()

        Return services
    End Function
End Module
