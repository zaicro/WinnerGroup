Imports System.Collections.Generic

Public NotInheritable Class SecretValue
    Public ReadOnly Property Value As String
    Public ReadOnly Property Version As String
    Public ReadOnly Property ExpiresOn As Nullable(Of DateTimeOffset)
    Public Sub New(value As String, version As String, Optional expiresOn As Nullable(Of DateTimeOffset) = Nothing)
        If String.IsNullOrEmpty(value) Then Throw New ArgumentException("El secreto no puede estar vacío.")
        Me.Value = value
        Me.Version = version
        Me.ExpiresOn = expiresOn
    End Sub
    Public Overrides Function ToString() As String
        Return "[SECRET REDACTED]"
    End Function
End Class

Public Interface ISecretStore
    Function GetSecret(name As String) As SecretValue
End Interface

' Cache solo en memoria, con TTL explícito. No sirve valores vencidos si el origen falla.
Public NotInheritable Class CachedSecretStore
    Implements ISecretStore
    Private ReadOnly _inner As ISecretStore
    Private ReadOnly _ttl As TimeSpan
    Private ReadOnly _clock As Func(Of DateTimeOffset)
    Private ReadOnly _gate As New Object()
    Private ReadOnly _entries As New Dictionary(Of String, Tuple(Of DateTimeOffset, SecretValue))(StringComparer.Ordinal)
    Public Sub New(inner As ISecretStore, ttl As TimeSpan, Optional clock As Func(Of DateTimeOffset) = Nothing)
        If inner Is Nothing Then Throw New ArgumentNullException(NameOf(inner))
        If ttl <= TimeSpan.Zero OrElse ttl > TimeSpan.FromHours(1) Then Throw New ArgumentOutOfRangeException(NameOf(ttl))
        _inner = inner
        _ttl = ttl
        _clock = If(clock, Function() DateTimeOffset.UtcNow)
    End Sub
    Public Function GetSecret(name As String) As SecretValue Implements ISecretStore.GetSecret
        SyncLock _gate
            Dim entry As Tuple(Of DateTimeOffset, SecretValue) = Nothing
            If _entries.TryGetValue(name, entry) AndAlso entry.Item1 > _clock() Then Return entry.Item2
            ' Se serializan lecturas en este ejemplo: evita solicitudes duplicadas al renovar.
            Dim value = _inner.GetSecret(name)
            Dim expires = _clock().Add(_ttl)
            If value.ExpiresOn.HasValue AndAlso value.ExpiresOn.Value < expires Then expires = value.ExpiresOn.Value
            If expires <= _clock() Then Throw New InvalidOperationException("El secreto ya venció.")
            _entries(name) = Tuple.Create(expires, value)
            Return value
        End SyncLock
    End Function
    Public Sub Clear()
        SyncLock _gate
            _entries.Clear()
        End SyncLock
    End Sub
End Class
