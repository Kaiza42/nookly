using System.Text.Json.Serialization;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Nookly.Api.Endpoints;
using Nookly.Api.Authentication;
using Nookly.Application.Abstractions;
using Nookly.Domain.Members;
using Nookly.Infrastructure;
using Nookly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Nookly.Api.Email;

EnvironmentLoader.Load(Directory.GetCurrentDirectory());
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentMember, CurrentMember>();
builder.Services.AddScoped<IPasswordHasher<Member>, PasswordHasher<Member>>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddSingleton<AccountTokenService>();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
var jwtKey = builder.Configuration["NOOKLY_JWT_KEY"]
             ?? throw new InvalidOperationException("NOOKLY_JWT_KEY is missing.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.FromMinutes(1)
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("MemberOnly", policy => policy.RequireRole("Member", "Admin"));
});
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var app = builder.Build();
using (var migrationScope = app.Services.CreateScope())
{
    await migrationScope.ServiceProvider.GetRequiredService<NooklyDbContext>().Database.MigrateAsync();
}
await DemoMemberSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.UseAuthentication();
app.UseAuthorization();
app.MapAuthenticationEndpoints();
app.MapMediaEndpoints();
app.MapBankEndpoints();
app.MapAdministrationEndpoints();

app.Run();
