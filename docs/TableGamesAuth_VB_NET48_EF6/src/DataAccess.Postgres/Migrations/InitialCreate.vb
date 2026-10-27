Imports System.Data.Entity.Migrations.Infrastructure
Imports TableGamesAuth.DataAccess.Ef6

Namespace TableGamesAuth.DataAccess.Postgres.Migrations
    Public Class InitialCreate
        Inherits InitialSchema
        Implements IMigrationMetadata
        Private Shared ReadOnly snapshot As New Lazy(Of String)(Function() MigrationSnapshot.Build("Npgsql", "9.6"))
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
