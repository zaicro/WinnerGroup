Imports System.ComponentModel.DataAnnotations

Namespace Models
    Public Class LoginRequest
        <Required, StringLength(100)>
        Public Property Username As String
        <Required, StringLength(256, MinimumLength:=12)>
        Public Property Password As String
    End Class
End Namespace
