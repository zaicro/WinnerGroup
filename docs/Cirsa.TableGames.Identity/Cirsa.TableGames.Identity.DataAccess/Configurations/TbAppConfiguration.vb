Imports System.ComponentModel.DataAnnotations.Schema
Imports System.Data.Entity.Infrastructure.Annotations
Imports Cirsa.TableGames.Identity.DataAccess.Entities

Namespace Configurations

    ''' <summary>
    ''' Configures application persistence and constraints.
    ''' </summary>
    Friend Class TbAppConfiguration
        Inherits DbAuditableConfiguration(Of TbApp)

        Public Sub New()
            ToTable("TbApp")

            HasKey(Function(x) x.ApplicationId)

            [Property](Function(x) x.ApplicationId) _
                .HasDatabaseGeneratedOption(DatabaseGeneratedOption.None)

            [Property](Function(x) x.Code) _
                .IsRequired() _
                .HasMaxLength(50) _
                .HasColumnAnnotation(
                    IndexAnnotation.AnnotationName,
                    New IndexAnnotation(New IndexAttribute("UX_TbApp_Code") With {.IsUnique = True}))

            [Property](Function(x) x.Name) _
                .IsRequired() _
                .HasMaxLength(150)

            [Property](Function(x) x.IsActive).IsRequired()
        End Sub

    End Class

End Namespace