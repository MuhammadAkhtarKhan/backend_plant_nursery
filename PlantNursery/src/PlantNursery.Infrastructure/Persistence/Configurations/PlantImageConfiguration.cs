using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantNursery.Domain.Entities;

namespace PlantNursery.Infrastructure.Persistence.Configurations;

public class PlantImageConfiguration : IEntityTypeConfiguration<PlantImage>
{
    public void Configure(EntityTypeBuilder<PlantImage> builder)
    {
        builder.ToTable("PlantImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.IsPrimary)
            .IsRequired();

        builder.HasOne(x => x.Plant)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.PlantId);
    }
}