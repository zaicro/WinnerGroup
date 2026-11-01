Imports System.Data.Entity
Imports Cirsa.TableGames.Identity.Domain.Entities
Imports Cirsa.TableGames.Identity.Domain.Repositories

Namespace Repositories
    Public Class UserRepository
        Implements IUserRepository

        Private ReadOnly _db As IdentityDbContext

        Public Sub New(db As IdentityDbContext)
            If db Is Nothing Then Throw New ArgumentNullException(NameOf(db))
            _db = db
        End Sub

        Public Async Function FindForApplicationAsync(userName As String, applicationCode As String) As Task(Of User) _
            Implements IUserRepository.FindForApplicationAsync

            Dim results = Await _db.TbUsers _
                .AsNoTracking() _
                .Where(Function(user) user.UserName = userName AndAlso user.IsActive) _
                .SelectMany(
                    Function(user) user.UserRoles,
                    Function(user, assignment) New With {
                        .UserEntity = user,
                        .RoleEntity = assignment.Role
                    }
                ) _
                .Where(Function(x) x.RoleEntity.IsActive AndAlso x.RoleEntity.App.IsActive AndAlso x.RoleEntity.App.Code = applicationCode) _
                .Select(
                    Function(x) New With {
                        .UserEntity = x.UserEntity,
                        .RoleEntity = x.RoleEntity,
                        .AppEntity = x.RoleEntity.App
                    }
                ) _
                .ToListAsync()

            If results.Count = 0 Then Return Nothing

            Dim dbUser = results(0).UserEntity

            Return New User With {
                .UserId = dbUser.UserId,
                .UserName = dbUser.UserName,
                .DisplayName = dbUser.DisplayName,
                .PhoneNumber = dbUser.PhoneNumber,
                .Email = dbUser.Email,
                .PasswordHash = dbUser.PasswordHash,
                .UserRoles = results.Select(
                    Function(row) New UserRole With {
                        .RoleId = row.RoleEntity.RoleId,
                        .Role = New Role With {
                            .RoleId = row.RoleEntity.RoleId,
                            .Code = row.RoleEntity.Code,
                            .Name = row.RoleEntity.Name,
                            .ApplicationId = row.RoleEntity.ApplicationId,
                            .App = New App With {
                                .ApplicationId = row.AppEntity.ApplicationId,
                                .Code = row.AppEntity.Code,
                                .Name = row.AppEntity.Name
                            }
                        }
                    }
                ) _
                .ToList()
            }
        End Function

    End Class
End Namespace