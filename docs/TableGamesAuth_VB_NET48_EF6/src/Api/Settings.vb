Imports System.Linq
Imports Newtonsoft.Json

Namespace TableGamesAuth.Api
    Public Class ConfigurationException
        Inherits Exception
        Public Sub New(message As String)
            MyBase.New(message)
        End Sub
    End Class
    Public Class ApiKeyEntry
        Public Property AppId As Guid
        Public Property Sha256 As String
    End Class
    Public Class Settings
        Public Property Provider As String
        Public Property ConnectionString As String
        Public Property ListenUrl As String
        Public Property JwtKey As String
        Public Property Issuer As String = "TableGamesAuth"
        Public Property Audience As String = "TableGamesAuth.Api"
        Public Property AllowInsecureLoopback As Boolean
        Public Property ApiKeys As List(Of ApiKeyEntry)
        Public Shared Function Required(name As String) As String
            Dim value = Environment.GetEnvironmentVariable(name)
            If String.IsNullOrWhiteSpace(value) Then Throw New ConfigurationException("Falta la variable " & name)
            Return value
        End Function
        Public Shared Function Load() As Settings
            Dim result As New Settings With {
                .Provider = Required("TG_DB_PROVIDER"), .ConnectionString = Required("TG_DB_CONNECTION"),
                .ListenUrl = If(Environment.GetEnvironmentVariable("TG_LISTEN_URL"), "https://localhost:9443/"),
                .JwtKey = Required("TG_JWT_KEY"),
                .AllowInsecureLoopback = String.Equals(Environment.GetEnvironmentVariable("TG_ALLOW_INSECURE_LOOPBACK"), "true", StringComparison.OrdinalIgnoreCase),
                .ApiKeys = JsonConvert.DeserializeObject(Of List(Of ApiKeyEntry))(Required("TG_API_KEYS"))}
            Dim uri = New Uri(result.ListenUrl)
            If uri.Scheme <> "https" AndAlso Not (uri.Scheme = "http" AndAlso uri.IsLoopback AndAlso result.AllowInsecureLoopback) Then Throw New ConfigurationException("Use HTTPS; HTTP solo se permite explícitamente en loopback.")
            If result.ApiKeys Is Nothing OrElse result.ApiKeys.Count = 0 OrElse result.ApiKeys.Count > 100 Then Throw New ConfigurationException("Configure entre 1 y 100 API keys.")
            For Each key In result.ApiKeys
                If key.AppId = Guid.Empty OrElse key.Sha256 Is Nothing OrElse key.Sha256.Length <> 64 OrElse key.Sha256.Any(Function(c) Not Uri.IsHexDigit(c)) Then Throw New ConfigurationException("API key mal configurada.")
            Next
            If result.ApiKeys.Select(Function(k) k.Sha256.ToUpperInvariant()).Distinct().Count() <> result.ApiKeys.Count Then Throw New ConfigurationException("Una API key no puede aparecer más de una vez.")
            Return result
        End Function
    End Class
End Namespace
