using Nookly.Domain.Members;

namespace Nookly.Domain.Tests.Members;

public sealed class MemberTests
{
    [Fact]
    public void Create_NormalizesEmailAndRequiresDisplayName()
    {
        var member = Member.Create("  Alice@Nookly.Test ", " Alice ");
        Assert.Equal("alice@nookly.test", member.Email);
        Assert.Equal("Alice", member.DisplayName);
        Assert.Equal(10, member.PublicId.Length);
        Assert.All(member.PublicId, character => Assert.Contains(character, "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"));
        Assert.Throws<ArgumentException>(() => Member.Create("test@nookly.test", " "));
    }

    [Fact]
    public void Member_CanBeConfirmedAndPromotedWithoutExposingPersonalData()
    {
        var member = Member.Create("admin@nookly.test", "Admin");
        Assert.Equal(MemberRole.Member, member.Role);
        Assert.False(member.IsEmailConfirmed);

        member.ConfirmEmail();
        member.PromoteToAdmin();

        Assert.True(member.IsEmailConfirmed);
        Assert.Equal(MemberRole.Admin, member.Role);
    }

    [Fact]
    public void Activity_CountsOnlyContinuousApplicationUsage()
    {
        var member = Member.Create("alice@nookly.test", "Alice");
        var start = new DateTimeOffset(2026, 9, 18, 8, 0, 0, TimeSpan.Zero);
        member.RecordActivity(start);
        member.RecordActivity(start.AddSeconds(60));
        member.RecordActivity(start.AddMinutes(10));

        Assert.Equal(60, member.UsageSeconds);
    }
}
