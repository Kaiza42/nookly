using Nookly.Domain.Banking;

namespace Nookly.Domain.Tests.Banking;

public sealed class BankingTests
{
    [Fact]
    public void Account_StoresStartingBalance()
    {
        var account = BankAccount.Create(Guid.NewGuid());
        account.SetStartingBalance(2000m);
        Assert.Equal(2000m, account.StartingBalance);
    }

    [Fact]
    public void Entry_RequiresLabelAndNonZeroAmount()
    {
        var memberId = Guid.NewGuid();
        Assert.Equal(200m, BankEntry.Create(memberId, "Salaire", 200m).Amount);
        Assert.Equal(-200m, BankEntry.Create(memberId, "Courses", -200m).Amount);
        Assert.Throws<ArgumentException>(() => BankEntry.Create(memberId, " ", 10m));
        Assert.Throws<ArgumentOutOfRangeException>(() => BankEntry.Create(memberId, "Vide", 0m));
    }
}
