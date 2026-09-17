using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nookly.Domain.Media;

namespace Nookly.Infrastructure.Data.Configurations;

internal sealed class MediaItemConfiguration : IEntityTypeConfiguration<MediaItem>
{
    public void Configure(EntityTypeBuilder<MediaItem> builder)
    {
        builder.ToTable("media_items");

        builder.HasKey(item => item.Id);
        builder.Property(item => item.Title).HasMaxLength(300).IsRequired();
        builder.Property(item => item.Description).HasMaxLength(4000);
        builder.Property(item => item.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(item => item.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(item => item.ExternalSource).HasMaxLength(50);
        builder.Property(item => item.ExternalId).HasMaxLength(100);
        builder.Property(item => item.PosterUrl).HasMaxLength(2000);
        builder.Property(item => item.CommunityRating).HasPrecision(4, 2);

        builder.HasIndex(item => new { item.ExternalSource, item.ExternalId });
    }
}
