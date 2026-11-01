Namespace Entities

    ''' <summary>
    ''' Represents an application registered in the identity system.
    ''' </summary>
    Public Class TbApp
        Inherits DbAuditable

        Public Sub New()
            ApplicationId = Guid.NewGuid()
            IsActive = True
            Roles = New HashSet(Of TbRole)()
        End Sub

        Public Property ApplicationId As Guid
        Public Property Code As String
        Public Property Name As String
        Public Property IsActive As Boolean

        Public Property Roles As ICollection(Of TbRole)
    End Class
End Namespace