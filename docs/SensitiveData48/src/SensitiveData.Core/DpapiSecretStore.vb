Imports System.IO
Imports System.Security.AccessControl
Imports System.Security.Cryptography
Imports System.Security.Principal
Imports System.Text
Imports System.Text.RegularExpressions
Imports System.Xml.Linq

' Alternativa local para Windows: solo el mismo usuario de Windows puede descifrar.
' No usa LocalMachine ni guarda una llave maestra propia junto al archivo.
Public NotInheritable Class DpapiSecretStore
    Implements ISecretStore
    Private ReadOnly _directory As String
    Public Sub New(directory As String)
        _directory = Path.GetFullPath(directory)
    End Sub
    Private Function FilePath(name As String) As String
        If name Is Nothing OrElse Not Regex.IsMatch(name, "\A[a-zA-Z0-9-]{1,100}\z") Then
            Throw New ArgumentException("Nombre de secreto inválido.")
        End If
        Return Path.Combine(_directory, name & ".secret.xml")
    End Function
    Private Shared Function Entropy(name As String) As Byte()
        ' Contexto público de propósito. No es una contraseña ni una llave de cifrado.
        Return Encoding.UTF8.GetBytes("SensitiveData48/v1/" & name)
    End Function
    Public Function GetSecret(name As String) As SecretValue Implements ISecretStore.GetSecret
        Dim path = FilePath(name)
        Dim info As New FileInfo(path)
        If Not info.Exists Then Throw New FileNotFoundException("Falta un secreto local requerido.")
        If info.Length > 131072 Then Throw New InvalidDataException("Archivo de secreto demasiado grande.")
        Dim document = XDocument.Load(path)
        Dim root = document.Root
        If root Is Nothing OrElse root.Name <> XName.Get("protectedSecret") OrElse
            CStr(root.Attribute("format")) <> "1" Then Throw New InvalidDataException("Formato de secreto inválido.")
        Dim encrypted = Convert.FromBase64String(root.Value)
        Dim plaintext = ProtectedData.Unprotect(encrypted, Entropy(name), DataProtectionScope.CurrentUser)
        Try
            Return New SecretValue(Encoding.UTF8.GetString(plaintext), CStr(root.Attribute("version")))
        Finally
            Array.Clear(plaintext, 0, plaintext.Length)
        End Try
    End Function
    Public Sub PutSecret(name As String, value As String, Optional replace As Boolean = False)
        If String.IsNullOrWhiteSpace(value) OrElse value.Length > 16384 Then Throw New ArgumentException("Valor vacío o demasiado largo.")
        Dim target = FilePath(name)
        Dim sid = WindowsIdentity.GetCurrent().User
        Dim acl As New DirectorySecurity()
        acl.SetAccessRuleProtection(True, False)
        acl.SetOwner(sid)
        Dim inheritance = InheritanceFlags.ContainerInherit Or InheritanceFlags.ObjectInherit
        acl.AddAccessRule(New FileSystemAccessRule(sid, FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow))
        acl.AddAccessRule(New FileSystemAccessRule(New SecurityIdentifier(WellKnownSidType.LocalSystemSid, Nothing), FileSystemRights.FullControl, inheritance, PropagationFlags.None, AccessControlType.Allow))
        ' Carpeta dedicada: si existe, también se restablecen sus ACL al aprovisionar.
        System.IO.Directory.CreateDirectory(_directory, acl)
        System.IO.Directory.SetAccessControl(_directory, acl)
        If File.Exists(target) AndAlso Not replace Then Throw New IOException("El secreto ya existe. Use replace explícitamente para cambiarlo.")
        Dim plaintext = Encoding.UTF8.GetBytes(value)
        Dim encrypted As Byte()
        Try
            encrypted = ProtectedData.Protect(plaintext, Entropy(name), DataProtectionScope.CurrentUser)
        Finally
            Array.Clear(plaintext, 0, plaintext.Length)
        End Try
        Dim doc As New XDocument(New XElement("protectedSecret",
            New XAttribute("format", "1"), New XAttribute("version", Guid.NewGuid().ToString("N")), Convert.ToBase64String(encrypted)))
        Dim temporary = Path.Combine(_directory, Guid.NewGuid().ToString("N") & ".tmp")
        Try
            Using stream As New FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None)
                doc.Save(stream)
                stream.Flush(True)
            End Using
            If replace AndAlso File.Exists(target) Then
                File.Replace(temporary, target, Nothing)
            Else
                File.Move(temporary, target)
            End If
        Finally
            If File.Exists(temporary) Then File.Delete(temporary)
        End Try
    End Sub
End Class
