namespace FunEvents.Infrastructure.Sql.Contexts;

//add-migration Init -p FunEvents.Infrastructure.Sql -c FunEventsDbContext -o Migrations -s FunEvents.Infrastructure.Sql -verbose
//update-database -p FunEvents.Infrastructure.Sql -s FunEvents.Infrastructure.Sql -verbose
//remove-migration -p FunEvents.Infrastructure.Sql -s FunEvents.Infrastructure.Sql
internal sealed class FunEventsDbContextFactory : IDesignTimeDbContextFactory<FunEventsDbContext>
{
    public FunEventsDbContext CreateDbContext(string[] args)
    {
        try
        {
            var optionsBuilder = new DbContextOptionsBuilder<FunEventsDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=DESKTOP-T0SDTHB;" +
                "Initial Catalog=FunEvents;" +
                "Integrated Security=True;" +
                "TrustServerCertificate=True;");

            return new FunEventsDbContext(optionsBuilder.Options, new DesignTimeCurrentUser());
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }
}

internal sealed class DesignTimeCurrentUser : ICurrentUser
{
    public string? UserName => "Migration";

    public string? ClientId => null;

    public bool IsAuthenticated => false;
}