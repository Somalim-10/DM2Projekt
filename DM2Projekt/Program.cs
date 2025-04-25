using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DM2Projekt.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<DM2ProjektContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DM2ProjektContext")
        ?? throw new InvalidOperationException("Connection string 'DM2ProjektContext' not found.")));

// Add session support
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DM2ProjektContext>();

    context.Database.Migrate(); // Apply migrations
    SeedData.Initialize(services); // Seed database
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Enable session middleware
app.UseSession();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
