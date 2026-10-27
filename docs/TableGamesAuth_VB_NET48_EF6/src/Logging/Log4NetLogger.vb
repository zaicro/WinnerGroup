Imports System.IO
Imports log4net
Imports log4net.Config
Imports TableGamesAuth.Application

Namespace TableGamesAuth.Logging
    Public Class Log4NetLogger
        Implements IAppLogger
        Private ReadOnly log As ILog = LogManager.GetLogger(GetType(Log4NetLogger))
        Public Shared Sub Configure(configPath As String)
            If Not File.Exists(configPath) Then Throw New InvalidOperationException("Falta log4net.config.")
            XmlConfigurator.Configure(New FileInfo(configPath))
        End Sub
        Public Sub Info(message As String) Implements IAppLogger.Info
            log.Info(message)
        End Sub
        Public Sub Warn(message As String) Implements IAppLogger.Warn
            log.Warn(message)
        End Sub
        Public Sub [Error](eventName As String, exception As Exception) Implements IAppLogger.Error
            ' Provider exception messages can contain SQL values, connection details or credentials.
            ' Log the type and stack only. Never pass the raw exception to log4net.
            log.Error(eventName & " type=" & exception.GetType().FullName & " stack=" & exception.StackTrace)
        End Sub
    End Class
End Namespace
