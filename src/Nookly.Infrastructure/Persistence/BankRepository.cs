using Microsoft.EntityFrameworkCore;
using Nookly.Application.Abstractions;
using Nookly.Domain.Banking;
using Nookly.Infrastructure.Data;

namespace Nookly.Infrastructure.Persistence;

internal sealed class BankRepository(NooklyDbContext db, ICurrentMember currentMember) : IBankRepository
{
    public Task<BankAccount?> GetAccountAsync(CancellationToken token) =>
        db.BankAccounts.SingleOrDefaultAsync(item => item.MemberId == currentMember.Id && item.Year == DateTimeOffset.UtcNow.Year && item.Month == DateTimeOffset.UtcNow.Month, token);
    public async Task<IReadOnlyList<BankEntry>> ListEntriesAsync(CancellationToken token) =>
        await db.BankEntries.AsNoTracking().Where(item => item.MemberId == currentMember.Id && item.CreatedAtUtc.Year == DateTimeOffset.UtcNow.Year && item.CreatedAtUtc.Month == DateTimeOffset.UtcNow.Month)
            .OrderByDescending(item => item.CreatedAtUtc).ToListAsync(token);
    public async Task<IReadOnlyList<BankAccount>> ListAccountsAsync(CancellationToken token) =>
        await db.BankAccounts.AsNoTracking().Where(item => item.MemberId == currentMember.Id).ToListAsync(token);
    public async Task<IReadOnlyList<BankEntry>> ListAllEntriesAsync(CancellationToken token) =>
        await db.BankEntries.AsNoTracking().Where(item => item.MemberId == currentMember.Id).OrderByDescending(item => item.CreatedAtUtc).ToListAsync(token);
    public async Task SaveAccountAsync(BankAccount account, CancellationToken token)
    {
        if (db.Entry(account).State == EntityState.Detached) db.BankAccounts.Add(account);
        await db.SaveChangesAsync(token);
    }
    public async Task AddEntryAsync(BankEntry entry, CancellationToken token)
    { db.BankEntries.Add(entry); await db.SaveChangesAsync(token); }
    public async Task<bool> DeleteEntryAsync(Guid id, CancellationToken token)
    {
        var entry = await db.BankEntries.SingleOrDefaultAsync(item => item.Id == id && item.MemberId == currentMember.Id, token);
        if (entry is null) return false;
        db.BankEntries.Remove(entry); await db.SaveChangesAsync(token); return true;
    }
    public async Task ResetCurrentMonthAsync(BankAccount account, CancellationToken token)
    {
        if (db.Entry(account).State == EntityState.Detached) db.BankAccounts.Add(account);
        var now = DateTimeOffset.UtcNow;
        await db.BankEntries.Where(item => item.MemberId == currentMember.Id && item.CreatedAtUtc.Year == now.Year && item.CreatedAtUtc.Month == now.Month).ExecuteDeleteAsync(token);
        await db.SaveChangesAsync(token);
    }
}
