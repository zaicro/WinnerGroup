Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Infrastructure.Annotations
Imports Cirsa.TableGames.Identity.DataAccess.Entities

Namespace Configurations

    ''' <summary>
    ''' Configures application-specific roles and their relationships.
    ''' </summary>
    Friend Class TbRoleConfiguration
        Inherits DbAuditableConfiguration(Of TbRole)

        Public Sub New()
            ToTable("TbRole")

            HasKey(Function(x) x.RoleId)

            [Property](Function(x) x.RoleId) _
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)

            [Property](Function(x) x.ApplicationId) _
                .IsRequired() _
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    New IndexAnnotation(New IndexAttribute("UX_TbRole_ApplicationId_Code", 1) With {.IsUnique = True})
                    )

            [Property](Function(x) x.Code) _
                .IsRequired() _
                .HasMaxLength(50) _
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    New IndexAnnotation(New IndexAttribute("UX_TbRole_ApplicationId_Code", 2) With {.IsUnique = True})
                    )

            [Property](Function(x) x.Name) _
                .IsRequired() _
                .HasMaxLength(150)

            [Property](Function(x) x.IsActive).IsRequired()

            HasRequired(Function(x) x.App) _
                .WithMany(Function(x) x.Roles) _
                .HasForeignKey(Function(x) x.ApplicationId) _
                .WillCascadeOnDelete(False)
        End Sub

    End Class

End Namespace