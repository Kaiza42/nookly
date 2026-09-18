using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nookly.Domain.Media;
using Nookly.Domain.Members;

namespace Nookly.Infrastructure.Data.Configurations;

internal sealed class EpisodeProgressConfiguration : IEntityTypeConfiguration<EpisodeProgress>
{
    public void Configure(EntityTypeBuilder<EpisodeProgress> builder)
    {
        builder.ToTable("episode_progress");
        builder.HasKey(item => item.Id);
        builder.Property(item => item.ExternalSource).HasMaxLength(50).IsRequired();
        builder.Property(item => item.ExternalId).HasMaxLength(100).IsRequired();
        builder.Property(item => item.PersonalNotes).HasMaxLength(4000);
        builder.HasOne<Member>().WithMany().HasForeignKey(item => item.MemberId).OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(item => new
        {
            item.MemberId,
            item.ExternalSource,
            item.ExternalId,
            item.SeasonNumber,
            item.EpisodeNumber
        }).IsUnique();
    }
}
