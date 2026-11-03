Imports System.Web.Http
Imports Microsoft.Extensions.DependencyInjection
Imports Cirsa.TableGames.Identity.Api.DependencyInjection
Imports Cirsa.TableGames.Identity.Application.DependencyInjection
Imports Cirsa.TableGames.Identity.DataAccess.Sql
Imports System.Globalization
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration
Imports Cirsa.WebApi2.Extensions.Authentication.Extensions
Imports Cirsa.WebApi2.Extensions.Swagger.Configuration
Imports Cirsa.WebApi2.Extensions.Swagger.Extensions

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


        Dim apiKeys = GetRequiredSetting("Security.ApiKeys")
        Dim keys As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        For Each entry In apiKeys.Split(";"c)
            Dim separator = entry.IndexOf(":"c)

            If separator <= 0 Then Throw New ConfigurationErrorsException("Security.ApiKeys must use the format name:key;name:key.")

            Dim name = entry.Substring(0, separator).Trim()
            Dim value = entry.Substring(separator + 1).Trim()

            If String.IsNullOrWhiteSpace(name) OrElse String.IsNullOrWhiteSpace(value) Then _
                Throw New ConfigurationErrorsException("Security.ApiKeys must contain nonempty names and keys.")

            If keys.ContainsKey(name) Then Throw New ConfigurationErrorsException("Duplicate API key name: " & name)

            keys.Add(name, value)
        Next

        Dim jwtIssuer = GetRequiredSetting("Security.JwtIssuer")
        Dim jwtAudience = GetRequiredSetting("Security.JwtAudience")
        Dim jwtSigningKey = GetRequiredSetting("Security.JwtSigningKey")
        Dim jwtLifetimeMinutes As Integer

        If Not Integer.TryParse(GetRequiredSetting("Security.JwtLifetimeMinutes"), NumberStyles.None, CultureInfo.InvariantCulture, jwtLifetimeMinutes) Then _
            Throw New ConfigurationErrorsException("Security.JwtLifetimeMinutes must be an integer.")

        services.AddApiAuthentication(New ApiAuthenticationOptions With {
            .EnableApiKey = True,
            .EnableJwt = True,
            .ApiKeys = keys,
            .Issuer = jwtIssuer,
            .Audience = jwtAudience,
            .SigningKeyBase64 = jwtSigningKey
        })
        services.AddJwtTokenIssuer(New JwtTokenIssuerOptions With {
            .Issuer = jwtIssuer,
            .Audience = jwtAudience,
            .SigningKeyBase64 = jwtSigningKey,
            .LifetimeMinutes = jwtLifetimeMinutes
        })

        Dim provider = services.BuildServiceProvider(
            New ServiceProviderOptions With {
                .ValidateScopes = True,
                .ValidateOnBuild = True
            })

        config.DependencyResolver = New DependencyInjectionResolver(provider)
        config.UseApiAuthentication(provider)
        config.UseSwaggerDocumentation(New SwaggerOptions With {
            .Title = "Identity API",
            .Version = "v1",
            .IncludeAuthenticationSchemes = True
        })
    End Sub


    Private Function ReadApiKeys() As Dictionary(Of String, String)

        Dim configuredValue = GetRequiredSetting("Security.ApiKeys")

        Dim apiKeys As New Dictionary(Of String, String)(
            StringComparer.OrdinalIgnoreCase)

        For Each entry In configuredValue.Split(";"c)
            Dim separator = entry.IndexOf(":"c)

            If separator <= 0 Then
                Throw New ConfigurationErrorsException(
                    "Each API key entry must use the format name:key.")
            End If

            Dim name = entry.Substring(0, separator).Trim()
            Dim key = entry.Substring(separator + 1).Trim()

            If String.IsNullOrWhiteSpace(name) OrElse
               String.IsNullOrWhiteSpace(key) Then

                Throw New ConfigurationErrorsException(
                    "API key names and values must not be empty.")
            End If

            If apiKeys.ContainsKey(name) Then
                Throw New ConfigurationErrorsException(
                    "Duplicate API key name: " & name)
            End If

            apiKeys.Add(name, key)
        Next

        Return apiKeys
    End Function

    Private Function GetRequiredSetting(key As String) As String
        Dim value = ConfigurationManager.AppSettings(key)

        If String.IsNullOrWhiteSpace(value) Then _
            Throw New ConfigurationErrorsException("The required configuration setting is missing or empty: " & key)

        Return value
    End Function

End Module