Imports System
Imports System.Collections.Generic
Imports System.IdentityModel.Tokens.Jwt
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Reflection
Imports System.Security.Claims
Imports System.Threading
Imports System.Web.Http
Imports System.Web.Http.Controllers
Imports System.Web.Http.Description
Imports System.Web.Http.Dispatcher
Imports Cirsa.WebApi2.Extensions.Swagger.Extensions
Imports Newtonsoft.Json.Linq
Imports System.Web.Http.Filters
Imports Cirsa.WebApi2.Extensions.Authentication.Attributes
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration
Imports Cirsa.WebApi2.Extensions.Authentication.Extensions
Imports Cirsa.WebApi2.Extensions.Authentication.Filters
Imports Cirsa.WebApi2.Extensions.Authentication.Policies
Imports Cirsa.WebApi2.Extensions.Authentication.Tokens
Imports Cirsa.WebApi2.Extensions.Authentication.Validation
Imports Cirsa.WebApi2.Extensions.Swagger.Configuration
Imports Cirsa.WebApi2.Extensions.Swagger.Filters
Imports Microsoft.Extensions.DependencyInjection
Imports Microsoft.IdentityModel.Tokens
Imports Swashbuckle.Swagger

Module Program
    Private Passed As Integer
    Private Failed As Integer
    ' Test fixtures only: never use these credentials in a deployed API.
    Private ReadOnly Key As String = New String("k"c, 40)
    Private ReadOnly SigningKey As String = Convert.ToBase64String(Enumerable.Range(1, 32).Select(Function(n) CByte(n)).ToArray())
    Private ReadOnly Options As New ApiAuthenticationOptions With {
        .ApiKeys = New Dictionary(Of String, String) From {{"baccarat", Key}},
        .Issuer = "test-issuer", .Audience = "test-audience", .SigningKeyBase64 = SigningKey}

    Public Function Main() As Integer
        Run("policy precedence, aliases and malformed schemes", AddressOf Policies)
        Run("key validation, duplicate configuration and immutable snapshot", AddressOf ApiKeys)
        Run("JWT roundtrip, reserved claims and invalid token rejection", AddressOf Tokens)
        Run("single-scheme configuration and disabled-policy rejection", AddressOf IndependentSchemes)
        Run("authentication matrix: missing, valid and invalid credentials", AddressOf Matrix)
        Run("JWT identity plus named API key identity", AddressOf MergedIdentity)
        Run("401 challenges: correct schemes and no duplicates", AddressOf Challenges)
        Run("Swagger requirements match runtime policies", AddressOf SwaggerPolicies)
        Run("DI registration, snapshots and duplicate guards", AddressOf Registration)
        Run("startup policy validation rejects invalid configuration", AddressOf InvalidStartupPolicy)
        Run("Swagger HTTP document, UI and embedded JavaScript", AddressOf SwaggerHttp)
        Run("embedded Swagger authorization asset", AddressOf Resource)
        Console.WriteLine($"RESULT: {Passed} passed; {Failed} failed.")
        Return If(Failed = 0, 0, 1)
    End Function

    Private Sub Run(name As String, action As Action)
        Try
            action()
            Passed += 1
            Console.WriteLine("PASS " & name)
        Catch ex As Exception
            Failed += 1
            Console.WriteLine("FAIL " & name & Environment.NewLine & ex.ToString())
        End Try
    End Sub

    Private Sub Check(condition As Boolean, Optional message As String = "Assertion failed.")
        If Not condition Then Throw New Exception(message)
    End Sub

    Private Sub Throws(Of T As Exception)(action As Action)
        Try
            action()
        Catch ex As T
            Return
        End Try
        Throw New Exception("Expected " & GetType(T).Name)
    End Sub

    Private Function Descriptor(name As String, Optional controllerType As Type = Nothing, Optional config As HttpConfiguration = Nothing) As ReflectedHttpActionDescriptor
        If controllerType Is Nothing Then controllerType = GetType(PolicyController)
        If config Is Nothing Then config = New HttpConfiguration()
        Dim descriptorValue As New HttpControllerDescriptor(config, controllerType.Name, controllerType)
        Return New ReflectedHttpActionDescriptor(descriptorValue, controllerType.GetMethod(name))
    End Function

    Private Sub Policies()
        Check(AuthenticationPolicyResolver.Resolve(Descriptor("InheritedPolicy")) = AuthenticationMode.ApiKey)
        Check(AuthenticationPolicyResolver.Resolve(Descriptor("Jwt")) = AuthenticationMode.Jwt)
        Check(AuthenticationPolicyResolver.Resolve(Descriptor("Both")) = AuthenticationMode.Both)
        Check(AuthenticationPolicyResolver.Resolve(Descriptor("Either")) = AuthenticationMode.Either)
        Check(AuthenticationPolicyResolver.Resolve(Descriptor("PublicAction")) = AuthenticationMode.None)
        Check(AuthenticationPolicyResolver.Resolve(Descriptor("Get", GetType(AnonymousController))) = AuthenticationMode.None)
        Check(AuthenticationPolicyResolver.Resolve(Descriptor("Get", GetType(PublicController))) = AuthenticationMode.None)
        For Each value In New String() {Nothing, "", " ", "Basic", "ApiKey,", ",Bearer", "JWT"}
            Throws(Of InvalidOperationException)(Sub() AuthenticationPolicyResolver.Resolve(New ApiAuthorizeAttribute With {.AuthenticationSchemes = value}))
        Next
        Check(AuthenticationPolicyResolver.Resolve(New ApiAuthorizeAttribute With {.AuthenticationSchemes = " apikey , BEARER "}) = AuthenticationMode.Either)
    End Sub

    Private Sub ApiKeys()
        Dim validator As New AuthorizeValidator(Options)
        Dim name As String = "old"
        Check(validator.TryValidateApiKey(Key, name) AndAlso name = "baccarat")
        For Each bad In New String() {Nothing, "", "short", New String("x"c, 40), New String("x"c, 513), New String(ChrW(&HD800), 40)}
            name = "old"
            Check(Not validator.TryValidateApiKey(bad, name) AndAlso name Is Nothing)
        Next
        Dim dictionary As New Dictionary(Of String, String) From {{"one", Key}}
        Dim mutable As New ApiAuthenticationOptions With {.EnableJwt = False, .ApiKeys = dictionary}
        Dim snapshot As New AuthorizeValidator(mutable)
        dictionary("one") = New String("z"c, 40)
        Check(snapshot.TryValidateApiKey(Key, name) AndAlso name = "one")
        Throws(Of ArgumentException)(Sub() NewValidator(New Dictionary(Of String, String) From {{"one", Key}, {"two", Key}}))
        Throws(Of ArgumentException)(Sub() NewValidator(New Dictionary(Of String, String) From {{"one", Key}, {"ONE", New String("a"c, 40)}}))
    End Sub

    Private Sub NewValidator(keys As Dictionary(Of String, String))
        Dim value As New AuthorizeValidator(New ApiAuthenticationOptions With {.EnableJwt = False, .ApiKeys = keys})
    End Sub

    Private Function Issue() As IssuedJwtToken
        Return New JwtTokenIssuer(Options.Issuer, Options.Audience, SigningKey, 5).Create("user-123", New Claim() {New Claim("name", "admin"), New Claim("role", "manager")})
    End Function

    Private Sub Tokens()
        Dim validator As New AuthorizeValidator(Options)
        Dim principal As ClaimsPrincipal = Nothing
        Dim issued = Issue()
        Check(validator.TryValidateJwt(issued.AccessToken, principal))
        Check(principal.Identity.Name = "admin" AndAlso principal.IsInRole("manager"))
        Check(principal.FindFirst("sub").Value = "user-123")
        Check(issued.ExpiresAtUtc > DateTime.UtcNow)
        Dim issuer As New JwtTokenIssuer(Options.Issuer, Options.Audience, SigningKey, 5)
        For Each claimType In New String() {"sub", "iss", "aud", "exp", "nbf", "iat", "jti"}
            Throws(Of ArgumentException)(Sub() issuer.Create("subject", {New Claim(claimType, "override")}))
        Next
        Throws(Of ArgumentException)(Sub() issuer.Create(""))
        Throws(Of InvalidOperationException)(Sub() issuer.Create("subject", {New Claim("huge", New String("x"c, 9000))}))
        Dim badTokens As New List(Of String) From {Nothing, "", "invalid", New String("x"c, 8193)}
        badTokens.Add(New JwtTokenIssuer("wrong-issuer", Options.Audience, SigningKey, 5).Create("subject").AccessToken)
        badTokens.Add(New JwtTokenIssuer(Options.Issuer, "wrong-audience", SigningKey, 5).Create("subject").AccessToken)
        badTokens.Add(New JwtTokenIssuer(Options.Issuer, Options.Audience, Convert.ToBase64String(New Byte(31) {}), 5).Create("subject").AccessToken)
        Dim handler As New JwtSecurityTokenHandler()
        Dim signing As New SigningCredentials(New SymmetricSecurityKey(Convert.FromBase64String(SigningKey)), SecurityAlgorithms.HmacSha256)
        badTokens.Add(handler.WriteToken(New JwtSecurityToken(Options.Issuer, Options.Audience, Nothing, DateTime.UtcNow.AddHours(-2), DateTime.UtcNow.AddHours(-1), signing)))
        badTokens.Add(handler.WriteToken(New JwtSecurityToken(Options.Issuer, Options.Audience, Nothing, DateTime.UtcNow.AddHours(1), DateTime.UtcNow.AddHours(2), signing)))
        badTokens.Add(handler.WriteToken(New JwtSecurityToken(Options.Issuer, Options.Audience, Nothing, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), Nothing)))
        For Each token In badTokens
            Check(Not validator.TryValidateJwt(token, principal) AndAlso principal Is Nothing, "Unexpected accepted token.")
        Next
    End Sub

    Private Sub IndependentSchemes()
        Dim validator As New AuthorizeValidator(New ApiAuthenticationOptions With {.EnableJwt = False, .ApiKeys = Options.ApiKeys})
        validator.EnsureModeConfigured(AuthenticationMode.ApiKey)
        Throws(Of InvalidOperationException)(Sub() validator.EnsureModeConfigured(AuthenticationMode.Jwt))
        Throws(Of InvalidOperationException)(Sub() validator.EnsureModeConfigured(AuthenticationMode.Either))
        validator = New AuthorizeValidator(New ApiAuthenticationOptions With {.EnableApiKey = False, .Issuer = Options.Issuer, .Audience = Options.Audience, .SigningKeyBase64 = SigningKey})
        validator.EnsureModeConfigured(AuthenticationMode.Jwt)
        Throws(Of InvalidOperationException)(Sub() validator.EnsureModeConfigured(AuthenticationMode.ApiKey))
        Throws(Of ArgumentException)(Sub() ConstructDisabled())
    End Sub

    Private Sub ConstructDisabled()
        Dim validator As New AuthorizeValidator(New ApiAuthenticationOptions With {.EnableApiKey = False, .EnableJwt = False})
    End Sub

    Private Function Authenticate(action As String, apiKey As String, jwt As String, Optional duplicate As Boolean = False) As HttpAuthenticationContext
        Dim request As New HttpRequestMessage(HttpMethod.Get, "http://localhost/test")
        If apiKey IsNot Nothing Then
            request.Headers.TryAddWithoutValidation("X-Api-Key", If(duplicate, New String() {apiKey, apiKey}, New String() {apiKey}))
        End If
        If jwt IsNot Nothing Then request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", jwt)
        Dim descriptorValue = Descriptor(action)
        request.SetConfiguration(descriptorValue.ControllerDescriptor.Configuration)
        Dim controllerContext As New HttpControllerContext With {.Request = request, .Configuration = descriptorValue.ControllerDescriptor.Configuration, .ControllerDescriptor = descriptorValue.ControllerDescriptor}
        Dim context As New HttpAuthenticationContext(New HttpActionContext(controllerContext, descriptorValue), Nothing)
        Dim filter As New AuthenticationFilter(New AuthorizeValidator(Options))
        filter.AuthenticateAsync(context, CancellationToken.None).GetAwaiter().GetResult()
        Return context
    End Function

    Private Sub Matrix()
        Dim token = Issue().AccessToken
        For Each action In New String() {"InheritedPolicy", "Jwt", "Both", "Either"}
            For keyState As Integer = 0 To 2
                For jwtState As Integer = 0 To 2
                    Dim validKey = keyState = 1
                    Dim validJwt = jwtState = 1
                    Dim expected = If(action = "InheritedPolicy", validKey, If(action = "Jwt", validJwt, If(action = "Both", validKey AndAlso validJwt, validKey OrElse validJwt)))
                    Dim context = Authenticate(action, If(keyState = 0, Nothing, If(validKey, Key, New String("b"c, 40))), If(jwtState = 0, Nothing, If(validJwt, token, "invalid")))
                    Check((context.ErrorResult Is Nothing) = expected, action & " " & keyState & "/" & jwtState)
                    If expected Then Check(context.Principal.Identity.IsAuthenticated)
                Next
            Next
        Next
        Check(Authenticate("InheritedPolicy", Key, Nothing, True).ErrorResult IsNot Nothing)
        Check(Authenticate("PublicAction", Nothing, Nothing).ErrorResult Is Nothing)
    End Sub

    Private Sub MergedIdentity()
        Dim principal = DirectCast(Authenticate("Both", Key, Issue().AccessToken).Principal, ClaimsPrincipal)
        Check(principal.Identities.Count() = 2)
        Check(principal.Identity.Name = "admin" AndAlso principal.IsInRole("manager"))
        Check(principal.Identities.Single(Function(i) i.AuthenticationType = "ApiKey").Name = "baccarat")
        Check(Not principal.Claims.Any(Function(c) c.Value = Key))
        Check(DirectCast(Authenticate("Either", Key, Issue().AccessToken).Principal, ClaimsPrincipal).Identities.Count() = 2)
    End Sub

    Private Sub Challenges()
        For Each action In New String() {"InheritedPolicy", "Jwt", "Both", "Either"}
            Dim context = Authenticate(action, Nothing, Nothing)
            Dim challenge As New HttpAuthenticationChallengeContext(context.ActionContext, context.ErrorResult)
            Dim filter As New AuthenticationFilter(New AuthorizeValidator(Options))
            filter.ChallengeAsync(challenge, CancellationToken.None).GetAwaiter().GetResult()
            Dim response = challenge.Result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult()
            Dim schemes = response.Headers.WwwAuthenticate.Select(Function(h) h.Scheme).ToArray()
            Check(response.StatusCode = HttpStatusCode.Unauthorized)
            Check(schemes.Length = If(action = "Both" OrElse action = "Either", 2, 1))
            Check(schemes.Distinct().Count() = schemes.Length)
            Dim actionResponse As New HttpResponseMessage(HttpStatusCode.Unauthorized)
            actionResponse.Headers.WwwAuthenticate.Add(New AuthenticationHeaderValue(If(action = "Jwt", "Bearer", "ApiKey")))
            Dim returned As New HttpAuthenticationChallengeContext(context.ActionContext, New System.Web.Http.Results.ResponseMessageResult(actionResponse))
            filter.ChallengeAsync(returned, CancellationToken.None).GetAwaiter().GetResult()
            Dim challenged = returned.Result.ExecuteAsync(CancellationToken.None).GetAwaiter().GetResult()
            Check(challenged.Headers.WwwAuthenticate.Count() = schemes.Length)
        Next
    End Sub

    Private Sub SwaggerPolicies()
        Dim filter As New SwaggerSecurityRequirementsFilter()
        For Each action In New String() {"InheritedPolicy", "Jwt", "Both", "Either", "PublicAction"}
            Dim operation As New Operation With {.responses = New Dictionary(Of String, Response) From {{"200", New Response With {.description = "OK"}}}}
            filter.Apply(operation, Nothing, New ApiDescription With {.ActionDescriptor = Descriptor(action)})
            Check(operation.parameters Is Nothing)
            Check(operation.responses.ContainsKey("200"))
            Check(operation.responses.ContainsKey("401") = (action <> "PublicAction"))
            Select Case action
                Case "Both"
                    Check(operation.security.Count = 1 AndAlso operation.security(0).Count = 2)
                Case "Either"
                    Check(operation.security.Count = 2 AndAlso operation.security.All(Function(r) r.Count = 1))
                Case "PublicAction"
                    Check(operation.security.Count = 0)
                Case Else
                    Check(operation.security.Count = 1 AndAlso operation.security(0).ContainsKey(If(action = "Jwt", "Bearer", "ApiKey")))
            End Select
        Next
    End Sub

    Private Sub Registration()
        Dim services As New ServiceCollection()
        services.AddApiAuthentication(Options)
        services.AddJwtTokenIssuer(New JwtTokenIssuerOptions With {.Issuer = Options.Issuer, .Audience = Options.Audience, .SigningKeyBase64 = SigningKey, .LifetimeMinutes = 5})
        Throws(Of InvalidOperationException)(Sub() services.AddApiAuthentication(Options))
        Using provider = services.BuildServiceProvider()
            Check(provider.GetRequiredService(Of AuthorizeValidator)() Is provider.GetRequiredService(Of AuthorizeValidator)())
            Check(provider.GetRequiredService(Of JwtTokenIssuer)().Create("subject").AccessToken.Length > 0)
            Using configuration As New HttpConfiguration()
                configuration.Services.Replace(GetType(IHttpControllerTypeResolver), New FixedControllerResolver(GetType(PolicyController), GetType(PublicController)))
                configuration.UseApiAuthentication(provider)
                Throws(Of InvalidOperationException)(Sub() configuration.UseApiAuthentication(provider))
                configuration.Routes.MapHttpRoute("default", "api/{controller}/{action}")
                configuration.EnsureInitialized()
            End Using
        End Using
    End Sub

    Private Sub InvalidStartupPolicy()
        Dim services As New ServiceCollection()
        services.AddApiAuthentication(Options)
        Using provider = services.BuildServiceProvider()
            Using config As New HttpConfiguration()
                config.Services.Replace(GetType(IHttpControllerTypeResolver), New FixedControllerResolver(GetType(InvalidPolicyController)))
                config.UseApiAuthentication(provider)
                config.Routes.MapHttpRoute("default", "api/{controller}/{action}")
                Throws(Of InvalidOperationException)(Sub() config.EnsureInitialized())
            End Using
        End Using
    End Sub

    Private Sub SwaggerHttp()
        Dim services As New ServiceCollection()
        services.AddApiAuthentication(Options)
        Using provider = services.BuildServiceProvider()
            Using config As New HttpConfiguration()
                config.Services.Replace(GetType(IHttpControllerTypeResolver), New FixedControllerResolver(GetType(DocumentationController)))
                config.UseApiAuthentication(provider)
                config.UseSwaggerDocumentation(New SwaggerOptions With {.Title = "Test API", .Version = "v1", .IncludeAuthenticationSchemes = True})
                Throws(Of InvalidOperationException)(Sub() config.UseSwaggerDocumentation(New SwaggerOptions()))
                config.MapHttpAttributeRoutes()
                config.EnsureInitialized()
                Using server As New HttpServer(config)
                    Using client As New HttpClient(server)
                        client.BaseAddress = New Uri("http://localhost/")
                        Dim response = client.GetAsync("swagger/docs/v1").GetAwaiter().GetResult()
                        Dim json = response.Content.ReadAsStringAsync().GetAwaiter().GetResult()
                        Check(response.IsSuccessStatusCode, json)
                        Dim doc = JObject.Parse(json)
                        Check(doc("securityDefinitions")("ApiKey")("name").ToString() = "X-Api-Key")
                        Check(doc("securityDefinitions")("Bearer")("name").ToString() = "Authorization")
                        Dim requirements = DirectCast(doc("paths")("/api/documentation/both")("get")("security"), JArray)
                        Check(requirements.Count = 1 AndAlso DirectCast(requirements(0), JObject).Properties().Count() = 2)
                        Dim publicRequirements = DirectCast(doc("paths")("/api/documentation/public")("get")("security"), JArray)
                        Check(publicRequirements.Count = 0)
                        Dim html = client.GetStringAsync("swagger/ui/index").GetAwaiter().GetResult()
                        Check(html.Contains("authorization"), "Embedded JS was not injected into Swagger UI.")
                        ' Swashbuckle embeds injected resource URLs in the customScripts JSON array.
                        Dim match = System.Text.RegularExpressions.Regex.Match(html, "ext/[^""'\s]*authorization[^""'\s]*")
                        Check(match.Success, "Embedded resource URL was not found.")
                        Dim script = client.GetStringAsync("swagger/ui/" & match.Value).GetAwaiter().GetResult()
                        Check(script.Contains("cirsa-authorize"))
                        Dim denied = client.GetAsync("api/documentation/both").GetAwaiter().GetResult()
                        Check(denied.StatusCode = HttpStatusCode.Unauthorized)
                        Check(denied.Headers.WwwAuthenticate.Count() = 2)
                        Dim request As New HttpRequestMessage(HttpMethod.Get, "api/documentation/both")
                        request.Headers.Add("X-Api-Key", Key)
                        request.Headers.Authorization = New AuthenticationHeaderValue("Bearer", Issue().AccessToken)
                        Dim allowed = client.SendAsync(request).GetAwaiter().GetResult()
                        Check(allowed.IsSuccessStatusCode, allowed.Content.ReadAsStringAsync().GetAwaiter().GetResult())
                    End Using
                End Using
            End Using
        End Using
    End Sub

    Private Sub Resource()
        Dim assemblyValue = GetType(AuthorizeValidator).Assembly
        Dim name = "Cirsa.WebApi2.Extensions.Swagger.Assets.authorization.js"
        Check(assemblyValue.GetManifestResourceNames().Contains(name))
        Using stream = assemblyValue.GetManifestResourceStream(name)
            Check(stream.Length > 1000)
        End Using
    End Sub
End Module

<ApiAuthorize(AuthenticationSchemes:="ApiKey")>
Public Class PolicyController
    Inherits ApiController
    Public Sub InheritedPolicy()
    End Sub
    <ApiAuthorize(AuthenticationSchemes:="Bearer")>
    Public Sub Jwt()
    End Sub
    <ApiAuthorize(AuthenticationSchemes:="ApiKey,Bearer", RequireAllSchemes:=True)>
    Public Sub Both()
    End Sub
    <ApiAuthorize(AuthenticationSchemes:="ApiKey,Bearer")>
    Public Sub Either()
    End Sub
    <AllowAnonymous>
    Public Sub PublicAction()
    End Sub
End Class

<AllowAnonymous>
Public Class AnonymousController
    Inherits ApiController
    <ApiAuthorize(AuthenticationSchemes:="Bearer")>
    Public Sub [Get]()
    End Sub
End Class

Public Class PublicController
    Inherits ApiController
    Public Sub [Get]()
    End Sub
End Class

Public Class FixedControllerResolver
    Implements IHttpControllerTypeResolver
    Private ReadOnly _types As Type()
    Public Sub New(ParamArray types As Type())
        _types = types
    End Sub
    Public Function GetControllerTypes(assembliesResolver As IAssembliesResolver) As ICollection(Of Type) Implements IHttpControllerTypeResolver.GetControllerTypes
        Return _types
    End Function
End Class

<ApiAuthorize(AuthenticationSchemes:="Basic")>
Public Class InvalidPolicyController
    Inherits ApiController
    Public Function [Get]() As IHttpActionResult
        Return Ok()
    End Function
End Class

<RoutePrefix("api/documentation")>
Public Class DocumentationController
    Inherits ApiController
    <HttpGet, Route("both"), ApiAuthorize(AuthenticationSchemes:="ApiKey,Bearer", RequireAllSchemes:=True)>
    Public Function GetBoth() As IHttpActionResult
        Return Ok("authenticated")
    End Function
    <HttpGet, Route("public"), AllowAnonymous>
    Public Function GetPublic() As IHttpActionResult
        Return Ok()
    End Function
End Class
