Imports System.IO
Imports System.Threading
Imports System.Windows.Forms
Imports CrudPersonas.Application
Imports CrudPersonas.DataAccess

Friend Module Program
    <STAThread>
    Public Sub Main()
        System.Windows.Forms.Application.EnableVisualStyles()
        System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(False)

        ' Una instancia por sesión: impide dos migraciones desde esta aplicación a la vez.
        Dim esPrimeraInstancia As Boolean
        Using instancia As New Mutex(True, "Local\CrudPersonas.UI", esPrimeraInstancia)
            If Not esPrimeraInstancia Then
                MessageBox.Show("La aplicación ya está abierta en esta sesión.", "Personas",
                                MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If
            Try
                DatabaseInitializer.Actualizar()
                Dim servicio As New PersonaService(New PersonaRepository())
                System.Windows.Forms.Application.Run(New MainForm(servicio))
            Catch ex As Exception
                RegistrarError(ex)
                MessageBox.Show("No fue posible iniciar la aplicación." & Environment.NewLine &
                                "Comprueba que SQL Server LocalDB esté instalado." & Environment.NewLine &
                                "Detalle: " & ex.GetBaseException().Message & Environment.NewLine &
                                "Registro: " & RutaRegistro(),
                                "Error de inicio", MessageBoxButtons.OK, MessageBoxIcon.Error)
            Finally
                instancia.ReleaseMutex()
            End Try
        End Using
    End Sub

    Friend Sub RegistrarError(ex As Exception)
        Try
            Dim ruta = RutaRegistro()
            Directory.CreateDirectory(Path.GetDirectoryName(ruta))
            File.AppendAllText(ruta, DateTime.Now.ToString("s") & Environment.NewLine &
                               ex.ToString() & Environment.NewLine & Environment.NewLine)
        Catch
            ' Un fallo de diagnóstico no debe ocultar el error original.
        End Try
    End Sub

    Private Function RutaRegistro() As String
        Return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                            "CrudPersonas", "Logs", "errores.log")
    End Function
End Module
