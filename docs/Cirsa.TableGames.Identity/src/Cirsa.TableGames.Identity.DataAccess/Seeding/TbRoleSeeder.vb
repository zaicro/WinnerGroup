Imports Cirsa.TableGames.Identity.DataAccess.Entities

Namespace Seeding

    ''' <summary>
    ''' Creates the initial identity data without modifying existing records.
    ''' </summary>
    Public NotInheritable Class TbRoleSeeder

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

            If Not context.TbRoles.Any(Function(x) x.ApplicationId = DBConsts.BaccaratApplicationId And x.Code = "Admin") Then
                context.TbRoles.Add(
                    New TbRole With {
                        .RoleId = DBConsts.BaccaratRoleId,
                        .ApplicationId = DBConsts.BaccaratApplicationId,
                        .Code = "admin",
                        .Name = "Admin",
                        .IsActive = True,
                        .CreatedBy = DBConsts.UserAudit,
                        .CreatedAtUtc = createdAtUtc,
                        .ModifiedBy = DBConsts.UserAudit,
                        .ModifiedAtUtc = createdAtUtc
                    })
            End If

            If Not context.TbRoles.Any(Function(x) x.ApplicationId = DBConsts.RouletteApplicationId And x.Code = "Admin") Then
                context.TbRoles.Add(
                    New TbRole With {
                        .RoleId = DBConsts.RouletteRoleId,
                        .ApplicationId = DBConsts.RouletteApplicationId,
                        .Code = "admin",
                        .Name = "Admin",
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