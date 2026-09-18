using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nookly.Domain.Banking;
using Nookly.Domain.Members;

namespace Nookly.Infrastructure.Data.Configurations;

internal sealed class BankEntryConfiguration : IEntityTypeConfiguration<BankEntry>
{
    public void Configure(EntityTypeBuilder<BankEntry> builder)
    {
        builder.ToTable("bank_entries"); builder.HasKey(item => item.Id);
        builder.Property(item => item.Label).HasMaxLength(200).IsRequired();
        builder.Property(item => item.Amount).HasPrecision(18, 2);
        builder.HasIndex(item => new { item.MemberId, item.CreatedAtUtc });
        builder.HasOne<Member>().WithMany().HasForeignKey(item => item.MemberId).OnDelete(DeleteBehavior.Cascade);
    }
}
