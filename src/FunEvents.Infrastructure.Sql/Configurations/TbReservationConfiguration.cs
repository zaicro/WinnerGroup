namespace FunEvents.Infrastructure.Sql.Configurations;

internal sealed class TbReservationConfiguration : IEntityTypeConfiguration<TbReservation>
{
    public void Configure(EntityTypeBuilder<TbReservation> builder)
    {
        builder.ToTable("TbReservations");

        builder.Property(x => x.Id)
            .HasColumnOrder(0);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ReservationCode)
            .HasColumnName("reservationCode")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(1);

        builder.HasIndex(x => x.ReservationCode)
            .IsUnique();

        builder.Property(x => x.UserId)
            .HasColumnName("userId")
            .HasColumnType("int")
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(x => x.EventId)
            .HasColumnName("eventId")
            .HasColumnType("int")
            .IsRequired()
            .HasColumnOrder(3);

        builder.Property(x => x.Quantity)
            .HasColumnName("quantity")
            .HasColumnType("int")
            .IsRequired()
            .HasColumnOrder(4);

        builder.Property(x => x.Channel)
            .HasColumnName("channel")
            .HasColumnType("int")
            .IsRequired()
            .HasColumnOrder(5);

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasColumnType("int")
            .IsRequired()
            .HasColumnOrder(6);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Event)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Restrict);

        AuditableTableConfiguration.Configure(builder);
    }
}