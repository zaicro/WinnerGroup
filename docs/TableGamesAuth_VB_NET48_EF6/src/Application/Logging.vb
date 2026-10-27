Namespace TableGamesAuth.Application
    Public Interface IAppLogger
        Sub Info(message As String)
        Sub Warn(message As String)
        Sub [Error](eventName As String, exception As Exception)
    End Interface
    Public Class NullAppLogger
        Implements IAppLogger
        Public Sub Info(message As String) Implements IAppLogger.Info
        End Sub
        Public Sub Warn(message As String) Implements IAppLogger.Warn
        End Sub
        Public Sub [Error](eventName As String, exception As Exception) Implements IAppLogger.Error
        End Sub
    End Class
End Namespace
