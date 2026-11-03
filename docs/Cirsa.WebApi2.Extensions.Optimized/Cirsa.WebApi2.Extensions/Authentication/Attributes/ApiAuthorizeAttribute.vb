Imports System

Namespace Authentication.Attributes

    ''' <summary>Selects accepted credential schemes for an action or controller.</summary>
    <AttributeUsage(AttributeTargets.Class Or AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public NotInheritable Class ApiAuthorizeAttribute
        Inherits Attribute

        ''' <summary>Gets or sets ApiKey, Bearer, or their comma-separated combination.</summary>
        Public Property AuthenticationSchemes As String

        ''' <summary>Requires both credentials when both schemes are selected.</summary>
        Public Property RequireAllSchemes As Boolean

    End Class

End Namespace
