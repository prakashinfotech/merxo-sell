using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using MerxoSell.API.Constants;
using MerxoSell.API.DTOs.Auth;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services;
using MerxoSell.API.Services.Email;

namespace MerxoSell.Tests.Services;

public class AuthServiceTests : TestBase
{
    private readonly Mock<IUserRepository>   _userRepo   = new();
    private readonly Mock<IRoleRepository>   _roleRepo   = new();
    private readonly Mock<ISellerRepository> _sellerRepo = new();
    private readonly Mock<IEmailService>     _email      = new();

    private AuthService BuildSut() => new(
        _userRepo.Object, _roleRepo.Object, _sellerRepo.Object,
        Jwt, JwtConfig, _email.Object,
        NullLogger<AuthService>.Instance);

    // ── Register ──────────────────────────────────────────────────────────────

    [Fact]
    public async Task Register_Buyer_ReturnsResponseWithBuyerRole()
    {
        var dto = new RegisterRequestDto
        {
            FullName        = "Alice",
            Email           = "alice@test.com",
            Password        = "Alice@123",
            ConfirmPassword = "Alice@123",
            Role            = "Buyer"
        };

        _userRepo.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(false);
        _roleRepo.Setup(r => r.GetByNameAsync(AppRoles.Buyer)).ReturnsAsync(BuyerRole);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>()))
                 .ReturnsAsync((User u) => { u.UserId = 99; u.Role = BuyerRole; return u; });
        _email.Setup(e => e.SendWelcomeAsync(
                   It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<IReadOnlyList<MerxoSell.API.DTOs.Products.ProductListDto>>()))
              .ReturnsAsync(true);

        var sut    = BuildSut();
        var result = await sut.RegisterAsync(dto);

        result.Role.Should().Be(AppRoles.Buyer);
        result.Email.Should().Be(dto.Email);
        result.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Register_DuplicateEmail_ThrowsInvalidOperationException()
    {
        var dto = new RegisterRequestDto
        {
            FullName = "Bob", Email = "dup@test.com",
            Password = "Bob@1234!", ConfirmPassword = "Bob@1234!", Role = "Buyer"
        };

        _userRepo.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(true);

        var sut = BuildSut();
        await sut.Invoking(s => s.RegisterAsync(dto))
                 .Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*already exists*");
    }

    [Fact]
    public async Task Register_InvalidRole_ThrowsInvalidOperationException()
    {
        var dto = new RegisterRequestDto
        {
            FullName = "Eve", Email = "eve@test.com",
            Password = "Eve@1234!", ConfirmPassword = "Eve@1234!", Role = "SuperAdmin"
        };

        _userRepo.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(false);

        var sut = BuildSut();
        await sut.Invoking(s => s.RegisterAsync(dto))
                 .Should().ThrowAsync<InvalidOperationException>()
                 .WithMessage("*Buyer or Seller*");
    }

    [Fact]
    public async Task Register_Seller_CreatesSellerRecord()
    {
        var dto = new RegisterRequestDto
        {
            FullName        = "Sam Seller",
            Email           = "sam@test.com",
            Password        = "Sam@1234!",
            ConfirmPassword = "Sam@1234!",
            Role            = "Seller",
            StoreName       = "Sam's Shop"
        };

        var createdUser = new User
        {
            UserId = 88, RoleId = SellerRole.RoleId, Role = SellerRole,
            FullName = dto.FullName, Email = dto.Email,
            PasswordHash = "", IsActive = true, Seller = SellerRecord
        };

        _userRepo.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(false);
        _roleRepo.Setup(r => r.GetByNameAsync(AppRoles.Seller)).ReturnsAsync(SellerRole);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _sellerRepo.Setup(r => r.CreateAsync(It.IsAny<Seller>()))
                   .ReturnsAsync((Seller s) => s);
        _userRepo.Setup(r => r.GetByIdAsync(createdUser.UserId)).ReturnsAsync(createdUser);
        _email.Setup(e => e.SendWelcomeAsync(
                   It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<IReadOnlyList<MerxoSell.API.DTOs.Products.ProductListDto>>()))
              .ReturnsAsync(true);

        var sut    = BuildSut();
        var result = await sut.RegisterAsync(dto);

        _sellerRepo.Verify(r => r.CreateAsync(It.Is<Seller>(s => s.StoreName == "Sam's Shop")), Times.Once);
        result.Role.Should().Be(AppRoles.Seller);
    }

    [Fact]
    public async Task Register_Seller_NullStoreName_DefaultsToFullNameStore()
    {
        var dto = new RegisterRequestDto
        {
            FullName = "No Store", Email = "nostore@test.com",
            Password = "NoStore@1!", ConfirmPassword = "NoStore@1!",
            Role = "Seller", StoreName = null
        };

        var createdUser = new User
        {
            UserId = 90, RoleId = SellerRole.RoleId, Role = SellerRole,
            FullName = dto.FullName, Email = dto.Email,
            PasswordHash = "", IsActive = true, Seller = SellerRecord
        };

        _userRepo.Setup(r => r.ExistsByEmailAsync(dto.Email)).ReturnsAsync(false);
        _roleRepo.Setup(r => r.GetByNameAsync(AppRoles.Seller)).ReturnsAsync(SellerRole);
        _userRepo.Setup(r => r.CreateAsync(It.IsAny<User>())).ReturnsAsync(createdUser);
        _sellerRepo.Setup(r => r.CreateAsync(It.IsAny<Seller>())).ReturnsAsync((Seller s) => s);
        _userRepo.Setup(r => r.GetByIdAsync(90)).ReturnsAsync(createdUser);
        _email.Setup(e => e.SendWelcomeAsync(
                   It.IsAny<string>(), It.IsAny<string>(),
                   It.IsAny<IReadOnlyList<MerxoSell.API.DTOs.Products.ProductListDto>>()))
              .ReturnsAsync(true);

        await BuildSut().RegisterAsync(dto);

        _sellerRepo.Verify(r => r.CreateAsync(
            It.Is<Seller>(s => s.StoreName.Contains("No Store"))), Times.Once);
    }

    // ── Login (Buyer) ─────────────────────────────────────────────────────────

    [Fact]
    public async Task Login_ValidCredentials_ReturnsToken()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("buyer@test.com")).ReturnsAsync(BuyerUser);

        var result = await BuildSut().LoginAsync(new LoginRequestDto
        {
            Email = "buyer@test.com", Password = "Buyer@123"
        });

        result.AccessToken.Should().NotBeNullOrEmpty();
        result.Role.Should().Be(AppRoles.Buyer);
    }

    [Fact]
    public async Task Login_WrongPassword_ThrowsUnauthorized()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("buyer@test.com")).ReturnsAsync(BuyerUser);

        await BuildSut().Invoking(s => s.LoginAsync(new LoginRequestDto
        {
            Email = "buyer@test.com", Password = "WrongPassword!"
        }))
        .Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Login_UnknownEmail_ThrowsSameMessageToPreventEnumeration()
    {
        _userRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>())).ReturnsAsync((User?)null);

        await BuildSut().Invoking(s => s.LoginAsync(new LoginRequestDto
        {
            Email = "ghost@test.com", Password = "Any@123!"
        }))
        .Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("*Invalid email or password*");
    }

    [Fact]
    public async Task Login_InactiveAccount_ThrowsUnauthorized()
    {
        var inactive = new User
        {
            UserId = 20, Role = BuyerRole, RoleId = BuyerRole.RoleId,
            FullName = "Inactive", Email = "off@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Off@1234!", workFactor: 4),
            IsActive = false
        };

        _userRepo.Setup(r => r.GetByEmailAsync(inactive.Email)).ReturnsAsync(inactive);

        await BuildSut().Invoking(s => s.LoginAsync(new LoginRequestDto
        {
            Email = inactive.Email, Password = "Off@1234!"
        }))
        .Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("*disabled or removed*");
    }

    [Fact]
    public async Task Login_SellerAccountOnBuyerEndpoint_ThrowsAccessDenied()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("seller@test.com")).ReturnsAsync(SellerUser);

        await BuildSut().Invoking(s => s.LoginAsync(new LoginRequestDto
        {
            Email = "seller@test.com", Password = "Seller@123"
        }))
        .Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("*Buyer credentials*");
    }

    // ── AdminLogin ────────────────────────────────────────────────────────────

    [Fact]
    public async Task AdminLogin_ValidAdmin_ReturnsToken()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("admin@test.com")).ReturnsAsync(AdminUser);

        var result = await BuildSut().AdminLoginAsync(new LoginRequestDto
        {
            Email = "admin@test.com", Password = "Admin@123"
        });

        result.Role.Should().Be(AppRoles.SuperAdmin);
    }

    [Fact]
    public async Task AdminLogin_BuyerAccount_ThrowsAccessDenied()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("buyer@test.com")).ReturnsAsync(BuyerUser);

        await BuildSut().Invoking(s => s.AdminLoginAsync(new LoginRequestDto
        {
            Email = "buyer@test.com", Password = "Buyer@123"
        }))
        .Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("*SuperAdmin*");
    }

    // ── SellerLogin ───────────────────────────────────────────────────────────

    [Fact]
    public async Task SellerLogin_ValidSeller_ReturnsToken()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("seller@test.com")).ReturnsAsync(SellerUser);

        var result = await BuildSut().SellerLoginAsync(new LoginRequestDto
        {
            Email = "seller@test.com", Password = "Seller@123"
        });

        result.Role.Should().Be(AppRoles.Seller);
        result.SellerId.Should().Be(SellerRecord.SellerId);
    }

    [Fact]
    public async Task SellerLogin_InactiveSeller_ThrowsUnauthorized()
    {
        var inactiveSeller = new Seller { SellerId = 99, UserId = 50, StoreName = "Old", IsActive = false };
        var user = new User
        {
            UserId = 50, Role = SellerRole, RoleId = SellerRole.RoleId,
            FullName = "Inactive Seller", Email = "inact@test.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Inact@1234!", workFactor: 4),
            IsActive = true, Seller = inactiveSeller
        };

        _userRepo.Setup(r => r.GetByEmailAsync(user.Email)).ReturnsAsync(user);

        await BuildSut().Invoking(s => s.SellerLoginAsync(new LoginRequestDto
        {
            Email = user.Email, Password = "Inact@1234!"
        }))
        .Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("*seller account is inactive*");
    }

    // ── JWT claims ────────────────────────────────────────────────────────────

    [Fact]
    public void GenerateJwt_BuyerUser_ContainsEmailAndRoleClaims()
    {
        var token   = Jwt.GenerateToken(BuyerUser);
        var handler = new JwtSecurityTokenHandler();
        var parsed  = handler.ReadJwtToken(token);

        parsed.Claims.Should().Contain(c => c.Type == "email" && c.Value == BuyerUser.Email);
        parsed.Claims.Should().Contain(c =>
            (c.Type == ClaimTypes.Role || c.Type == "role") && c.Value == AppRoles.Buyer);
    }

    [Fact]
    public void GenerateJwt_SellerUser_ContainsSellerIdClaim()
    {
        var token  = Jwt.GenerateToken(SellerUser);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Claims.Should().Contain(c =>
            c.Type == "sellerId" && c.Value == SellerRecord.SellerId.ToString());
    }

    [Fact]
    public void GenerateJwt_BuyerUser_DoesNotContainSellerIdClaim()
    {
        var token  = Jwt.GenerateToken(BuyerUser);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Claims.Should().NotContain(c => c.Type == "sellerId");
    }

    [Fact]
    public void GenerateJwt_ContainsSubClaim_MatchingUserId()
    {
        var token  = Jwt.GenerateToken(BuyerUser);
        var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token);

        parsed.Claims.Should().Contain(c =>
            c.Type == JwtRegisteredClaimNames.Sub && c.Value == BuyerUser.UserId.ToString());
    }
}
