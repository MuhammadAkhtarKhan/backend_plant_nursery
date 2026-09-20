using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PlantNursery.Domain.Entities;

namespace PlantNursery.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.OrderNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.OrderType)
            .IsRequired();

        builder.Property(x => x.UserId)
            .IsRequired();

        builder.Property(x => x.TotalAmount)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .IsRequired();

        builder.Property(x => x.PaymentStatus)
            .IsRequired();

        builder.Property(x => x.OrderStatus)
            .IsRequired();

        // An order number must be unique.
        builder.HasIndex(x => x.OrderNumber)
            .IsUnique();

        // Useful when retrieving a user's orders.
        builder.HasIndex(x => x.UserId);

        // Useful for filtering online/POS orders.
        builder.HasIndex(x => x.OrderType);

        // Order -> OrderItems
        builder.HasMany(x => x.Items)
            .WithOne(x => x.Order)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}