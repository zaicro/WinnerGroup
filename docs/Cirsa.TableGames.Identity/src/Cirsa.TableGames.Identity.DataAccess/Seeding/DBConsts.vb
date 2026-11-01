Public NotInheritable Class DBConsts

    Private Sub New()
    End Sub

    Public Const UserAudit As String = "Seed"

    Public Shared ReadOnly BaccaratApplicationId As Guid = Guid.Parse("92FDE845-AC85-4E96-A680-A4923C249931")

    Public Shared ReadOnly RouletteApplicationId As Guid = Guid.Parse("33CDF1EF-D86C-4950-93A0-305DC6FEA2D4")

    Public Shared ReadOnly AdministratorUserId As Guid = Guid.Parse("4B49EBE3-4340-4721-A93D-FB5F4B585CD1")

    Public Shared ReadOnly BaccaratRoleId As Guid = Guid.Parse("C68D779D-842F-45C3-B1CD-2F20047C96D7")

    Public Shared ReadOnly RouletteRoleId As Guid = Guid.Parse("A058E5E9-1632-42F6-9ECE-8242D362D389")

    ' Initial login credentials for development testing.
    ' User: admin
    ' Password: kvmtxk0uNnyYE6TuptAkrIi82Eg7GIOa
    Public Const InitialPasswordHash As String = "v1$pbkdf2-sha256$600000$PBIgk/OZvI5tIlgNNCe6RA==$tKiNn01mKTE1UQ7GAWx3hO9Bp9XRZ3fUtOxVDc0aUiE="

End Class