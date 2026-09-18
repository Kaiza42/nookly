using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nookly.Domain.Members;
using Nookly.Infrastructure.Data;

namespace Nookly.Api.Authentication;

public static class DemoMemberSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<NooklyDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Member>>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        await db.Database.ExecuteSqlRawAsync("UPDATE members SET \"Role\" = 'Member', \"IsEmailConfirmed\" = TRUE WHERE \"Role\" = ''");
        var definitions = configuration["NOOKLY_DEMO_USERS"] ?? string.Empty;
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
                member.ConfirmEmail();
                db.Members.Add(member);
            }
            else if (!member.IsEmailConfirmed) member.ConfirmEmail();
            firstMember ??= member;
        }
        await db.SaveChangesAsync();
        var adminEmail = "emerick.roeting1@gmail.com";
        var adminPassword = configuration["NOOKLY_ADMIN_PASSWORD"];
        if (!string.IsNullOrWhiteSpace(adminPassword))
        {
            var admin = await db.Members.SingleOrDefaultAsync(item => item.Email == adminEmail);
            if (admin is null)
            {
                admin = Member.Create(adminEmail, "Emerick", MemberRole.Admin);
                admin.SetPasswordHash(hasher.HashPassword(admin, adminPassword));
                admin.ConfirmEmail();
                db.Members.Add(admin);
                await db.SaveChangesAsync();
            }
            else
            {
                admin.PromoteToAdmin(); admin.ConfirmEmail();
                admin.SetPasswordHash(hasher.HashPassword(admin, adminPassword));
                await db.SaveChangesAsync();
            }
        }
        if (firstMember is not null)
        {
            await db.MediaItems.Where(item => item.MemberId == null).ExecuteUpdateAsync(
                setters => setters.SetProperty(item => item.MemberId, firstMember.Id));
            await db.DiscoveryPreferences.Where(item => item.MemberId == null).ExecuteUpdateAsync(
                setters => setters.SetProperty(item => item.MemberId, firstMember.Id));
        }
    }
}
