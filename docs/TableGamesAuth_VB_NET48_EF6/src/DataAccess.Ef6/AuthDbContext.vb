Imports System.Data.Common
Imports System.Data.Entity
Imports System.Data.Entity.ModelConfiguration.Conventions
Imports System.Data.Entity.Infrastructure.Annotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.DataAccess.Ef6
    Public MustInherit Class AuthDbContext
        Inherits DbContext
        Protected Sub New(connection As DbConnection)
            MyBase.New(connection, True)
            Configuration.LazyLoadingEnabled = False
            Configuration.ProxyCreationEnabled = False
        End Sub
        Public Property Users As DbSet(Of User)
        Public Property Roles As DbSet(Of Role)
        Public Property Apps As DbSet(Of ClientApp)
        Public Property UserApps As DbSet(Of UserApp)
        Protected Overrides Sub OnModelCreating(model As DbModelBuilder)
            ModelV1.Configure(model)
            MyBase.OnModelCreating(model)
        End Sub
    End Class
End Namespace
