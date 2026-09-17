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
        Assert.Throws<ArgumentException>(() => Member.Create("test@nookly.test", " "));
    }
}
