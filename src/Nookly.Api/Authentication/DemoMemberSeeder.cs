using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nookly.Domain.Members;
using Nookly.Infrastructure.Data;

namespace Nookly.Api.Authentication;

public static class DemoMemberSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        var definitions = Environment.GetEnvironmentVariable("NOOKLY_DEMO_USERS");
        if (string.IsNullOrWhiteSpace(definitions)) return;
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NooklyDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Member>>();
        Member? firstMember = null;
        foreach (var definition in definitions.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var values = definition.Split('|');
            if (values.Length != 3) continue;
            var email = values[0].Trim().ToLowerInvariant();
            var member = await db.Members.SingleOrDefaultAsync(item => item.Email == email);
            if (member is null)
            {
                member = Member.Create(email, values[2]);
                member.SetPasswordHash(hasher.HashPassword(member, values[1]));
                db.Members.Add(member);
            }
            firstMember ??= member;
        }
        await db.SaveChangesAsync();
        if (firstMember is not null)
        {
            await db.MediaItems.Where(item => item.MemberId == null).ExecuteUpdateAsync(
                setters => setters.SetProperty(item => item.MemberId, firstMember.Id));
            await db.DiscoveryPreferences.Where(item => item.MemberId == null).ExecuteUpdateAsync(
                setters => setters.SetProperty(item => item.MemberId, firstMember.Id));
        }
    }
}
