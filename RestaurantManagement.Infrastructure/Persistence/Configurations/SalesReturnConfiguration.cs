using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RestaurantManagement.Domain.Entities;

namespace RestaurantManagement.Infrastructure.Persistence.Configurations;

public sealed class SalesReturnConfiguration : IEntityTypeConfiguration<SalesReturn>
{
    public void Configure(EntityTypeBuilder<SalesReturn> builder)
    {
        builder.Property(x => x.ReturnNumber).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Total).HasColumnType("decimal(18,2)");

        builder.HasIndex(x => new { x.BranchId, x.ReturnNumber }).IsUnique();

        builder.HasOne(x => x.Branch)
            .WithMany()
            .HasForeignKey(x => x.BranchId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.SalesOrder)
            .WithMany(x => x.SalesReturns)
            .HasForeignKey(x => x.SalesOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
