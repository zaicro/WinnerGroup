namespace FunEvents.Infrastructure.Sql.Configurations;

internal sealed class TbEventConfiguration : IEntityTypeConfiguration<TbEvent>
{
    public void Configure(EntityTypeBuilder<TbEvent> builder)
    {
        builder.ToTable("TbEvents");

        builder.Property(x => x.Id)
            .HasColumnOrder(0);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .HasColumnName("code")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(1);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .HasColumnName("name")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(x => x.EventDate)
            .HasColumnName("eventDate")
            .HasColumnType("datetime")
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(x => x.Capacity)
            .HasColumnName("capacity")
            .HasColumnType("int")
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(x => x.AvailableCapacity)
            .HasColumnName("availableCapacity")
            .HasColumnType("int")
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("int")
            .IsRequired()
            .HasColumnOrder(6);

        builder.HasMany(x => x.Reservations)
            .WithOne(x => x.Event)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        AuditableTableConfiguration.Configure(builder);
    }
}