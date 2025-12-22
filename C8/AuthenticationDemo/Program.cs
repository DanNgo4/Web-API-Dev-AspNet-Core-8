using AuthenticationDemo.Data;
using AuthenticationDemo.Models.Role;
using AuthenticationDemo.Models.User;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddIdentityCore<AppUser>() // add and configure the identity system for the specified User type
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

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
    x.AddPolicy("RequireAdministratorRole", y => y.RequireRole(AppRoles.Administrator));
    x.AddPolicy("RequireVipUserRole", y => y.RequireRole(AppRoles.VipUser));
    x.AddPolicy("RequireUserRole", y => y.RequireRole(AppRoles.User));
    x.AddPolicy("RequireUserRoleOrVipUserRole", y => y.RequireRole(AppRoles.User, AppRoles.VipUser));
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

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
