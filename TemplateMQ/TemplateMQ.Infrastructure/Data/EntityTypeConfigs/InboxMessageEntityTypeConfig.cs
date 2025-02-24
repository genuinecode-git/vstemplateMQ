namespace TemplateMQ.Infrastructure.Data.EntityTypeConfigs;

public class InboxMessageConfiguration : IEntityTypeConfiguration<InboxMessage>
{
    public void Configure(EntityTypeBuilder<InboxMessage> builder)
    {

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Id)
            .IsRequired()
            .ValueGeneratedNever(); 

        builder.Property(m => m.MessageType)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(m => m.Payload)
            .IsRequired()
            .HasColumnType("nvarchar(max)");

        builder.Property(m => m.ReceivedAt)
            .IsRequired();

        builder.Property(m => m.ProcessedAt)
            .IsRequired(false);

        builder.HasIndex(m => m.ProcessedAt);

        builder.Property(m => m.RetryCount)
           .IsRequired()
           .HasDefaultValue(0);

        builder.Property(m => m.ErrorMessage)
            .HasColumnType("nvarchar(4000)");

        builder.ToTable("InboxMessages");

    }
}
