using MerxoSell.API.Constants;
using MerxoSell.API.DTOs.Auth;
using MerxoSell.API.Helpers;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services.Email;
using MerxoSell.API.Services.Interfaces;

namespace MerxoSell.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository       _userRepo;
    private readonly IRoleRepository       _roleRepo;
    private readonly ISellerRepository      _sellerRepo;
    private readonly JwtHelper             _jwt;
    private readonly IConfiguration        _config;
    private readonly IEmailService         _email;
    private readonly ILogger<AuthService>  _logger;

    public AuthService(
        IUserRepository userRepo, IRoleRepository roleRepo, ISellerRepository sellerRepo,
        JwtHelper jwt, IConfiguration config, IEmailService email, ILogger<AuthService> logger)
    {
        _userRepo   = userRepo;
        _roleRepo   = roleRepo;
        _sellerRepo = sellerRepo;
        _jwt        = jwt;
        _config     = config;
        _email      = email;
        _logger     = logger;
    }

    public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto)
    {
        if (await _userRepo.ExistsByEmailAsync(dto.Email))
            throw new InvalidOperationException("An account with this email already exists.");

        // Public registration supports Seller plus the single buyer/customer role.
        var roleName = AppRoles.NormalizePublicRole(dto.Role);
        if (!AppRoles.PublicRegistrationRoles.Contains(roleName))
            throw new InvalidOperationException("Registration role must be Buyer or Seller.");

        var role = await _roleRepo.GetByNameAsync(roleName)
            ?? throw new InvalidOperationException($"Role '{roleName}' not configured.");

        var user = new User
        {
            RoleId            = role.RoleId,
            FullName          = dto.FullName.Trim(),
            Email             = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash      = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 11),
            PreferredCurrency = "CAD",
            IsActive          = true,
            CreatedAt         = DateTime.UtcNow
        };

        var created = await _userRepo.CreateAsync(user);

        // If registering as Seller, auto-create a Seller record
        if (roleName == AppRoles.Seller)
        {
            var storeName = dto.StoreName?.Trim() ?? $"{dto.FullName.Trim()}'s Store";
            await _sellerRepo.CreateAsync(new Seller
            {
                UserId    = created.UserId,
                StoreName = storeName,
                IsActive  = true
            });
            // Reload with Seller navigation for JWT generation
            created = await _userRepo.GetByIdAsync(created.UserId) 
                ?? throw new InvalidOperationException("User not found after creation.");
        }

        // Welcome email — best effort, must not fail registration.
        try
        {
            var emailSent = await _email.SendWelcomeAsync(created.Email, created.FullName);
            if (!emailSent)
            {
                _logger.LogWarning(
                    "Welcome email was not accepted by the configured SMTP provider for user {UserId} ({Email}).",
                    created.UserId, created.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Welcome email failed for user {UserId}", created.UserId);
        }

        return BuildResponse(created);
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

        // Combine "not found" and "wrong password" into one message to prevent account enumeration
        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive || user.IsDeleted)
            throw new UnauthorizedAccessException("This account has been disabled or removed.");

        if (user.Role.RoleName != AppRoles.Buyer)
            throw new UnauthorizedAccessException("Access denied. Buyer credentials required.");

        return BuildResponse(user);
    }

    public async Task<LoginResponseDto> AdminLoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive || user.IsDeleted)
            throw new UnauthorizedAccessException("This account has been disabled or removed.");

        if (user.Role.RoleName != AppRoles.SuperAdmin)
            throw new UnauthorizedAccessException("Access denied. SuperAdmin credentials required.");

        return BuildResponse(user);
    }

    public async Task<LoginResponseDto> SellerLoginAsync(LoginRequestDto dto)
    {
        var user = await _userRepo.GetByEmailAsync(dto.Email.Trim().ToLowerInvariant());

        if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        if (!user.IsActive || user.IsDeleted)
            throw new UnauthorizedAccessException("This account has been disabled or removed.");

        if (user.Role.RoleName != AppRoles.Seller)
            throw new UnauthorizedAccessException("Access denied. Seller credentials required.");

        if (user.Seller is null || !user.Seller.IsActive)
            throw new UnauthorizedAccessException("Your seller account is inactive. Contact support.");

        return BuildResponse(user);
    }

    private LoginResponseDto BuildResponse(User user) =>
        new()
        {
            AccessToken       = _jwt.GenerateToken(user),
            ExpiresIn         = int.Parse(_config["Jwt:ExpiryMins"] ?? "1440") * 60,
            UserId            = user.UserId,
            FullName          = user.FullName,
            Email             = user.Email,
            Role              = user.Role.RoleName,
            SellerId          = user.Seller?.SellerId,
            PreferredCurrency = user.PreferredCurrency
        };
}
