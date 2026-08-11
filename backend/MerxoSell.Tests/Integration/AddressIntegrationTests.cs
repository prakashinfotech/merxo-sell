using FluentAssertions;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MerxoSell.API.DTOs.Profile;

namespace MerxoSell.Tests.Integration;

/// <summary>
/// Address CRUD — every operation is scoped to the requesting user's JWT.
/// A buyer never sees another buyer's addresses.
/// </summary>
public class AddressIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory _factory;

    public AddressIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
        _client  = factory.CreateClient();
    }

    private static CreateAddressDto ValidAddress() => new(
        FullName: "John Doe",
        AddressLine1: "123 Test St",
        AddressLine2: null,
        City: "Toronto",
        State: "ON",
        PostalCode: "M5A 1A1",
        Country: "Canada",
        Phone: null,
        Label: "Home"
    );

    // ── 401 without token ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetAddresses_NoToken_Returns401()
    {
        var response = await _client.GetAsync("/api/profile/addresses");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PostAddress_NoToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/profile/addresses", ValidAddress());
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Buyer can CRUD their own addresses ────────────────────────────────────

    [Fact]
    public async Task PostAddress_AuthenticatedBuyer_Returns201()
    {
        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.PostAsJsonAsync("/api/profile/addresses", ValidAddress());

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<AddressDto>();
        body!.City.Should().Be("Toronto");
    }

    [Fact]
    public async Task GetAddresses_AuthenticatedBuyer_ReturnsOwnAddresses()
    {
        var token = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client);
        using var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        await client.PostAsJsonAsync("/api/profile/addresses", ValidAddress());

        var response = await client.GetAsync("/api/profile/addresses");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IEnumerable<AddressDto>>();
        body!.Should().HaveCountGreaterThan(0);
    }

    [Fact]
    public async Task GetAddresses_TwoBuyers_DoNotSeeEachOthersAddresses()
    {
        // Buyer A creates an address
        var tokenA = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client, "ownerA");
        using var clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        await clientA.PostAsJsonAsync("/api/profile/addresses", ValidAddress());

        // Buyer B has no addresses
        var tokenB = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client, "ownerB");
        using var clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var response = await clientB.GetAsync("/api/profile/addresses");
        var addresses = await response.Content.ReadFromJsonAsync<IEnumerable<AddressDto>>();

        // Buyer B must see zero addresses — not Buyer A's
        addresses!.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAddress_WrongOwner_Returns404()
    {
        // Buyer A creates an address
        var tokenA = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client, "delA");
        using var clientA = _factory.CreateClient();
        clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
        var createResp = await clientA.PostAsJsonAsync("/api/profile/addresses", ValidAddress());
        var addr = await createResp.Content.ReadFromJsonAsync<AddressDto>();

        // Buyer B tries to delete Buyer A's address
        var tokenB = await IntegrationHelpers.RegisterAndLoginBuyerAsync(_client, "delB");
        using var clientB = _factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var deleteResp = await clientB.DeleteAsync($"/api/profile/addresses/{addr!.AddressId}");

        // Service scopes by user — must be 404 (not found for that user)
        deleteResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
