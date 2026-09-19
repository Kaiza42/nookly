using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nookly.Domain.Members;

namespace Nookly.Infrastructure.Data.Configurations;

internal sealed class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");
        builder.HasKey(member => member.Id);
        builder.Property(member => member.Email).HasMaxLength(320).IsRequired();
        builder.Property(member => member.DisplayName).HasMaxLength(50).IsRequired();
        builder.Property(member => member.PublicId).HasMaxLength(10).IsRequired();
        builder.Property(member => member.PasswordHash).HasMaxLength(1000).IsRequired();
        builder.Property(member => member.Role).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.HasIndex(member => member.Email).IsUnique();
        builder.HasIndex(member => member.PublicId).IsUnique();
    }
}
