using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;

var builder = WebApplication.CreateBuilder(args);

// Load config files:
// - appsettings.json is always loaded (this one’s public, no secrets here)
// - appsettings.Local.json is optional, only on your machine with secrets
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// Add Razor Pages support — gotta have those web pages!
builder.Services.AddRazorPages();

// Hook up your DB context using connection string from config
builder.Services.AddDbContext<DM2ProjektContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DM2ProjektContext")
        ?? throw new InvalidOperationException("No connection string found — please check your config!")));

// Set up session — keep users logged in for 30 minutes
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Make HttpContext easy to access anywhere
builder.Services.AddHttpContextAccessor();

// Background service to send email reminders — automatic, nice!
builder.Services.AddHostedService<DM2Projekt.Services.BookingReminderService>();

// Email service for sending out those emails
builder.Services.AddTransient<DM2Projekt.Services.EmailService>();

var app = builder.Build();

// Run migrations and seed some starter data — ready to roll!
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DM2ProjektContext>();

    context.Database.Migrate(); // apply any pending DB changes
    SeedData.Initialize(services); // add starter data if needed
}

// Error handling if not in dev mode — keep it smooth for users
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
