namespace FunEvents.Infrastructure.Sql.Configurations.Base;

internal static class AuditableTableConfiguration
{
    public static void Configure<T>(EntityTypeBuilder<T> builder)
        where T : AuditableTable
    {
        builder.Property(x => x.IsActive)
            .HasColumnName("isActive")
            .HasColumnType("bit")
            .IsRequired()
            .HasColumnOrder(1000);

        builder.Property(x => x.IsDeleted)
            .HasColumnName("isDeleted")
            .HasColumnType("bit")
            .IsRequired()
            .HasColumnOrder(1001);

        builder.Property(x => x.CreatedBy)
            .HasColumnName("createdBy")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(1002);

        builder.Property(x => x.CreatedAt)
            .HasColumnName("createdAt")
            .HasColumnType("datetime2")
            .IsRequired()
            .HasColumnOrder(1003);

        builder.Property(x => x.ModifiedBy)
            .HasColumnName("modifiedBy")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(1004);

        builder.Property(x => x.ModifiedAt)
            .HasColumnName("modifiedAt")
            .HasColumnType("datetime2")
            .IsRequired()
            .HasColumnOrder(1005);

        builder.Property(x => x.Remarks)
            .HasColumnName("remarks")
            .HasColumnType("varchar(500)")
            .HasMaxLength(500)
            .IsRequired()
            .HasColumnOrder(1006);
    }
}