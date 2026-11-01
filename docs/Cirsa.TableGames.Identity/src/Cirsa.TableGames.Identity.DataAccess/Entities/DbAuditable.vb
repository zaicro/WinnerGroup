Namespace Entities

    ''' <summary>
    ''' Stores creation and last modification audit information.
    ''' All timestamps must be recorded in UTC.
    ''' </summary>
    ''' <remarks>
    ''' Audit values must be assigned by the persistence layer.
    ''' </remarks>
    Public MustInherit Class DbAuditable
        Public Property CreatedBy As String
        Public Property CreatedAtUtc As DateTime
        Public Property ModifiedBy As String
        Public Property ModifiedAtUtc As DateTime
    End Class
End Namespace