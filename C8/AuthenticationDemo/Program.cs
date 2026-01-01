using AuthenticationDemo.Data;
using AuthenticationDemo.Models.Authentication;
using AuthenticationDemo.Models.Role;
using AuthenticationDemo.Models.User;
using AuthenticationDemo.Utilities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddIdentityCore<AppUser>() // add and configure the identity system for the specified User type
                .AddSignInManager<AppUser>()
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

// new Identity API endpoints in ASP.NET Core 8
builder.Services.AddIdentityApiEndpoints<AppUser>()
                .AddEntityFrameworkStores<AppDbContext>();

builder.Services.Configure<IdentityOptions>(x =>
{
    // Password settings
    x.Password.RequireDigit = true;
    x.Password.RequireLowercase = true;
    x.Password.RequireUppercase = true;
    x.Password.RequireNonAlphanumeric = true;
    x.Password.RequiredLength = 8;
    x.Password.RequiredUniqueChars = 1;

    // User settings
    x.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    x.User.RequireUniqueEmail = true;

    // Lockout settings
    x.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    x.Lockout.MaxFailedAccessAttempts = 3;
    x.Lockout.AllowedForNewUsers = true;
});

builder.Services.AddAuthentication(x =>
                {
                    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
                    x.DefaultScheme             = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(x =>
                {
                    var secret   = builder.Configuration["JwtConfig:Secret"];
                    var issuer   = builder.Configuration["JwtConfig:ValidIssuer"];
                    var audience = builder.Configuration["JwtConfig:ValidAudiences"];

                    if (secret is null || issuer is null || audience is null)
                    {
                        throw new ApplicationException("JWT is not set in the configuraiton");
                    }

                    x.SaveToken                 = true;
                    x.RequireHttpsMetadata      = false;
                    x.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer   = true,
                        ValidateAudience = true,
                        ValidAudience    = audience,
                        ValidIssuer      = issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                    };
                });

builder.Services.AddAuthorization(x =>
{
    // Role-based authorisation
    x.AddPolicy("RequireAdministratorRole", y => y.RequireRole(AppRoles.Administrator));
    x.AddPolicy("RequireVipUserRole", y => y.RequireRole(AppRoles.VipUser));
    x.AddPolicy("RequireUserRole", y => y.RequireRole(AppRoles.User));
    x.AddPolicy("RequireUserRoleOrVipUserRole", y => y.RequireRole(AppRoles.User, AppRoles.VipUser));

    // Claim-based authorisation
    x.AddPolicy(AppAuthorisationPolicies.RequireDrivingLicenseNumber, y => y.RequireClaim(AppClaimTypes.DrivingLicenseNumber));
    x.AddPolicy(AppAuthorisationPolicies.RequireAccessNumber, y => y.RequireClaim(AppClaimTypes.AccessNumber));
    x.AddPolicy(AppAuthorisationPolicies.RequireCountry, y => y.RequireClaim(AppClaimTypes.Country, "New Zealand"));
    x.AddPolicy(AppAuthorisationPolicies.RequireDrivingLicenseAndAccessNumber, y => y.RequireAssertion(z =>
    {
        var hasDrivingLicenseNumber = z.User.HasClaim(c => c.Type == AppClaimTypes.DrivingLicenseNumber);
        var hasAccessNumber = z.User.HasClaim(c => c.Type == AppClaimTypes.AccessNumber);

        return hasDrivingLicenseNumber && hasAccessNumber;
    }));

    // Policy-based authorisation
    x.AddPolicy(AppAuthorisationPolicies.SpecialPremiumContent, y =>
    {
        y.Requirements.Add(new SpecialPremiumContentRequirement("New Zealand"));
    });
});

builder.Services.AddSingleton<IAuthorizationHandler, SpecialPremiumContentAuthorisationHandler>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapGroup("/identity")
   .MapIdentityApi<AppUser>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var roleManager = app.Services.GetRequiredService<RoleManager<IdentityRole>>();

    var userRoleExists = await roleManager.RoleExistsAsync(AppRoles.User);
    if (!userRoleExists)
    {
        await roleManager.CreateAsync(new IdentityRole(AppRoles.User));
    }

    var vipUserRoleExists = await roleManager.RoleExistsAsync(AppRoles.VipUser);
    if (!vipUserRoleExists)
    {
        await roleManager.CreateAsync(new IdentityRole(AppRoles.VipUser));
    }

    var adminRoleExists = await roleManager.RoleExistsAsync(AppRoles.Administrator);
    if (!adminRoleExists)
    {
        await roleManager.CreateAsync(new IdentityRole(AppRoles.Administrator));
    }

    var dbContext = services.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
