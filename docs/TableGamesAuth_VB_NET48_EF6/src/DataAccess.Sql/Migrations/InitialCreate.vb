Imports System.Data.Entity.Migrations.Infrastructure
Imports TableGamesAuth.DataAccess.Ef6

Namespace TableGamesAuth.DataAccess.Sql.Migrations
    Public Class InitialCreate
        Inherits InitialSchema
        Implements IMigrationMetadata
        Private Shared ReadOnly snapshot As New Lazy(Of String)(Function() MigrationSnapshot.Build("System.Data.SqlClient", "2012"))
        Private ReadOnly Property Id As String Implements IMigrationMetadata.Id
            Get
                Return "202610260001000_InitialCreate"
            End Get
        End Property
        Private ReadOnly Property Source As String Implements IMigrationMetadata.Source
            Get
                Return Nothing
            End Get
        End Property
        Private ReadOnly Property Target As String Implements IMigrationMetadata.Target
            Get
                Return snapshot.Value
            End Get
        End Property
    End Class
End Namespace
