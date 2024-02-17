using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Secret8;
using Secret8.Migrations;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateOnBuild = true;
    options.ValidateScopes = true;
});

builder.Host.UseSerilog((hostContext, options) =>
{
    options.ReadFrom.Configuration(hostContext.Configuration);
    options.WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
});

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
        
        // options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthentication(options =>
    {
        // options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
    // .AddCookie(options =>
    // {
    //     options.Cookie.Name = "Discord";
    // })
    .AddDiscord(options =>
    {
        var section = builder.Configuration.GetRequiredSection("Discord");
        options.ClientId = section["ClientId"]!;
        options.ClientSecret = section["ClientSecret"]!;

        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
        options.CorrelationCookie.Name = "Correlation.";

        options.SaveTokens = true;
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();

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
app.MapGet("/", () => $"Hello: {Random.Shared.Next(100)}");
app.MapGet("/secure", () => "Secure.").RequireAuthorization();
app.MapControllers();

app.Run();
