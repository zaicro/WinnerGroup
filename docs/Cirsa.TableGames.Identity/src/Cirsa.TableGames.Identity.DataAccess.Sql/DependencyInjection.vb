Imports Cirsa.TableGames.Identity.DataAccess.Repositories
Imports Cirsa.TableGames.Identity.Domain.Repositories
Imports Microsoft.Extensions.DependencyInjection
Imports System.Data.Entity
Imports System.Runtime.CompilerServices

Public Module DependencyInjection

    ''' <summary>
    ''' Registers SQL Server identity persistence services.
    ''' </summary>
    <Extension()>
    Public Function AddIdentitySqlDataAccess(services As IServiceCollection, connectionString As String) As IServiceCollection

        If services Is Nothing Then _
            Throw New ArgumentNullException(NameOf(services))

        If String.IsNullOrWhiteSpace(connectionString) Then _
            Throw New ArgumentException("The connection string is required.", NameOf(connectionString))

        Database.SetInitializer(Of SqlDbContext)(Nothing)

        services.AddScoped(Of IdentityDbContext)(Function(serviceProvider) New SqlDbContext(connectionString))
        services.AddIdentityDataAccess()

        Return services
    End Function
End Module
