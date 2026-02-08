using InvoiceApp.IntegrationTests.Helpers;
using InvoiceApp.WebApi.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InvoiceApp.IntegrationTests.Fixtures;

public class CustomIntegrationTestsFixture : WebApplicationFactory<Program>
{
    private const string ConnectionString = @"Server=(localdb)\mssqllocaldb;Database=InvoiceIntegrationTestDb;Trusted_Connection=True";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Set up a test database
        builder.ConfigureServices(x =>
        {
            var descriptor = x.SingleOrDefault(y => y.ServiceType == typeof(DbContextOptions<InvoiceDbContext>));
            if (descriptor is null)
                return;

            x.Remove(descriptor);
            x.AddDbContext<InvoiceDbContext>(options =>
            {
                options.UseSqlServer(ConnectionString);
            });

            using var scope = x.BuildServiceProvider().CreateScope();
            var scopeServices = scope.ServiceProvider;
            var dbContext = scopeServices.GetRequiredService<InvoiceDbContext>();

            Utilities.InitialiseDatabase(dbContext);
        });

        //builder.ConfigureAppConfiguration((context, config) =>
        //{
        //    config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false);
        //});
        //builder.UseEnvironment("Development");
    }
}
