Imports System
Imports System.Data.Entity.Migrations
Imports Microsoft.VisualBasic

Namespace Migrations.Identity
    Public Partial Class Init
        Inherits DbMigration
    
        Public Overrides Sub Up()
            CreateTable(
                "Identity.TbApp",
                Function(c) New With
                    {
                        .ApplicationId = c.Guid(nullable := False),
                        .Code = c.String(nullable := False, maxLength := 50),
                        .Name = c.String(nullable := False, maxLength := 150),
                        .IsActive = c.Boolean(nullable := False),
                        .CreatedBy = c.String(nullable := False, maxLength := 150),
                        .CreatedAtUtc = c.DateTime(nullable := False),
                        .ModifiedBy = c.String(nullable := False, maxLength := 150),
                        .ModifiedAtUtc = c.DateTime(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.ApplicationId) _
                .Index(Function(t) t.Code, unique := True, name := "UX_TbApp_Code")
            
            CreateTable(
                "Identity.TbRole",
                Function(c) New With
                    {
                        .RoleId = c.Guid(nullable := False),
                        .ApplicationId = c.Guid(nullable := False),
                        .Code = c.String(nullable := False, maxLength := 50),
                        .Name = c.String(nullable := False, maxLength := 150),
                        .IsActive = c.Boolean(nullable := False),
                        .CreatedBy = c.String(nullable := False, maxLength := 150),
                        .CreatedAtUtc = c.DateTime(nullable := False),
                        .ModifiedBy = c.String(nullable := False, maxLength := 150),
                        .ModifiedAtUtc = c.DateTime(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.RoleId) _
                .ForeignKey("Identity.TbApp", Function(t) t.ApplicationId) _
                .Index(Function(t) New With { t.ApplicationId, t.Code }, unique := True, name := "UX_TbRole_ApplicationId_Code")
            
            CreateTable(
                "Identity.TbUserRole",
                Function(c) New With
                    {
                        .UserRoleId = c.Guid(nullable := False),
                        .UserId = c.Guid(nullable := False),
                        .RoleId = c.Guid(nullable := False),
                        .IsActive = c.Boolean(nullable := False),
                        .CreatedBy = c.String(nullable := False, maxLength := 150),
                        .CreatedAtUtc = c.DateTime(nullable := False),
                        .ModifiedBy = c.String(nullable := False, maxLength := 150),
                        .ModifiedAtUtc = c.DateTime(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.UserRoleId) _
                .ForeignKey("Identity.TbRole", Function(t) t.RoleId) _
                .ForeignKey("Identity.TbUser", Function(t) t.UserId) _
                .Index(Function(t) New With { t.UserId, t.RoleId }, unique := True, name := "UX_TbUserRole_UserId_RoleId")
            
            CreateTable(
                "Identity.TbUser",
                Function(c) New With
                    {
                        .UserId = c.Guid(nullable := False),
                        .UserName = c.String(nullable := False, maxLength := 100),
                        .DisplayName = c.String(nullable := False, maxLength := 150),
                        .PasswordHash = c.String(nullable := False, maxLength := 1024),
                        .PhoneNumber = c.String(nullable := False, maxLength := 30),
                        .Email = c.String(nullable := False, maxLength := 254),
                        .IsActive = c.Boolean(nullable := False),
                        .CreatedBy = c.String(nullable := False, maxLength := 150),
                        .CreatedAtUtc = c.DateTime(nullable := False),
                        .ModifiedBy = c.String(nullable := False, maxLength := 150),
                        .ModifiedAtUtc = c.DateTime(nullable := False)
                    }) _
                .PrimaryKey(Function(t) t.UserId) _
                .Index(Function(t) t.UserName, unique := True, name := "UX_TbUser_UserName")
            
        End Sub
        
        Public Overrides Sub Down()
            DropForeignKey("Identity.TbUserRole", "UserId", "Identity.TbUser")
            DropForeignKey("Identity.TbUserRole", "RoleId", "Identity.TbRole")
            DropForeignKey("Identity.TbRole", "ApplicationId", "Identity.TbApp")
            DropIndex("Identity.TbUser", "UX_TbUser_UserName")
            DropIndex("Identity.TbUserRole", "UX_TbUserRole_UserId_RoleId")
            DropIndex("Identity.TbRole", "UX_TbRole_ApplicationId_Code")
            DropIndex("Identity.TbApp", "UX_TbApp_Code")
            DropTable("Identity.TbUser")
            DropTable("Identity.TbUserRole")
            DropTable("Identity.TbRole")
            DropTable("Identity.TbApp")
        End Sub
    End Class
End Namespace
