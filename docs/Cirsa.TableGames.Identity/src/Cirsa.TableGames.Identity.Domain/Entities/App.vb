Namespace Entities

    ''' <summary>
    ''' Represents an application registered in the identity system.
    ''' </summary>
    Public Class App

        Public Sub New()
            ApplicationId = Guid.NewGuid()
            IsActive = True
            Roles = New HashSet(Of Role)()
        End Sub

        Public Property ApplicationId As Guid
        Public Property Code As String
        Public Property Name As String
        Public Property IsActive As Boolean

        Public Property Roles As ICollection(Of Role)
    End Class
End Namespace