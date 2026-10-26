Imports System.Drawing
Imports System.Windows.Forms

<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class MainForm
    Private components As System.ComponentModel.IContainer
    Private WithEvents txtNombre As TextBox
    Private WithEvents grdPersonas As DataGridView
    Private WithEvents btnNuevo As Button
    Private WithEvents btnGuardar As Button
    Private WithEvents btnEliminar As Button
    Private WithEvents btnActualizar As Button
    Private lblEstado As Label

    Protected Overrides Sub Dispose(disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then components.Dispose()
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    Private Sub InitializeComponent()
        components = New System.ComponentModel.Container()
        Dim lblTitulo As New Label()
        Dim lblNombre As New Label()
        txtNombre = New TextBox()
        grdPersonas = New DataGridView()
        btnNuevo = New Button()
        btnGuardar = New Button()
        btnEliminar = New Button()
        btnActualizar = New Button()
        lblEstado = New Label()
        CType(grdPersonas, System.ComponentModel.ISupportInitialize).BeginInit()
        SuspendLayout()

        lblTitulo.Text = "Administración de personas"
        lblTitulo.Font = New Font("Segoe UI", 16.0F, FontStyle.Bold)
        lblTitulo.Location = New Point(24, 20)
        lblTitulo.Size = New Size(650, 36)
        lblNombre.Text = "Nombre"
        lblNombre.Location = New Point(24, 76)
        lblNombre.AutoSize = True
        txtNombre.Name = "txtNombre"
        txtNombre.Location = New Point(24, 100)
        txtNombre.Size = New Size(712, 25)
        txtNombre.MaxLength = 150
        txtNombre.Anchor = AnchorStyles.Top Or AnchorStyles.Left Or AnchorStyles.Right
        txtNombre.TabIndex = 0

        btnGuardar.Text = "Crear"
        btnGuardar.Location = New Point(24, 145)
        btnGuardar.Size = New Size(150, 34)
        btnGuardar.TabIndex = 1
        btnNuevo.Text = "Nuevo / cancelar"
        btnNuevo.Location = New Point(184, 145)
        btnNuevo.Size = New Size(150, 34)
        btnNuevo.TabIndex = 2
        btnEliminar.Text = "Eliminar"
        btnEliminar.Location = New Point(344, 145)
        btnEliminar.Size = New Size(110, 34)
        btnEliminar.Enabled = False
        btnEliminar.TabIndex = 3
        btnActualizar.Text = "Actualizar lista"
        btnActualizar.Location = New Point(464, 145)
        btnActualizar.Size = New Size(140, 34)
        btnActualizar.TabIndex = 4

        grdPersonas.Name = "grdPersonas"
        grdPersonas.Location = New Point(24, 200)
        grdPersonas.Size = New Size(712, 300)
        grdPersonas.Anchor = AnchorStyles.Top Or AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        grdPersonas.ReadOnly = True
        grdPersonas.AllowUserToAddRows = False
        grdPersonas.AllowUserToDeleteRows = False
        grdPersonas.AllowUserToResizeRows = False
        grdPersonas.MultiSelect = False
        grdPersonas.SelectionMode = DataGridViewSelectionMode.FullRowSelect
        grdPersonas.AutoGenerateColumns = False
        grdPersonas.RowHeadersVisible = False
        grdPersonas.BackgroundColor = Color.White
        grdPersonas.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
        grdPersonas.TabIndex = 5
        grdPersonas.Columns.Add(New DataGridViewTextBoxColumn() With {
            .Name = "colId", .HeaderText = "Id", .DataPropertyName = "Id", .FillWeight = 15.0F})
        grdPersonas.Columns.Add(New DataGridViewTextBoxColumn() With {
            .Name = "colNombre", .HeaderText = "Nombre", .DataPropertyName = "Nombre", .FillWeight = 85.0F})

        lblEstado.Text = "Selecciona una persona para editarla o eliminarla."
        lblEstado.Location = New Point(24, 515)
        lblEstado.Size = New Size(712, 24)
        lblEstado.Anchor = AnchorStyles.Bottom Or AnchorStyles.Left Or AnchorStyles.Right
        lblEstado.AutoEllipsis = True
        AutoScaleMode = AutoScaleMode.Font
        Font = New Font("Segoe UI", 10.0F)
        ClientSize = New Size(760, 560)
        MinimumSize = New Size(690, 480)
        StartPosition = FormStartPosition.CenterScreen
        Text = "CRUD Personas — VB.NET / EF6"
        AcceptButton = btnGuardar
        Controls.AddRange(New Control() {lblTitulo, lblNombre, txtNombre, btnGuardar, btnNuevo,
                                         btnEliminar, btnActualizar, grdPersonas, lblEstado})
        CType(grdPersonas, System.ComponentModel.ISupportInitialize).EndInit()
        ResumeLayout(False)
        PerformLayout()
    End Sub
End Class
