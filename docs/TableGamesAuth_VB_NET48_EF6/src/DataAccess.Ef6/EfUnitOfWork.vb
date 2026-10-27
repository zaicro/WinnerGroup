Imports System.Data.Entity
Imports System.Data.Entity.Infrastructure
Imports System.Threading
Imports System.Threading.Tasks
Imports TableGamesAuth.Application
Imports TableGamesAuth.Domain

Namespace TableGamesAuth.DataAccess.Ef6
    Public Class EfUnitOfWork
        Implements IUnitOfWork
        Private ReadOnly db As AuthDbContext
        Private transactionOpen As Boolean
        Private ReadOnly logger As IAppLogger
        Public Sub New(db As AuthDbContext, Optional logger As IAppLogger = Nothing)
            Me.db = db
            Me.logger = If(logger, New NullAppLogger())
            Users = New UserRepository(db)
            Roles = New RoleRepository(db)
            Apps = New AppRepository(db)
            UserApps = New UserAppRepository(db)
        End Sub
        Public ReadOnly Property Users As IUserRepository Implements IUnitOfWork.Users
        Public ReadOnly Property Roles As IRoleRepository Implements IUnitOfWork.Roles
        Public ReadOnly Property Apps As IAppRepository Implements IUnitOfWork.Apps
        Public ReadOnly Property UserApps As IUserAppRepository Implements IUnitOfWork.UserApps
        Public Function BeginTransaction() As ITransaction Implements IUnitOfWork.BeginTransaction
            If transactionOpen Then Throw New InvalidOperationException("No se admiten transacciones anidadas.")
            Dim transaction = db.Database.BeginTransaction(System.Data.IsolationLevel.ReadCommitted)
            transactionOpen = True
            Return New EfTransaction(transaction, Sub() transactionOpen = False)
        End Function
        Public Async Function SaveAsync(ct As CancellationToken) As Task Implements IUnitOfWork.SaveAsync
            If Not transactionOpen Then Throw New InvalidOperationException("Save requiere una transacción explícita.")
            For Each entry In db.ChangeTracker.Entries()
                If entry.State = EntityState.Modified OrElse entry.State = EntityState.Added Then CType(entry.Entity, IEntity).Version = Guid.NewGuid()
            Next
            Try
                Await db.SaveChangesAsync(ct)
            Catch ex As DbUpdateConcurrencyException
                Throw New AppException(409, "Conflicto concurrente. Recargue los datos.")
            Catch ex As DbUpdateException
                ' Details stay in internal logs, never in HTTP responses.
                logger.Error("database_constraint_failure", ex)
                Throw New AppException(409, "La operación viola una restricción de datos.")
            End Try
        End Function
        Public Sub Dispose() Implements IDisposable.Dispose
            db.Dispose()
        End Sub
    End Class
    Friend Class EfTransaction
        Implements ITransaction
        Private ReadOnly transaction As DbContextTransaction
        Private ReadOnly finished As Action
        Private completed As Boolean
        Public Sub New(transaction As DbContextTransaction, finished As Action)
            Me.transaction = transaction : Me.finished = finished
        End Sub
        Public Sub Commit() Implements ITransaction.Commit
            transaction.Commit()
            completed = True
        End Sub
        Public Sub Rollback() Implements ITransaction.Rollback
            If Not completed Then transaction.Rollback()
            completed = True
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            Try
                If Not completed Then transaction.Rollback()
            Finally
                transaction.Dispose()
                finished()
            End Try
            ' Discard the entire request scope after rollback; tracked entities are not reset.
        End Sub
    End Class
End Namespace
