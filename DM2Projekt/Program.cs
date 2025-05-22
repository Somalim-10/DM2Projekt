using Microsoft.EntityFrameworkCore;
using DM2Projekt.Data;

var builder = WebApplication.CreateBuilder(args);

// load appsettings.json no matter what
// if Local.json is there, it overrides the stuff above
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

// razor pages. needed for web stuff
builder.Services.AddRazorPages();

// set up db connection
// if Local.json exists, that one wins
// if not, fallback in appsettings.json gets used
builder.Services.AddDbContext<DM2ProjektContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DM2ProjektContext")
        ?? throw new InvalidOperationException("No DB string found. Add one to appsettings.")));

// session settings. keeps stuff in memory for a bit
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// grab http context anywhere
builder.Services.AddHttpContextAccessor();

// background email job. runs by itself
builder.Services.AddHostedService<DM2Projekt.Services.BookingReminderService>();

// email sending service
builder.Services.AddTransient<DM2Projekt.Services.EmailService>();

var app = builder.Build();

// do db migrations and seed data when starting up
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DM2ProjektContext>();

    context.Database.Migrate();
    SeedData.Initialize(services);
}

// custom error stuff. only when not in dev mode
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