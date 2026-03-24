using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Persistence.Configurations;

public sealed class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.Property(x => x.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TaxAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountAmount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");
        builder.Property(x => x.PaidAmount).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.BranchId, x.OrderNumber }).IsUnique();

        builder.HasOne(x => x.CashierShift)
            .WithMany(x => x.SalesOrders)
            .HasForeignKey(x => x.CashierShiftId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(x => x.Branch)
    .WithMany()
    .HasForeignKey(x => x.BranchId)
    .OnDelete(DeleteBehavior.Restrict); 
    }
}
