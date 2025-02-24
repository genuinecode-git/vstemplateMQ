
namespace TemplateMQ.Infrastructure.Data.EntityTypeConfigs;

public class SampleEntityTypeConfig : IEntityTypeConfiguration<Sample>
{
    public void Configure(EntityTypeBuilder<Sample> builder)
    {
        // Define the primary key
        builder.HasKey(s => s.Id);

        // Configure the properties
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd(); // Auto-increment (if using int)

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Define table name (optional)
        builder.ToTable("Samples");
    }
}
