Imports Cirsa.TableGames.Identity.DataAccess.Entities

Namespace Seeding

    ''' <summary>
    ''' Creates the initial identity data without modifying existing records.
    ''' </summary>
    Public NotInheritable Class TbUserSeeder

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

            If Not context.TbUsers.Any(Function(x) x.UserName = "admin") Then

                context.TbUsers.Add(
                    New TbUser With {
                        .UserId = DBConsts.AdministratorUserId,
                        .UserName = "admin",
                        .DisplayName = "Administrator",
                        .PasswordHash = DBConsts.InitialPasswordHash,
                        .PhoneNumber = "0123456789",
                        .Email = "admin@admin.com",
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