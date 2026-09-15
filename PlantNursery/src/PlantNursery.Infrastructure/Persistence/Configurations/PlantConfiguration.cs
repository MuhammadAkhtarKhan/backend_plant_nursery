using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantNursery.Domain.Entities;

namespace PlantNursery.Infrastructure.Persistence.Configurations;

public class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.ToTable("Plants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .HasMaxLength(2000);

        builder.Property(x => x.Price)
            .HasPrecision(18, 2);

        builder.Property(x => x.StockQuantity)
            .IsRequired();

        builder.Property(x => x.ScientificName)
            .HasMaxLength(200);

        builder.Property(x => x.CareInstructions)
            .HasMaxLength(2000);

        builder.Property(x => x.SunlightRequirement)
            .HasMaxLength(200);

        builder.Property(x => x.WateringFrequency)
            .HasMaxLength(200);

        builder.Property(x => x.Size)
            .HasMaxLength(100);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Plants)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.CategoryId);
    }
}