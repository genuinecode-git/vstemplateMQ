using System;
namespace TemplateMQ.Infrastructure.Data.EntityTypeConfigs;

internal class OutboxMessageEntityTypeConfig : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {

        builder.HasKey(o => o.Id);

        builder.Property(o => o.MessageType)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(o => o.Payload)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.ProcessedAt)
            .IsRequired(false);

        builder.Property(o => o.RetryCount)
            .HasDefaultValue(0);

        builder.ToTable("OutboxMessages");

    }
}
