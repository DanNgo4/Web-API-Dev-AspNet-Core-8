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

// AddScoped() method indicating that the service is created once per client request and disposed of upon completion of the request
builder.Services.AddScoped<IScopedService, ScopedService>();
// AddTransient() method indicating that the service is created each time IT is requested and disposed of at the end of the request
builder.Services.AddTransient<ITransientService, TransientService>();
// AddSingleton() method indicating the service instance will be the same through application lifetime
builder.Services.AddSingleton<ISingletonService, SingletonService>();

var app = builder.Build();

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
