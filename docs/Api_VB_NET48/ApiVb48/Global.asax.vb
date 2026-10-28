Imports System.Web
Imports System.Web.Http

Public Class WebApiApplication
    Inherits HttpApplication

    Protected Sub Application_Start()
        Security.AuthSettings.Validate()
        GlobalConfiguration.Configure(AddressOf App_Start.WebApiConfig.Register)
    End Sub
End Class
