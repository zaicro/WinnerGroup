Imports System.Web.Http
Imports System.Web.Http.Controllers

Namespace Security
    Public Enum AuthenticationMode
        None = 0
        ApiKey = 1
        Jwt = 2
        ApiKeyOrJwt = 3
        ApiKeyAndJwt = 4
    End Enum

    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public MustInherit Class AuthenticationPolicyAttribute
        Inherits Attribute

        Public ReadOnly Property Mode As AuthenticationMode

        Protected Sub New(mode As AuthenticationMode)
            Me.Mode = mode
        End Sub
    End Class

    Public NotInheritable Class ApiKeyOnlyAttribute
        Inherits AuthenticationPolicyAttribute
        Public Sub New()
            MyBase.New(AuthenticationMode.ApiKey)
        End Sub
    End Class

    Public NotInheritable Class JwtOnlyAttribute
        Inherits AuthenticationPolicyAttribute
        Public Sub New()
            MyBase.New(AuthenticationMode.Jwt)
        End Sub
    End Class

    Public NotInheritable Class ApiKeyOrJwtAttribute
        Inherits AuthenticationPolicyAttribute
        Public Sub New()
            MyBase.New(AuthenticationMode.ApiKeyOrJwt)
        End Sub
    End Class

    Public NotInheritable Class ApiKeyAndJwtAttribute
        Inherits AuthenticationPolicyAttribute
        Public Sub New()
            MyBase.New(AuthenticationMode.ApiKeyAndJwt)
        End Sub
    End Class

    ' Una sola resolución, compartida por el filtro real y Swagger.
    Public Module AuthenticationPolicy
        Public Function Resolve(action As HttpActionDescriptor) As AuthenticationMode
            If action.GetCustomAttributes(Of AllowAnonymousAttribute)().Any() OrElse
                action.ControllerDescriptor.GetCustomAttributes(Of AllowAnonymousAttribute)().Any() Then
                Return AuthenticationMode.None
            End If
            Dim policies = action.GetCustomAttributes(Of AuthenticationPolicyAttribute)().ToArray()
            If policies.Length = 0 Then
                policies = action.ControllerDescriptor.GetCustomAttributes(Of AuthenticationPolicyAttribute)().ToArray()
            End If
            If policies.Length > 1 Then
                Throw New InvalidOperationException("Use un solo atributo de autenticación por acción o controlador.")
            End If
            If policies.Length = 0 Then Return AuthenticationMode.None
            Return policies(0).Mode
        End Function
    End Module
End Namespace
