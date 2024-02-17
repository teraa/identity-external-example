using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RazorApp.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.Password = new PasswordOptions()
        {
            RequireDigit = false,
            RequiredLength = 1,
            RequireLowercase = false,
            RequireUppercase = false,
            RequiredUniqueChars = 1,
            RequireNonAlphanumeric = false,
        };

        options.User.RequireUniqueEmail = false;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.AddRazorPages();

builder.Services.AddAuthentication()
    .AddDiscord(options =>
    {
        var section = builder.Configuration.GetRequiredSection("Discord");
        options.ClientId = section["ClientId"]!;
        options.ClientSecret = section["ClientSecret"]!;

        options.CorrelationCookie.SameSite = SameSiteMode.Unspecified;
        options.CorrelationCookie.Name = "Correlation.";

        options.SaveTokens = true;
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
