Imports Cirsa.TableGames.Identity.DataAccess.Entities

Namespace Seeding

    ''' <summary>
    ''' Creates the initial identity data without modifying existing records.
    ''' </summary>
    Public NotInheritable Class TbAppSeeder

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Creates missing application records.
        ''' </summary>
        Public Shared Sub Seed(context As IdentityDbContext)
            If context Is Nothing Then
                Throw New ArgumentNullException(NameOf(context))
            End If

            Dim createdAtUtc = DateTime.UtcNow

            If Not context.TbApps.Any(Function(x) x.Code = "baccarat") Then
                context.TbApps.Add(
                    New TbApp With {
                        .ApplicationId = DBConsts.BaccaratApplicationId,
                        .Code = "baccarat",
                        .Name = "Baccarat",
                        .IsActive = True,
                        .CreatedBy = DBConsts.UserAudit,
                        .CreatedAtUtc = createdAtUtc,
                        .ModifiedBy = DBConsts.UserAudit,
                        .ModifiedAtUtc = createdAtUtc
                    })
            End If

            If Not context.TbApps.Any(Function(x) x.Code = "crazzy_roulet") Then
                context.TbApps.Add(
                    New TbApp With {
                        .ApplicationId = DBConsts.RouletteApplicationId,
                        .Code = "crazzy_roulet",
                        .Name = "Crazzy Roulet",
                        .IsActive = True,
                        .CreatedBy = DBConsts.UserAudit,
                        .CreatedAtUtc = createdAtUtc,
                        .ModifiedBy = DBConsts.UserAudit,
                        .ModifiedAtUtc = createdAtUtc
                    })
            End If

            context.SaveChanges()
        End Sub

    End Class
End Namespace