Imports System.Web.Http.Dependencies
Imports Microsoft.Extensions.DependencyInjection

Namespace TableGamesAuth.Api
    Public Class DependencyResolver
        Implements IDependencyResolver
        Private ReadOnly services As IServiceProvider
        Private ReadOnly scope As IServiceScope
        Public Sub New(services As IServiceProvider, Optional scope As IServiceScope = Nothing)
            Me.services = services : Me.scope = scope
        End Sub
        Public Function BeginScope() As IDependencyScope Implements IDependencyResolver.BeginScope
            Dim child = services.CreateScope()
            Return New DependencyResolver(child.ServiceProvider, child)
        End Function
        Public Function GetService(serviceType As Type) As Object Implements IDependencyScope.GetService
            Return services.GetService(serviceType)
        End Function
        Public Function GetServices(serviceType As Type) As IEnumerable(Of Object) Implements IDependencyScope.GetServices
            Return services.GetServices(serviceType)
        End Function
        Public Sub Dispose() Implements IDisposable.Dispose
            If scope IsNot Nothing Then scope.Dispose()
        End Sub
    End Class
End Namespace
