using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;
using MerxoSell.API.Constants;
using MerxoSell.API.Data;
using MerxoSell.API.Helpers;
using MerxoSell.API.Middleware;
using MerxoSell.API.Models;
using MerxoSell.API.Repositories;
using MerxoSell.API.Repositories.Interfaces;
using MerxoSell.API.Services;
using MerxoSell.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── JWT Authentication ─────────────────────────────────────────
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = jwtSettings["Issuer"],
            ValidAudience            = jwtSettings["Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                                           Encoding.UTF8.GetBytes(jwtSettings["Secret"]!))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole(AppRoles.SuperAdmin));
    options.AddPolicy("Seller", policy => policy.RequireRole(AppRoles.Seller));
    options.AddPolicy("Buyer", policy => policy.RequireRole(AppRoles.Buyer));
    options.AddPolicy("SuperAdminOrSeller", policy => policy.RequireRole(AppRoles.SuperAdmin, AppRoles.Seller));
});

// ── CORS — allow any localhost origin (Angular :4200 + Flutter web + Swagger) ──
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
        policy.SetIsOriginAllowed(origin =>
                  Uri.TryCreate(origin, UriKind.Absolute, out var uri) &&
                  (uri.Host == "localhost" || uri.Host == "127.0.0.1"))
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// ── Helpers ────────────────────────────────────────────────────
builder.Services.AddScoped<JwtHelper>();

// ── Controllers & Swagger ─────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "MerxoSell API",
        Version     = "v1",
        Description = "REST API for the MerxoSell e-commerce platform."
    });

    // Bearer token input on the Authorize button
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: eyJhbGci..."
    };
    options.AddSecurityDefinition("Bearer", bearerScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });

    // Resolve schema ID conflicts when two DTOs share the same simple name
    options.CustomSchemaIds(type =>
    {
        var ns   = type.Namespace ?? string.Empty;
        var name = type.Name;
        // Use only the last segment of the namespace (e.g. "Admin" or "Orders")
        var segment = ns.Contains('.') ? ns[(ns.LastIndexOf('.') + 1)..] : ns;
        return segment.Length > 0 ? $"{segment}{name}" : name;
    });

    // Pull XML doc comments into Swagger descriptions
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
        options.IncludeXmlComments(xmlPath);
});

// ── Auth module ────────────────────────────────────────────────
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IAuthService,    AuthService>();

// ── Currency module ────────────────────────────────────────────
builder.Services.AddScoped<ICurrencyRepository, CurrencyRepository>();
builder.Services.AddScoped<ICurrencyService,    CurrencyService>();

// ── Categories module ──────────────────────────────────────────
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICategoryService,    CategoryService>();

// ── Products module ────────────────────────────────────────────
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService,    ProductService>();

// ── Manufacturers module ───────────────────────────────────────
builder.Services.AddScoped<IManufacturerRepository, ManufacturerRepository>();
builder.Services.AddScoped<IManufacturerService,    ManufacturerService>();

// ── Cart module ────────────────────────────────────────────────
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<ICartService,    CartService>();

// ── Orders module ──────────────────────────────────────────────
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService,    OrderService>();

// ── Coupons module ─────────────────────────────────────────────
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<ICouponService,    CouponService>();

// ── Reviews module ─────────────────────────────────────────────
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IReviewService,    ReviewService>();

// ── Profile module ─────────────────────────────────────────────
builder.Services.AddScoped<IAddressRepository, AddressRepository>();
builder.Services.AddScoped<IProfileService,    ProfileService>();

// ── Razorpay module ───────────────────────────────────────────
builder.Services.Configure<RazorpaySettings>(builder.Configuration.GetSection("Razorpay"));
builder.Services.AddHttpClient<IRazorpayService, RazorpayService>();

// ── Sellers module ─────────────────────────────────────────────
builder.Services.AddScoped<ISellerRepository,        SellerRepository>();
builder.Services.AddScoped<ISellerProductRepository, SellerProductRepository>();

// ── Customers module ───────────────────────────────────────────
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();

// ── Browsing-history module (buyer profile) ────────────────────
builder.Services.AddScoped<IBrowsingHistoryRepository, BrowsingHistoryRepository>();
builder.Services.AddScoped<IBrowsingHistoryService,    BrowsingHistoryService>();

// ── Approvals module ───────────────────────────────────────────
builder.Services.AddScoped<IProductApprovalRepository, ProductApprovalRepository>();

// ── Seller analytics module ────────────────────────────────────
builder.Services.AddScoped<ISellerAnalyticsRepository, SellerAnalyticsRepository>();
builder.Services.AddScoped<ISellerAnalyticsService,    SellerAnalyticsService>();

// ── Media module ───────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IMediaService, MediaService>();

// ── Dashboard module ───────────────────────────────────────────
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();

// ── Order-status-history module ────────────────────────────────
builder.Services.AddScoped<IOrderStatusHistoryRepository, OrderStatusHistoryRepository>();

// ── Price-drop notification ────────────────────────────────────
builder.Services.AddScoped<IPriceDropService, PriceDropService>();

// ── Offer Banners module ───────────────────────────────────────
builder.Services.AddScoped<IOfferBannerRepository, OfferBannerRepository>();
builder.Services.AddScoped<IOfferBannerService,    OfferBannerService>();

// ── Email module ───────────────────────────────────────────────
// Bound from the `Smtp` section of appsettings.json. When Enabled=false the
// service logs the payload and returns success so dev environments without
// real SMTP credentials don't break order/registration flows.
builder.Services.Configure<MerxoSell.API.Services.Email.SmtpOptions>(
    builder.Configuration.GetSection(MerxoSell.API.Services.Email.SmtpOptions.SectionName));
builder.Services.AddScoped<MerxoSell.API.Services.Email.IEmailService,
                           MerxoSell.API.Services.Email.SmtpEmailService>();

var app = builder.Build();

// ── Seed / repair default admin user ──────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    const string adminEmail    = "Admin@merxosell.com";
    const string adminPassword = "Admin@123";

    var db        = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var superAdminRole = EnsureRole(db, AppRoles.SuperAdmin);
    var sellerRole     = EnsureRole(db, AppRoles.Seller);
    var buyerRole      = EnsureRole(db, AppRoles.Buyer);

    MergeRole(db, "Admin", superAdminRole);
    MergeRole(db, "Customer", buyerRole);

    bool IsValidHash(string text, string? hash)
    {
        if (string.IsNullOrWhiteSpace(hash)) return false;
        try { return BCrypt.Net.BCrypt.Verify(text, hash); }
        catch { return false; }
    }

    var adminUser = db.Users.FirstOrDefault(u => u.Email.ToLower() == adminEmail.ToLower());

    if (adminUser is null)
    {
        db.Users.Add(new User
        {
            RoleId            = superAdminRole.RoleId,
            FullName          = "Administrator",
            Email             = adminEmail,
            PasswordHash      = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 11),
            PreferredCurrency = "CAD",
            IsActive          = true,
            CreatedAt         = DateTime.UtcNow
        });
        db.SaveChanges();
    }
    else if (!IsValidHash(adminPassword, adminUser.PasswordHash))
    {
        // Repair a truncated or outdated hash left by an earlier seed script
        adminUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword, workFactor: 11);
        adminUser.RoleId       = superAdminRole.RoleId;
        db.SaveChanges();
    }
    else if (adminUser.RoleId != superAdminRole.RoleId)
    {
        adminUser.RoleId = superAdminRole.RoleId;
        db.SaveChanges();
    }

    // ── Seed / repair default seller user ─────────────────────────────────────
    const string sellerEmail    = "seller@merxosell.com";
    const string sellerPassword = "Seller@123";
    
    var sellerUser = db.Users.Include(u => u.Seller).FirstOrDefault(u => u.Email.ToLower() == sellerEmail.ToLower());
    if (sellerUser is null)
    {
        var newUser = new User
        {
            RoleId            = sellerRole.RoleId,
            FullName          = "Sample Seller",
            Email             = sellerEmail,
            PasswordHash      = BCrypt.Net.BCrypt.HashPassword(sellerPassword, workFactor: 11),
            PreferredCurrency = "CAD",
            IsActive          = true,
            CreatedAt         = DateTime.UtcNow
        };
        db.Users.Add(newUser);
        db.SaveChanges();

        db.Sellers.Add(new Seller
        {
            UserId    = newUser.UserId,
            StoreName = "MerxoSell Sample Store",
            IsActive  = true,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
    }
    else if (!IsValidHash(sellerPassword, sellerUser.PasswordHash))
    {
        sellerUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(sellerPassword, workFactor: 11);
        sellerUser.RoleId       = sellerRole.RoleId;
        db.SaveChanges();
    }
    else if (sellerUser.RoleId != sellerRole.RoleId)
    {
        sellerUser.RoleId = sellerRole.RoleId;
        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MerxoSell API v1");
        options.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAngular");   // must be before UseStaticFiles so /uploads images get CORS headers
app.UseStaticFiles();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseAuthentication();
app.UseMiddleware<UserStatusMiddleware>();
app.UseAuthorization();
app.MapControllers();

app.Run();

static Role EnsureRole(AppDbContext db, string roleName)
{
    var role = db.Roles.FirstOrDefault(r => r.RoleName == roleName);
    if (role is not null) return role;

    role = new Role { RoleName = roleName };
    db.Roles.Add(role);
    db.SaveChanges();
    return role;
}

static void MergeRole(AppDbContext db, string obsoleteRoleName, Role targetRole)
{
    var obsoleteRole = db.Roles.FirstOrDefault(r => r.RoleName == obsoleteRoleName);
    if (obsoleteRole is null || obsoleteRole.RoleId == targetRole.RoleId) return;

    foreach (var user in db.Users.Where(u => u.RoleId == obsoleteRole.RoleId))
        user.RoleId = targetRole.RoleId;

    db.SaveChanges();

    if (!db.Users.Any(u => u.RoleId == obsoleteRole.RoleId))
    {
        db.Roles.Remove(obsoleteRole);
        db.SaveChanges();
    }
}

// Expose Program to the integration test project
public partial class Program { }
