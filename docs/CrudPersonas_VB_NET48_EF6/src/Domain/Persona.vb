Public Class Persona
    Public Const LongitudMaximaNombre As Integer = 150

    ' EF necesita un constructor sin parámetros y un setter accesible al materializar.
    Public Sub New()
    End Sub

    Public Sub New(nombre As String)
        CambiarNombre(nombre)
    End Sub

    Public Property Id As Integer
    Public Property Nombre As String

    Public Sub CambiarNombre(valor As String)
        If String.IsNullOrWhiteSpace(valor) Then
            Throw New ArgumentException("Debes ingresar un nombre.", NameOf(valor))
        End If
        Dim limpio = valor.Trim()
        If limpio.Length > LongitudMaximaNombre Then
            Throw New ArgumentException("El nombre no puede superar los 150 caracteres.", NameOf(valor))
        End If
        Nombre = limpio
    End Sub
End Class
