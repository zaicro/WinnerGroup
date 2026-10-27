Imports System.Data.Entity
Imports System.Linq
Imports System.Data.Entity.Infrastructure
Imports System.Data.Entity.Migrations
Imports System.IO
Imports System.IO.Compression
Imports System.Text
Imports System.Xml
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.DataAccess.Ef6
    Public MustInherit Class InitialSchema
        Inherits DbMigration
        Public Overrides Sub Up()
            CreateTable("auth.Roles", Function(c) New With {
                .Id = c.Guid(nullable:=False), .Version = c.Guid(nullable:=False),
                .Name = c.String(nullable:=False, maxLength:=80), .NormalizedName = c.String(nullable:=False, maxLength:=80)
            }).PrimaryKey(Function(t) t.Id).Index(Function(t) t.NormalizedName, unique:=True, name:="UX_Roles_Name")
            CreateTable("auth.Apps", Function(c) New With {
                .Id = c.Guid(nullable:=False), .Version = c.Guid(nullable:=False),
                .Name = c.String(nullable:=False, maxLength:=100), .Code = c.String(nullable:=False, maxLength:=60), .IsActive = c.Boolean(nullable:=False)
            }).PrimaryKey(Function(t) t.Id).Index(Function(t) t.Code, unique:=True, name:="UX_Apps_Code")
            CreateTable("auth.Users", Function(c) New With {
                .Id = c.Guid(nullable:=False), .Version = c.Guid(nullable:=False),
                .UserName = c.String(nullable:=False, maxLength:=80), .NormalizedUserName = c.String(nullable:=False, maxLength:=80),
                .PasswordHash = c.String(nullable:=False, maxLength:=512), .RoleId = c.Guid(nullable:=False), .IsActive = c.Boolean(nullable:=False),
                .SecurityStamp = c.Guid(nullable:=False), .FailedLoginCount = c.Int(nullable:=False), .LockoutUntilUtc = c.DateTime()
            }).PrimaryKey(Function(t) t.Id).Index(Function(t) t.RoleId).Index(Function(t) t.NormalizedUserName, unique:=True, name:="UX_Users_Name")
            CreateTable("auth.UserApps", Function(c) New With {
                .Id = c.Guid(nullable:=False), .Version = c.Guid(nullable:=False),
                .UserId = c.Guid(nullable:=False), .AppId = c.Guid(nullable:=False)
            }).PrimaryKey(Function(t) t.Id).Index(Function(t) New With {t.UserId, t.AppId}, unique:=True, name:="UX_UserApps_User_App").Index(Function(t) t.AppId)
            AddForeignKey("auth.Users", "RoleId", "auth.Roles", "Id", cascadeDelete:=False)
            AddForeignKey("auth.UserApps", "UserId", "auth.Users", "Id", cascadeDelete:=False)
            AddForeignKey("auth.UserApps", "AppId", "auth.Apps", "Id", cascadeDelete:=False)
        End Sub
        Public Overrides Sub Down()
            DropForeignKey("auth.UserApps", "AppId", "auth.Apps")
            DropForeignKey("auth.UserApps", "UserId", "auth.Users")
            DropForeignKey("auth.Users", "RoleId", "auth.Roles")
            DropTable("auth.UserApps") : DropTable("auth.Users") : DropTable("auth.Apps") : DropTable("auth.Roles")
        End Sub
    End Class
    Public Module MigrationSnapshot
        Public Function Build(provider As String, manifest As String) As String
            Dim builder As New DbModelBuilder()
            ModelV1.Configure(builder)
            Dim model = builder.Build(New DbProviderInfo(provider, manifest))
            Dim xml As New StringBuilder()
            Using writer = XmlWriter.Create(xml, New XmlWriterSettings With {.OmitXmlDeclaration = True})
                EdmxWriter.WriteEdmx(model, writer)
            End Using
            Using stream As New MemoryStream()
                Using gzip As New GZipStream(stream, CompressionMode.Compress, True)
                    Dim bytes = Encoding.UTF8.GetBytes(xml.ToString())
                    gzip.Write(bytes, 0, bytes.Length)
                End Using
                Return Convert.ToBase64String(stream.ToArray())
            End Using
        End Function
    End Module
    Public Module SeedData
        Public ReadOnly AdminRoleId As New Guid("10000000-0000-0000-0000-000000000001")
        Public ReadOnly OperatorRoleId As New Guid("10000000-0000-0000-0000-000000000002")
        Public ReadOnly ManagementAppId As New Guid("20000000-0000-0000-0000-000000000001")
        Public ReadOnly RouletteAppId As New Guid("20000000-0000-0000-0000-000000000002")
        Public ReadOnly BaccaratAppId As New Guid("20000000-0000-0000-0000-000000000003")
        Public Sub Apply(db As AuthDbContext)
            ' Insert-only seed. Never resets names, active flags or permissions on subsequent updates.
            If Not db.Roles.Any(Function(r) r.Id = AdminRoleId) Then db.Roles.Add(New Role With {.Id = AdminRoleId, .Name = "SystemAdmin", .NormalizedName = "SYSTEMADMIN"})
            If Not db.Roles.Any(Function(r) r.Id = OperatorRoleId) Then db.Roles.Add(New Role With {.Id = OperatorRoleId, .Name = "Operator", .NormalizedName = "OPERATOR"})
            AddApp(db, ManagementAppId, "Administración", "AUTH-MANAGEMENT")
            AddApp(db, RouletteAppId, "Crazzy Roulette", "CRAZZY-ROULETTE")
            AddApp(db, BaccaratAppId, "Baccarat", "BACCARAT")
            db.SaveChanges()
        End Sub
        Private Sub AddApp(db As AuthDbContext, id As Guid, name As String, code As String)
            If Not db.Apps.Any(Function(a) a.Id = id) Then db.Apps.Add(New ClientApp With {.Id = id, .Name = name, .Code = code})
        End Sub
    End Module
End Namespace
