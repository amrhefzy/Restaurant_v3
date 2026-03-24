using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Persistence.Configurations;

public sealed class CashierShiftConfiguration : IEntityTypeConfiguration<CashierShift>
{
    public void Configure(EntityTypeBuilder<CashierShift> builder)
    {
        builder.Property(x => x.ShiftNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.OpenedByUserId).HasMaxLength(128);
        builder.Property(x => x.OpenedByUserName).HasMaxLength(256);
        builder.Property(x => x.ClosedByUserId).HasMaxLength(128);
        builder.Property(x => x.ClosedByUserName).HasMaxLength(256);

        builder.Property(x => x.OpeningCash).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ClosingCashActual).HasColumnType("decimal(18,2)");
        builder.Property(x => x.ClosingCashExpected).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalSales).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCash).HasColumnType("decimal(18,2)");
        builder.Property(x => x.TotalCard).HasColumnType("decimal(18,2)");
        builder.Property(x => x.CashVariance).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.BranchId, x.ShiftNumber }).IsUnique();
        builder.HasIndex(x => new { x.BranchId, x.Status });

        builder.HasOne(x => x.Branch)
            .WithMany(x => x.CashierShifts)
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
