Imports System.Web.Http
Imports Microsoft.Extensions.DependencyInjection
Imports Cirsa.TableGames.Identity.Api.DependencyInjection
Imports Cirsa.TableGames.Identity.Application.DependencyInjection
Imports Cirsa.TableGames.Identity.DataAccess.Sql

Public Module DependencyInjectionConfig

    Public Sub Register(config As HttpConfiguration)
        If config Is Nothing Then Throw New ArgumentNullException(NameOf(config))

        Dim connectionSettings = ConfigurationManager.ConnectionStrings("IdentityDb")

        If connectionSettings Is Nothing OrElse String.IsNullOrWhiteSpace(connectionSettings.ConnectionString) Then _
            Throw New ConfigurationErrorsException("The IdentityDb connection string is required.")

        Dim connectionString = connectionSettings.ConnectionString
        Dim services As New ServiceCollection()

        services.AddIdentityApplication()
        services.AddIdentitySqlDataAccess(connectionString)

        Dim controllerTypes = GetType(DependencyInjectionConfig) _
            .Assembly _
            .GetTypes() _
            .Where(
            Function(t) t.IsClass AndAlso
                t.IsVisible AndAlso
                Not t.IsAbstract AndAlso
                Not t.ContainsGenericParameters AndAlso
                GetType(ApiController).IsAssignableFrom(t)
            )

        For Each controller In controllerTypes
            services.AddTransient(controller)
        Next

        Dim provider = services.BuildServiceProvider(
            New ServiceProviderOptions With {
                .ValidateScopes = True,
                .ValidateOnBuild = True
            })

        config.DependencyResolver = New DependencyInjectionResolver(provider)
    End Sub

End Module