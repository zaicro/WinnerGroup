using FunEvents.Infrastructure.Sql.Contexts;
using FunEvents.Infrastructure.Sql.Tables;
using Microsoft.EntityFrameworkCore;

namespace FunEvents.Infrastructure.Sql.Tests.Contexts;

[TestFixture]
public class FunEventsDbContextTests
{
    private FunEventsDbContext _context = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<FunEventsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new FunEventsDbContext(options, new DesignTimeCurrentUser());
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task SaveChangesAsync_WhenAddingEntity_ShouldSetAuditFields()
    {
        var user = new TbUser
        {
            Username = "USR-001",
            Name = "Test User",
            Email = "test@funevents.com",
            Phone = "1234567890",
            PasswordHash = "hashedpassword"
        };

        _context.Set<TbUser>().Add(user);

        await _context.SaveChangesAsync();

        Assert.Multiple(() =>
        {
            Assert.That(user.CreatedAt, Is.Not.EqualTo(default(DateTime)));
            Assert.That(user.ModifiedAt, Is.Not.EqualTo(default(DateTime)));

            Assert.That(user.CreatedBy, Is.EqualTo("Migration"));
            Assert.That(user.ModifiedBy, Is.EqualTo("Migration"));
        });
    }
}