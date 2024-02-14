using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Secret8;
using Secret8.Migrations;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlite("Data Source=data.db", ctxOptions =>
    {
        ctxOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });
});
builder.Services.AddAsyncInitializer<MigrationInitializer>();

builder.Services.AddIdentityApiEndpoints<AppUser>(options =>
    {
        options.Password = new PasswordOptions()
        {
            RequireDigit = false,
            RequiredLength = 8,
            RequiredUniqueChars = 4,
            RequireLowercase = false,
            RequireNonAlphanumeric = false,
            RequireUppercase = false,
        };
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();
await app.InitAsync();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapGroup("/identity").MapIdentityApi<AppUser>();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}