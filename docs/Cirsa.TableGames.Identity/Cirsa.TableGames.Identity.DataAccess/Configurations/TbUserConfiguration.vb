Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Infrastructure.Annotations
Imports Cirsa.TableGames.Identity.DataAccess.Entities

Namespace Configurations

    ''' <summary>
    ''' Configures user persistence and constraints.
    ''' </summary>
    Friend Class TbUserConfiguration
        Inherits DbAuditableConfiguration(Of TbUser)

        Public Sub New()
            ToTable("TbUser")

            HasKey(Function(x) x.UserId)

            [Property](Function(x) x.UserId) _
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)

            [Property](Function(x) x.UserName) _
                .IsRequired() _
                .HasMaxLength(100) _
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    New IndexAnnotation(New IndexAttribute("UX_TbUser_UserName") With {.IsUnique = True})
                    )

            [Property](Function(x) x.DisplayName) _
                .IsRequired() _
                .HasMaxLength(150)

            [Property](Function(x) x.PasswordHash) _
                .IsRequired() _
                .HasMaxLength(1024)

            [Property](Function(x) x.PhoneNumber) _
                .IsRequired() _
                .HasMaxLength(30)

            [Property](Function(x) x.Email) _
                .IsRequired() _
                .HasMaxLength(254)

            [Property](Function(x) x.IsActive).IsRequired()
        End Sub

    End Class

End Namespace