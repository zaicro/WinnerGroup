Imports System.Data.Entity
Imports System.Data.Entity.ModelConfiguration.Conventions
Imports System.Data.Entity.Infrastructure.Annotations
Imports System.ComponentModel.DataAnnotations.Schema
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.DataAccess.Ef6
    ' Immutable model snapshot for 202610260001000_InitialCreate.
    Public NotInheritable Class ModelV1
        Public Shared Sub Configure(model As DbModelBuilder)
            model.Conventions.Remove(Of PluralizingTableNameConvention)()
            model.HasDefaultSchema("auth")
            ConfigureBase(model.Entity(Of User)(), "Users")
            ConfigureBase(model.Entity(Of Role)(), "Roles")
            ConfigureBase(model.Entity(Of ClientApp)(), "Apps")
            ConfigureBase(model.Entity(Of UserApp)(), "UserApps")
            With model.Entity(Of User)()
                .Property(Function(e) e.UserName).IsRequired().HasMaxLength(80)
                .Property(Function(e) e.NormalizedUserName).IsRequired().HasMaxLength(80).HasColumnAnnotation("Index", New IndexAnnotation(New IndexAttribute("UX_Users_Name") With {.IsUnique = True}))
                .Property(Function(e) e.PasswordHash).IsRequired().HasMaxLength(512)
                .HasRequired(Function(e) e.Role).WithMany().HasForeignKey(Function(e) e.RoleId).WillCascadeOnDelete(False)
            End With
            With model.Entity(Of Role)()
                .Property(Function(e) e.Name).IsRequired().HasMaxLength(80)
                .Property(Function(e) e.NormalizedName).IsRequired().HasMaxLength(80).HasColumnAnnotation("Index", New IndexAnnotation(New IndexAttribute("UX_Roles_Name") With {.IsUnique = True}))
            End With
            With model.Entity(Of ClientApp)()
                .Property(Function(e) e.Name).IsRequired().HasMaxLength(100)
                .Property(Function(e) e.Code).IsRequired().HasMaxLength(60).HasColumnAnnotation("Index", New IndexAnnotation(New IndexAttribute("UX_Apps_Code") With {.IsUnique = True}))
            End With
            With model.Entity(Of UserApp)()
                .Property(Function(e) e.UserId).HasColumnAnnotation("Index", New IndexAnnotation(New IndexAttribute("UX_UserApps_User_App", 0) With {.IsUnique = True}))
                .Property(Function(e) e.AppId).HasColumnAnnotation("Index", New IndexAnnotation(New IndexAttribute("UX_UserApps_User_App", 1) With {.IsUnique = True}))
                .HasRequired(Function(e) e.User).WithMany().HasForeignKey(Function(e) e.UserId).WillCascadeOnDelete(False)
                .HasRequired(Function(e) e.App).WithMany().HasForeignKey(Function(e) e.AppId).WillCascadeOnDelete(False)
            End With
        End Sub
        Private Shared Sub ConfigureBase(Of T As {Class, IEntity})(map As System.Data.Entity.ModelConfiguration.EntityTypeConfiguration(Of T), table As String)
            map.ToTable(table)
            map.HasKey(Function(e) e.Id)
            map.Property(Function(e) e.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
            map.Property(Function(e) e.Version).IsConcurrencyToken().HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)
        End Sub
    End Class
End Namespace
