using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nookly.Domain.Discovery;
using Nookly.Domain.Members;

namespace Nookly.Infrastructure.Data.Configurations;

internal sealed class DiscoveryHistoryEntryConfiguration : IEntityTypeConfiguration<DiscoveryHistoryEntry>
{
    public void Configure(EntityTypeBuilder<DiscoveryHistoryEntry> builder)
    {
        builder.ToTable("discovery_history");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ExternalSource).HasMaxLength(50).IsRequired();
        builder.Property(item => item.ExternalId).HasMaxLength(100).IsRequired();
        builder.Property(item => item.MediaType).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(item => item.Title).HasMaxLength(300).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(4000);
        builder.Property(item => item.PosterUrl).HasMaxLength(1000);
        builder.Property(item => item.CastLabel).HasMaxLength(1000);
        builder.HasOne<Member>().WithMany().HasForeignKey(item => item.MemberId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(item => new { item.MemberId, item.ExternalSource, item.ExternalId, item.MediaType }).IsUnique();
        builder.HasIndex(item => new { item.MemberId, item.ViewedAtUtc });
    }
}
