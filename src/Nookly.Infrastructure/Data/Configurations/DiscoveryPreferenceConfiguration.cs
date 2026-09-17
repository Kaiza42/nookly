using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nookly.Domain.Discovery;

namespace Nookly.Infrastructure.Data.Configurations;

internal sealed class DiscoveryPreferenceConfiguration : IEntityTypeConfiguration<DiscoveryPreference>
{
    public void Configure(EntityTypeBuilder<DiscoveryPreference> builder)
    {
        builder.ToTable("discovery_preferences");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ExternalSource).HasMaxLength(50).IsRequired();
        builder.Property(item => item.ExternalId).HasMaxLength(100).IsRequired();
        builder.Property(item => item.Title).HasMaxLength(300).IsRequired();
        builder.Property(item => item.MediaType).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(item => new { item.ExternalSource, item.ExternalId }).IsUnique();
    }
}
