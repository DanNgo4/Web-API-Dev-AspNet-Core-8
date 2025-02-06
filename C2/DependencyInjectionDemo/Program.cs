using DependencyInjectionDemo;
using DependencyInjectionDemo.Interfaces;
using DependencyInjectionDemo.Services;

// Entry point of the application
var builder = WebApplication.CreateBuilder(args);

// Configure the application by using the "builder" instance
// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPostService, PostsService>();

builder.Services.AddScoped<IDemoService, DemoService>();

// using extension method for the IServiceCollection interface to register all IService services at once (group registration)
builder.Services.AddLifetimeServices();

// regiter keyed/named services to the service container
builder.Services.AddKeyedScoped<IDataService, SqlDatabaseService>("sqlDatabaseService");
builder.Services.AddKeyedScoped<IDataService, CosmosDatabaseService>("cosmosDatabaseService");

var app = builder.Build();

// creates a scope and resolves the IDemoService service from the service container
// then it can use the service to do something
// after the scope is disposed of, the service will be disposed as well
using (var serviceScope = app.Services.CreateScope())
{
    var services = serviceScope.ServiceProvider;

    var demoService = services.GetRequiredService<IDemoService>();
    var message = demoService.SayHello();
    Console.WriteLine(message);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
