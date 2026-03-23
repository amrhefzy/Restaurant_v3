using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Persistence.Configurations;

public sealed class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
    {
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ReferenceNumber).HasMaxLength(80);

        builder.HasIndex(x => new { x.BranchId, x.PaidOnUtc });
        builder.HasIndex(x => new { x.SalesOrderId, x.CashierShiftId });

        builder.HasOne(x => x.SalesOrder)
            .WithMany(x => x.PaymentTransactions)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CashierShift)
            .WithMany(x => x.PaymentTransactions)
            .HasForeignKey(x => x.CashierShiftId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
