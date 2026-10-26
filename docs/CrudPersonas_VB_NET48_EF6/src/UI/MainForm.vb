Imports System.Windows.Forms
Imports CrudPersonas.Application

Public Partial Class MainForm
    Inherits Form

    Private ReadOnly _servicio As PersonaService
    Private _idSeleccionado As Integer

    Public Sub New(servicio As PersonaService)
        If servicio Is Nothing Then Throw New ArgumentNullException(NameOf(servicio))
        _servicio = servicio
        InitializeComponent()
    End Sub

    Private Sub FormularioCargado(sender As Object, e As EventArgs) Handles MyBase.Load
        Ejecutar(Sub() CargarPersonas())
    End Sub

    Private Sub CargarPersonas()
        grdPersonas.DataSource = Nothing
        grdPersonas.DataSource = _servicio.Listar()
        LimpiarEdicion()
        lblEstado.Text = grdPersonas.Rows.Count.ToString() & " persona(s) registrada(s)"
    End Sub

    Private Sub LimpiarEdicion()
        _idSeleccionado = 0
        txtNombre.Clear()
        grdPersonas.ClearSelection()
        btnGuardar.Text = "Crear"
        btnEliminar.Enabled = False
    End Sub

    Private Sub SeleccionarPersona(sender As Object, e As EventArgs) Handles grdPersonas.SelectionChanged
        If grdPersonas.SelectedRows.Count = 0 Then Return
        Dim persona = TryCast(grdPersonas.SelectedRows(0).DataBoundItem, PersonaDto)
        If persona Is Nothing Then Return
        _idSeleccionado = persona.Id
        txtNombre.Text = persona.Nombre
        btnGuardar.Text = "Guardar cambios"
        btnEliminar.Enabled = True
    End Sub

    Private Sub Guardar(sender As Object, e As EventArgs) Handles btnGuardar.Click
        Ejecutar(Sub()
                     If _idSeleccionado = 0 Then
                         _servicio.Crear(txtNombre.Text)
                     Else
                         _servicio.Editar(_idSeleccionado, txtNombre.Text)
                     End If
                     CargarPersonas()
                     lblEstado.Text = "Persona guardada correctamente."
                     txtNombre.Focus()
                 End Sub)
    End Sub

    Private Sub Eliminar(sender As Object, e As EventArgs) Handles btnEliminar.Click
        If _idSeleccionado = 0 Then Return
        If MessageBox.Show(Me, "¿Eliminar a " & txtNombre.Text & "?", "Confirmar eliminación",
                           MessageBoxButtons.YesNo, MessageBoxIcon.Question) <> DialogResult.Yes Then Return
        Ejecutar(Sub()
                     _servicio.Eliminar(_idSeleccionado)
                     CargarPersonas()
                     lblEstado.Text = "Persona eliminada."
                 End Sub)
    End Sub

    Private Sub Nuevo(sender As Object, e As EventArgs) Handles btnNuevo.Click
        LimpiarEdicion()
        txtNombre.Focus()
    End Sub

    Private Sub Refrescar(sender As Object, e As EventArgs) Handles btnActualizar.Click
        Ejecutar(Sub() CargarPersonas())
    End Sub

    Private Sub Ejecutar(accion As Action)
        Try
            UseWaitCursor = True
            accion()
        Catch ex As ArgumentException
            MessageBox.Show(Me, ex.Message, "Revisa el nombre", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            txtNombre.Focus()
        Catch ex As InvalidOperationException
            Program.RegistrarError(ex)
            MessageBox.Show(Me, ex.Message, "No se pudo completar la operación", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        Catch ex As Exception
            Program.RegistrarError(ex)
            MessageBox.Show(Me, "No se pudo completar la operación. Revisa la conexión y el registro de errores.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        Finally
            UseWaitCursor = False
        End Try
    End Sub
End Class
