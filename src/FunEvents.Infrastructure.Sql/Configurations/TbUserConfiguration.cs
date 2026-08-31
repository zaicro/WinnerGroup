namespace FunEvents.Infrastructure.Sql.Configurations;

internal sealed class TbUserConfiguration : IEntityTypeConfiguration<TbUser>
{
    public void Configure(EntityTypeBuilder<TbUser> builder)
    {
        builder.ToTable("TbUsers");

        builder.Property(x => x.Id)
            .HasColumnOrder(0);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .HasColumnName("username")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(1);

        builder.HasIndex(x => x.Username)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(150)")
            .HasMaxLength(150)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(x => x.Email)
            .HasColumnName("email")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(x => x.Phone)
            .HasColumnName("phone")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(x => x.PasswordHash)
            .HasColumnName("passwordHash")
            .HasColumnType("varchar(100)")
            .HasMaxLength(100)
            .IsRequired()
            .HasColumnOrder(5);

        builder.HasMany(x => x.Reservations)
            .WithOne(x => x.User)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        AuditableTableConfiguration.Configure(builder);
    }
}