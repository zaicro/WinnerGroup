Imports System.Web.Http.Dependencies
Imports Microsoft.Extensions.DependencyInjection

Namespace DependencyInjection

    ''' <summary>
    ''' Connects Web API dependency resolution with the service container.
    ''' </summary>
    Public NotInheritable Class DependencyInjectionResolver
        Implements IDependencyResolver

        Private ReadOnly _provider As IServiceProvider
        Private ReadOnly _owner As IDisposable
        Private ReadOnly _scopeFactory As IServiceScopeFactory

        Private _disposed As Boolean

        Public Sub New(provider As ServiceProvider)
            If provider Is Nothing Then Throw New ArgumentNullException(NameOf(provider))
            _provider = provider
            _owner = provider
            _scopeFactory = provider.GetRequiredService(Of IServiceScopeFactory)()
        End Sub

        Private Sub New(scope As IServiceScope)
            _provider = scope.ServiceProvider
            _owner = scope
            _scopeFactory = _provider.GetRequiredService(Of IServiceScopeFactory)()
        End Sub

        Public Function BeginScope() As IDependencyScope _
            Implements IDependencyResolver.BeginScope
            Return New DependencyInjectionResolver(_scopeFactory.CreateScope())
        End Function

        Public Function GetService(serviceType As Type) As Object _
            Implements IDependencyScope.GetService
            Return _provider.GetService(serviceType)
        End Function

        Public Function GetServices(serviceType As Type) As IEnumerable(Of Object) _
            Implements IDependencyScope.GetServices
            Return _provider.GetServices(serviceType)
        End Function

        Public Sub Dispose() Implements IDisposable.Dispose
            If _disposed Then Return
            _disposed = True
            _owner.Dispose()
        End Sub
    End Class
End Namespace