namespace FunEvents.Infrastructure.Sql.Contexts;

public sealed class FunEventsDbContext : DbContext
{
    private readonly ICurrentUser _currentUser;
    private const string UnauthorizedUser = "Unauthorized";

    public FunEventsDbContext(DbContextOptions<FunEventsDbContext> options, ICurrentUser currentUser)
        : base(options)
    {
        _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
        ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FunEventsDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var currentUser = _currentUser.UserName ?? UnauthorizedUser;

        foreach (var entry in ChangeTracker.Entries<AuditableTable>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUser;
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.ModifiedBy = currentUser;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedBy = currentUser;
                    entry.Entity.ModifiedAt = DateTime.UtcNow;
                    entry.Property(x => x.CreatedBy).IsModified = false;
                    entry.Property(x => x.CreatedAt).IsModified = false;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}