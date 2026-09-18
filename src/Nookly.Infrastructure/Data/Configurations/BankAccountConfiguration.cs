using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nookly.Domain.Banking;
using Nookly.Domain.Members;

namespace Nookly.Infrastructure.Data.Configurations;

internal sealed class BankAccountConfiguration : IEntityTypeConfiguration<BankAccount>
{
    public void Configure(EntityTypeBuilder<BankAccount> builder)
    {
        builder.ToTable("bank_accounts"); builder.HasKey(item => item.Id);
        builder.Property(item => item.StartingBalance).HasPrecision(18, 2);
        builder.HasIndex(item => item.MemberId).IsUnique();
        builder.HasOne<Member>().WithMany().HasForeignKey(item => item.MemberId).OnDelete(DeleteBehavior.Cascade);
    }
}
