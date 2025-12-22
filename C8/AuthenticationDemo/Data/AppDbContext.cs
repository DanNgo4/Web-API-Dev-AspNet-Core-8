using AuthenticationDemo.Models.User;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthenticationDemo.Data;

public class AppDbContext 
           : IdentityDbContext<AppUser>
{
    private readonly IConfiguration _configuration;

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) 
         : base(options)
    {
        _configuration = configuration;
    }
           
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));
    }
}
