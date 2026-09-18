using Microsoft.EntityFrameworkCore;
using Nookly.Application.Abstractions;
using Nookly.Domain.Banking;
using Nookly.Infrastructure.Data;

namespace Nookly.Infrastructure.Persistence;

internal sealed class BankRepository(NooklyDbContext db, ICurrentMember currentMember) : IBankRepository
{
    public Task<BankAccount?> GetAccountAsync(CancellationToken token) =>
        db.BankAccounts.SingleOrDefaultAsync(item => item.MemberId == currentMember.Id, token);
    public async Task<IReadOnlyList<BankEntry>> ListEntriesAsync(CancellationToken token) =>
        await db.BankEntries.AsNoTracking().Where(item => item.MemberId == currentMember.Id)
            .OrderByDescending(item => item.CreatedAtUtc).ToListAsync(token);
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
}
