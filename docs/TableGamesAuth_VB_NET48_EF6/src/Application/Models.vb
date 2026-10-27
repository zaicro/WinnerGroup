Imports System.ComponentModel.DataAnnotations

Namespace TableGamesAuth.Application
    Public Class VersionInput
        <Required> Public Property Version As Guid
    End Class
    Public Class UserInput
        Inherits VersionInput
        <Required, StringLength(80, MinimumLength:=3), RegularExpression("^[A-Za-z0-9._@-]+$")>
        Public Property UserName As String
        Public Property Password As String
        Public Property PasswordHash As String
        Public Property RoleId As Guid
        Public Property IsActive As Boolean = True
    End Class
    Public Class RoleInput
        Inherits VersionInput
        <Required, StringLength(80, MinimumLength:=2)>
        Public Property Name As String
    End Class
    Public Class AppInput
        Inherits VersionInput
        <Required, StringLength(100, MinimumLength:=2)>
        Public Property Name As String
        <Required, StringLength(60, MinimumLength:=2), RegularExpression("^[A-Za-z0-9._-]+$")>
        Public Property Code As String
        Public Property IsActive As Boolean = True
    End Class
    Public Class UserAppInput
        Inherits VersionInput
        Public Property UserId As Guid
        Public Property AppId As Guid
    End Class
    Public Class LoginInput
        <Required, StringLength(80)> Public Property UserName As String
        <Required, StringLength(128)> Public Property Password As String
        Public Property AppId As Guid
    End Class
    Public Class LoginResult
        Public Property AccessToken As String
        Public Property TokenType As String = "Bearer"
        Public Property ExpiresAtUtc As DateTime
    End Class
    Public Class UserView
        Public Property Id As Guid
        Public Property Version As Guid
        Public Property UserName As String
        Public Property RoleId As Guid
        Public Property IsActive As Boolean
    End Class
End Namespace
