using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Persistence.Configurations;

public sealed class InventoryMovementConfiguration : IEntityTypeConfiguration<InventoryMovement>
{
    public void Configure(EntityTypeBuilder<InventoryMovement> builder)
    {
        builder.Property(x => x.QuantityChange).HasColumnType("decimal(18,3)");
        builder.Property(x => x.ReferenceNumber).HasMaxLength(60).IsRequired();
        builder.Property(x => x.Reason).HasMaxLength(400);

        builder.HasIndex(x => new { x.BranchId, x.ProductId, x.CreatedOn });
        builder.HasIndex(x => new { x.BranchId, x.MovementType, x.CreatedOn });

        builder.HasOne(x => x.Product)
            .WithMany(x => x.InventoryMovements)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
