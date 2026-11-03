Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Net
Imports System.Net.Http
Imports System.Net.Http.Headers
Imports System.Threading
Imports System.Threading.Tasks
Imports System.Web.Http
Imports Cirsa.WebApi2.Extensions.Authentication.Configuration

Namespace Authentication.Filters
    Friend NotInheritable Class AuthenticationChallenges
        Private Sub New()
        End Sub
        Friend Shared Function ForMode(mode As AuthenticationMode) As IEnumerable(Of AuthenticationHeaderValue)
            Dim result As New List(Of AuthenticationHeaderValue)()
            If mode = AuthenticationMode.ApiKey OrElse mode = AuthenticationMode.Both OrElse mode = AuthenticationMode.Either Then
                result.Add(New AuthenticationHeaderValue("ApiKey"))
            End If
            If mode = AuthenticationMode.Jwt OrElse mode = AuthenticationMode.Both OrElse mode = AuthenticationMode.Either Then
                result.Add(New AuthenticationHeaderValue("Bearer"))
            End If
            Return result
        End Function
    End Class

    Friend NotInheritable Class AuthenticationChallengeResult
        Implements IHttpActionResult
        Private ReadOnly _inner As IHttpActionResult
        Private ReadOnly _challenges As IEnumerable(Of AuthenticationHeaderValue)
        Friend Sub New(inner As IHttpActionResult, challenges As IEnumerable(Of AuthenticationHeaderValue))
            If inner Is Nothing Then Throw New ArgumentNullException(NameOf(inner))
            _inner = inner
            _challenges = challenges
        End Sub
        Public Async Function ExecuteAsync(cancellationToken As CancellationToken) As Task(Of HttpResponseMessage) Implements IHttpActionResult.ExecuteAsync
            Dim response = Await _inner.ExecuteAsync(cancellationToken).ConfigureAwait(False)
            If response.StatusCode = HttpStatusCode.Unauthorized Then
                For Each challenge In _challenges
                    If Not response.Headers.WwwAuthenticate.Any(Function(existing) String.Equals(existing.Scheme, challenge.Scheme, StringComparison.OrdinalIgnoreCase)) Then
                        response.Headers.WwwAuthenticate.Add(challenge)
                    End If
                Next
            End If
            Return response
        End Function
    End Class
End Namespace
