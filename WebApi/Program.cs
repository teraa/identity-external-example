using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApi;
using WebApi.Migrations;
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

builder.Services
    .AddIdentity<AppUser, IdentityRole>(options =>
    {
        options.User.AllowedUserNameCharacters += ":";
        // options.SignIn.RequireConfirmedAccount = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    // .AddUserConfirmation<>()
    .Services
    .ConfigureApplicationCookie(options => { options.Cookie.Name = "Auth"; })
    .ConfigureExternalCookie(options => { options.Cookie.Name = "External"; })
    ;

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultChallengeScheme = DefaultAuthenticationHandler.SchemeName;
        options.DefaultForbidScheme = DefaultAuthenticationHandler.SchemeName;
        options.AddScheme<DefaultAuthenticationHandler>(DefaultAuthenticationHandler.SchemeName, null);
    })
    .AddDiscord(options =>
    {
        var section = builder.Configuration.GetRequiredSection("Discord");
        options.ClientId = section["ClientId"]!;
        options.ClientSecret = section["ClientSecret"]!;

        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
        options.CorrelationCookie.Name = "Correlation.";

        // options.SaveTokens = true;
        // options.SignInScheme = IdentityConstants.ApplicationScheme;
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

app.MapGet("/", () => $"Hello: {Random.Shared.Next(100)}");
app.MapGet("/secure", () => "Secure.").RequireAuthorization();
app.MapControllers();

app.Run();
