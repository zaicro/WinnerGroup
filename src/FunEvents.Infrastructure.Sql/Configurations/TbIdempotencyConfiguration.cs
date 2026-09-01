namespace FunEvents.Infrastructure.Sql.Configurations;

internal sealed class TbIdempotencyConfiguration : IEntityTypeConfiguration<TbIdempotency>
{
    public void Configure(EntityTypeBuilder<TbIdempotency> builder)
    {
        builder.ToTable("TbIdempotency");

        builder.Property(x => x.Id)
            .HasColumnOrder(0);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Key)
            .HasColumnName("key")
            .HasColumnType("varchar(50)")
            .HasMaxLength(50)
            .IsRequired()
            .HasColumnOrder(1);

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.Property(x => x.TableName)
            .HasColumnName("tableName")
            .HasColumnType("varchar(200)")
            .HasMaxLength(200)
            .IsRequired()
            .HasColumnOrder(2);

        builder.Property(x => x.TableKeyValue)
            .HasColumnName("tableKeyValue")
            .HasColumnType("bigint")
            .IsRequired()
            .HasColumnOrder(3);

        AuditableTableConfiguration.Configure(builder);
    }
}