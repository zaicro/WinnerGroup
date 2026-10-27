Imports System.Data.Entity
Imports System.IO
Imports TableGamesAuth.Logging
Imports System.Linq
Imports System.Data.Entity.SqlServer
Imports System.Security.Cryptography
Imports System.Threading
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.Owin.Hosting
Imports Owin
Imports System.Web.Http
Imports System.Web.Http.ExceptionHandling
Imports Newtonsoft.Json.Serialization
Imports TableGamesAuth.Application
Imports TableGamesAuth.DataAccess.Ef6
Imports TableGamesAuth.Security
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.Api
    Public Module Program
        Public Sub Main(args As String())
            Try
                Log4NetLogger.Configure(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log4net.config"))
                If args.Length = 1 AndAlso args(0) = "--generate-secrets" Then
                    PrintSecretTemplate()
                    Return
                End If
                Dim settings As Settings = TableGamesAuth.Api.Settings.Load()
                DbConfiguration.SetConfiguration(New ProvidersConfiguration())
                Using services = Composition.Build(settings)
                    If args.Length = 1 AndAlso args(0) = "--migrate" Then
                        Dim migrations As System.Data.Entity.Migrations.DbMigrationsConfiguration
                        If settings.Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase) Then
                            migrations = New TableGamesAuth.DataAccess.Sql.Migrations.Configuration()
                        Else
                            migrations = New TableGamesAuth.DataAccess.Postgres.Migrations.Configuration()
                        End If
                        Dim migrator As New System.Data.Entity.Migrations.DbMigrator(migrations)
                        migrator.Update()
                        Console.WriteLine("Migración y semilla aplicadas.")
                        Return
                    End If
                    If args.Length = 1 AndAlso args(0) = "--bootstrap-admin" Then
                        Bootstrap(services)
                        Return
                    End If
                    Using config = ApiConfiguration.Create(services, settings)
                        Using host = WebApp.Start(settings.ListenUrl, Sub(app) app.UseWebApi(config))
                            Console.WriteLine("API escuchando en " & settings.ListenUrl)
                            Console.WriteLine("Presione Enter para detener.")
                            Console.ReadLine()
                        End Using
                    End Using
                End Using
            Catch ex As Exception
                ' No connection strings, passwords or API keys in console error messages.
                Dim logger As New Log4NetLogger()
                logger.Error("startup_failed", ex)
                If TypeOf ex Is ConfigurationException Then
                    Console.Error.WriteLine(ex.Message)
                Else
                    Console.Error.WriteLine("Inicio fallido: " & ex.GetType().Name & ". Revise configuración, conexión y esquema.")
                End If
                Environment.ExitCode = 1
            End Try
        End Sub
        Private Sub PrintSecretTemplate()
            Dim bytes(31) As Byte
            Dim rawKey As String, jwtKey As String
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(bytes) : jwtKey = Convert.ToBase64String(bytes)
                rng.GetBytes(bytes) : rawKey = Convert.ToBase64String(bytes)
            End Using
            Dim hash As String
            Using sha = SHA256.Create()
                hash = BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawKey))).Replace("-", "").ToLowerInvariant()
            End Using
            Dim appId = SeedData.ManagementAppId.ToString()
            Console.WriteLine("Guarde estos secretos; no los suba a Git:")
            Console.WriteLine("TG_JWT_KEY=" & jwtKey)
            Console.WriteLine("Client X-Api-Key=" & rawKey)
            Console.WriteLine("TG_BOOTSTRAP_APP_ID=" & appId)
            Console.WriteLine("TG_API_KEYS=[{""appId"":""" & appId & """,""sha256"":""" & hash & """}]")
        End Sub
        Private Sub Bootstrap(services As ServiceProvider)
            Using scope = services.CreateScope()
                Dim uow = scope.ServiceProvider.GetRequiredService(Of IUnitOfWork)()
                Dim hasher = services.GetRequiredService(Of IPasswordHasher)()
                Dim appId = SeedData.ManagementAppId
                If Not services.GetRequiredService(Of Settings)().ApiKeys.Any(Function(k) k.AppId = appId) Then Throw New InvalidOperationException("La app de bootstrap necesita una API key.")
                Dim name = Settings.Required("TG_BOOTSTRAP_USER")
                CrudService.Validate(New UserInput With {.UserName = name})
                Dim password = Settings.Required("TG_BOOTSTRAP_PASSWORD")
                Using tx = uow.BeginTransaction()
                    Dim ct = CancellationToken.None
                    If uow.Users.ListAsync(0, 1, ct).GetAwaiter().GetResult().Count > 0 Then Throw New InvalidOperationException("Bootstrap solo se permite sin usuarios.")
                    Dim role = uow.Roles.ByNameAsync("SYSTEMADMIN", ct).GetAwaiter().GetResult()
                    If role Is Nothing Then
                        role = New Role With {.Name = "SystemAdmin", .NormalizedName = "SYSTEMADMIN"}
                        uow.Roles.Add(role)
                    End If
                    Dim app = uow.Apps.GetAsync(appId, ct).GetAwaiter().GetResult()
                    If app Is Nothing Then
                        app = New ClientApp With {.Id = appId, .Code = "AUTH-MANAGEMENT", .Name = "Administración"}
                        uow.Apps.Add(app)
                    End If
                    Dim user As New User With {.UserName = name.Trim(), .NormalizedUserName = CrudService.Normalize(name), .PasswordHash = hasher.Hash(password), .RoleId = role.Id}
                    uow.Users.Add(user)
                    uow.UserApps.Add(New UserApp With {.UserId = user.Id, .AppId = appId})
                    uow.SaveAsync(ct).GetAwaiter().GetResult()
                    tx.Commit()
                    Console.WriteLine("Administrador creado. Elimine TG_BOOTSTRAP_PASSWORD del entorno.")
                End Using
            End Using
        End Sub
    End Module
    Public Class ProvidersConfiguration
        Inherits DbConfiguration
        Public Sub New()
            SetProviderServices("System.Data.SqlClient", SqlProviderServices.Instance)
            SetProviderFactory("Npgsql", Npgsql.NpgsqlFactory.Instance)
            SetProviderServices("Npgsql", Npgsql.NpgsqlServices.Instance)
        End Sub
    End Class
End Namespace
