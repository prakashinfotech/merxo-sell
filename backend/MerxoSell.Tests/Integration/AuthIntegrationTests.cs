using FluentAssertions;
using System.Net;
using System.Net.Http.Json;
using MerxoSell.API.DTOs.Auth;
using MerxoSell.API.DTOs.Common;

namespace MerxoSell.Tests.Integration;

public class AuthIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_ValidBuyer_Returns200WithToken()
    {
        var dto = new RegisterRequestDto
        {
            FullName        = "Happy Buyer",
            Email           = $"happy_{Guid.NewGuid():N}@test.com",
            Password        = "Happy@1234!",
            ConfirmPassword = "Happy@1234!",
            Role            = "Buyer"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
        body!.Success.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        body.Data.Role.Should().Be("Buyer");
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        var email = $"dup_{Guid.NewGuid():N}@test.com";
        var dto = new RegisterRequestDto
        {
            FullName = "Dup", Email = email,
            Password = "Dup@1234!", ConfirmPassword = "Dup@1234!", Role = "Buyer"
        };

        await _client.PostAsJsonAsync("/api/auth/register", dto);
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_MissingFullName_Returns400()
    {
        var dto = new { Email = "x@x.com", Password = "Abc@1234!", ConfirmPassword = "Abc@1234!", Role = "Buyer" };
        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_ValidSeller_Returns200WithSellerRole()
    {
        var dto = new RegisterRequestDto
        {
            FullName        = "New Seller",
            Email           = $"seller_{Guid.NewGuid():N}@test.com",
            Password        = "Seller@1234!",
            ConfirmPassword = "Seller@1234!",
            Role            = "Seller",
            StoreName       = "New Store"
        };

        var response = await _client.PostAsJsonAsync("/api/auth/register", dto);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
        body!.Data!.Role.Should().Be("Seller");
        body.Data.SellerId.Should().NotBeNull();
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_SeededAdmin_Returns200()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new LoginRequestDto
        {
            Email = "buyer_integration@test.com", Password = "Any@1234!"
        });
        // Unknown email → 401, which is correct
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_RegisteredBuyer_Returns200WithBuyerRole()
    {
        var email    = $"lb_{Guid.NewGuid():N}@test.com";
        var password = "Login@1234!";
        var regDto   = new RegisterRequestDto
        {
            FullName = "Login Buyer", Email = email,
            Password = password, ConfirmPassword = password, Role = "Buyer"
        };

        await _client.PostAsJsonAsync("/api/auth/register", regDto);

        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto { Email = email, Password = password });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
        body!.Data!.Role.Should().Be("Buyer");
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequestDto { Email = "admin@merxosell.com", Password = "Wrong@1234!" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Admin login ───────────────────────────────────────────────────────────

    [Fact]
    public async Task AdminLogin_SeededAdmin_Returns200()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/admin/login",
            new LoginRequestDto { Email = "admin@merxosell.com", Password = "Admin@123" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<ApiResponse<LoginResponseDto>>();
        body!.Data!.Role.Should().Be("SuperAdmin");
    }

    [Fact]
    public async Task AdminLogin_BuyerCredentials_Returns401()
    {
        var email    = $"ba_{Guid.NewGuid():N}@test.com";
        var password = "BuyerAdmin@1234!";
        var regDto   = new RegisterRequestDto
        {
            FullName = "B Admin", Email = email,
            Password = password, ConfirmPassword = password, Role = "Buyer"
        };
        await _client.PostAsJsonAsync("/api/auth/register", regDto);

        var response = await _client.PostAsJsonAsync("/api/auth/admin/login",
            new LoginRequestDto { Email = email, Password = password });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
