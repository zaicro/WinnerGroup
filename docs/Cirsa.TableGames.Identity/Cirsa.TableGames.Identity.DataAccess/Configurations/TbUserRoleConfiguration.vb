Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Infrastructure.Annotations
Imports Cirsa.TableGames.Identity.DataAccess.Entities

Namespace Configurations

    ''' <summary>
    ''' Configures user-role assignments and prevents duplicates.
    ''' </summary>
    Friend Class TbUserRoleConfiguration
        Inherits DbAuditableConfiguration(Of TbUserRole)

        Public Sub New()
            ToTable("TbUserRole")

            HasKey(Function(x) x.UserRoleId)

            [Property](Function(x) x.UserRoleId) _
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)

            [Property](Function(x) x.UserId) _
                .IsRequired() _
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    New IndexAnnotation(New IndexAttribute("UX_TbUserRole_UserId_RoleId", 1) With {.IsUnique = True})
                    )

            [Property](Function(x) x.RoleId) _
                .IsRequired() _
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    New IndexAnnotation(New IndexAttribute("UX_TbUserRole_UserId_RoleId", 2) With {.IsUnique = True})
                    )

            [Property](Function(x) x.IsActive).IsRequired()

            HasRequired(Function(x) x.User) _
                .WithMany(Function(x) x.UserRoles) _
                .HasForeignKey(Function(x) x.UserId) _
                .WillCascadeOnDelete(False)

            HasRequired(Function(x) x.Role) _
                .WithMany(Function(x) x.UserRoles) _
                .HasForeignKey(Function(x) x.RoleId) _
                .WillCascadeOnDelete(False)
        End Sub

    End Class

End Namespace