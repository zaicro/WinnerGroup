Imports System.Data.Entity.ModelConfiguration
Imports Cirsa.TableGames.Identity.DataAccess.Entities

Namespace Configurations

    ''' <summary>
    ''' Configures audit properties shared by persistence entities.
    ''' </summary>
    Friend MustInherit Class DbAuditableConfiguration(Of T As DbAuditable)
        Inherits EntityTypeConfiguration(Of T)

        Protected Sub New()
            [Property](Function(x) x.CreatedBy) _
                .HasMaxLength(150) _
                .IsRequired()

            [Property](Function(x) x.CreatedAtUtc) _
                .IsRequired()

            [Property](Function(x) x.ModifiedBy) _
                .HasMaxLength(150) _
                .IsRequired()

            [Property](Function(x) x.ModifiedAtUtc) _
                .IsRequired()
        End Sub

    End Class

End Namespace