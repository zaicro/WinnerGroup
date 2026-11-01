Imports Cirsa.TableGames.Identity.DataAccess.Entities
Imports Microsoft.VisualBasic.ApplicationServices

Namespace Seeding

    ''' <summary>
    ''' Creates the initial identity data without modifying existing records.
    ''' </summary>
    Public NotInheritable Class TbUserRoleSeeder

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

            If Not context.TbUsersRoles.Any(Function(x) x.UserId = DBConsts.AdministratorUserId And x.RoleId = DBConsts.BaccaratRoleId) Then
                context.TbUsersRoles.Add(
                    New TbUserRole With {
                        .UserRoleId = Guid.NewGuid(),
                        .UserId = DBConsts.AdministratorUserId,
                        .RoleId = DBConsts.BaccaratRoleId,
                        .IsActive = True,
                        .CreatedBy = DBConsts.UserAudit,
                        .CreatedAtUtc = createdAtUtc,
                        .ModifiedBy = DBConsts.UserAudit,
                        .ModifiedAtUtc = createdAtUtc
                    })
            End If

            If Not context.TbUsersRoles.Any(Function(x) x.UserId = DBConsts.AdministratorUserId And x.RoleId = DBConsts.RouletteRoleId) Then
                context.TbUsersRoles.Add(
                    New TbUserRole With {
                        .UserRoleId = Guid.NewGuid(),
                        .UserId = DBConsts.AdministratorUserId,
                        .RoleId = DBConsts.RouletteRoleId,
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