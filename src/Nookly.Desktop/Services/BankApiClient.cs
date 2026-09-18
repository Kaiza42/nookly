using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Nookly.Contracts.Banking;

namespace Nookly.Desktop.Services;

public sealed class BankApiClient(HttpClient client) : IBankApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    public async Task<BankSummaryResponse> GetAsync(CancellationToken token = default) =>
        await client.GetFromJsonAsync<BankSummaryResponse>("api/bank", JsonOptions, token)
        ?? throw new InvalidOperationException("Empty bank response.");
    public async Task<BankSummaryResponse> SetStartingBalanceAsync(decimal amount, CancellationToken token = default)
    {
        using var response = await client.PutAsJsonAsync("api/bank/starting-balance", new SetStartingBalanceRequest(amount), JsonOptions, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BankSummaryResponse>(JsonOptions, token) ?? throw new InvalidOperationException("Empty bank response.");
    }
    public async Task<BankEntryResponse> AddEntryAsync(string label, decimal amount, CancellationToken token = default)
    {
        using var response = await client.PostAsJsonAsync("api/bank/entries", new CreateBankEntryRequest(label, amount), JsonOptions, token);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<BankEntryResponse>(JsonOptions, token) ?? throw new InvalidOperationException("Empty bank response.");
    }
    public async Task DeleteEntryAsync(Guid id, CancellationToken token = default)
    { using var response = await client.DeleteAsync($"api/bank/entries/{id}", token); response.EnsureSuccessStatusCode(); }
    public async Task ResetCurrentMonthAsync(CancellationToken token = default)
    { using var response = await client.DeleteAsync("api/bank/current-month", token); response.EnsureSuccessStatusCode(); }
}
