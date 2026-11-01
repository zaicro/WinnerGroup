Imports System.Data.Entity
Imports Cirsa.TableGames.Identity.DataAccess.Configurations
Imports Cirsa.TableGames.Identity.DataAccess.Entities

Public Class IdentityDbContext
    Inherits DbContext

    Protected Sub New()
        MyBase.New()

        ConfigureContext()
    End Sub

    Protected Sub New(connectionString As String)
        MyBase.New(connectionString)

        ConfigureContext()
    End Sub

    Private Sub ConfigureContext()
        Configuration.LazyLoadingEnabled = False
        Configuration.ProxyCreationEnabled = False
    End Sub

    Public Property TbApps As DbSet(Of TbApp)
    Public Property TbRoles As DbSet(Of TbRole)
    Public Property TbUsers As DbSet(Of TbUser)
    Public Property TbUsersRoles As DbSet(Of TbUserRole)

    Protected Overrides Sub OnModelCreating(modelBuilder As DbModelBuilder)
        modelBuilder.HasDefaultSchema("Identity")
        modelBuilder.Ignore(Of DbAuditable)()
        modelBuilder.Configurations.Add(New TbAppConfiguration())
        modelBuilder.Configurations.Add(New TbRoleConfiguration())
        modelBuilder.Configurations.Add(New TbUserConfiguration())
        modelBuilder.Configurations.Add(New TbUserRoleConfiguration())
        MyBase.OnModelCreating(modelBuilder)
    End Sub

End Class