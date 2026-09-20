using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantNursery.Domain.Entities;

namespace PlantNursery.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("OrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        // OrderItem -> Plant
        builder.HasOne(x => x.Plant)
            .WithMany()
            .HasForeignKey(x => x.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.PlantId);
    }
}