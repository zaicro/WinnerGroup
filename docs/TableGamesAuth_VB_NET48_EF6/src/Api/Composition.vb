Imports TableGamesAuth.Logging
Imports System.Linq
Imports System.Web.Http
Imports Microsoft.Extensions.DependencyInjection
Imports TableGamesAuth.Application
Imports TableGamesAuth.DataAccess.Ef6
Imports TableGamesAuth.DataAccess.Sql
Imports TableGamesAuth.DataAccess.Postgres
Imports TableGamesAuth.Security

Namespace TableGamesAuth.Api
    Public Module Composition
        Public Function Build(settings As Settings) As ServiceProvider
            Dim services As New ServiceCollection()
            services.AddSingleton(settings)
            services.AddSingleton(Of IAppLogger, Log4NetLogger)()
            services.AddSingleton(Of IPasswordHasher, Pbkdf2PasswordHasher)()
            services.AddSingleton(Of ITokenService)(New JwtTokenService(settings.JwtKey, settings.Issuer, settings.Audience))
            Select Case settings.Provider.ToLowerInvariant()
                Case "sqlserver"
                    services.AddScoped(Of IUnitOfWork)(Function(sp) New EfUnitOfWork(New SqlAuthDbContext(settings.ConnectionString), sp.GetRequiredService(Of IAppLogger)()))
                Case "postgres"
                    services.AddScoped(Of IUnitOfWork)(Function(sp) New EfUnitOfWork(New PostgresAuthDbContext(settings.ConnectionString), sp.GetRequiredService(Of IAppLogger)()))
                Case Else
                    Throw New InvalidOperationException("TG_DB_PROVIDER debe ser SqlServer o Postgres.")
            End Select
            Dim dummy = New Pbkdf2PasswordHasher().Hash(Guid.NewGuid().ToString())
            services.AddScoped(Of AuthService)(Function(sp) New AuthService(sp.GetRequiredService(Of IUnitOfWork)(), sp.GetRequiredService(Of IPasswordHasher)(), sp.GetRequiredService(Of ITokenService)(), dummy, sp.GetRequiredService(Of IAppLogger)()))
            services.AddScoped(Of CrudService)()
            For Each controller In GetType(Composition).Assembly.GetTypes().Where(Function(t) Not t.IsAbstract AndAlso GetType(ApiController).IsAssignableFrom(t))
                services.AddTransient(controller)
            Next
            Return services.BuildServiceProvider(New ServiceProviderOptions With {.ValidateScopes = True, .ValidateOnBuild = True})
        End Function
    End Module
End Namespace
