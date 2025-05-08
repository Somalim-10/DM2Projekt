using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using DM2Projekt.Data;

var builder = WebApplication.CreateBuilder(args);

// add razor pages support
builder.Services.AddRazorPages();

// connect to db
builder.Services.AddDbContext<DM2ProjektContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DM2ProjektContext")
        ?? throw new InvalidOperationException("Connection string 'DM2ProjektContext' not found.")));

// session stuff
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// so we can use httpcontext easily
builder.Services.AddHttpContextAccessor();

// background service that sends email reminders
builder.Services.AddHostedService<DM2Projekt.Services.BookingReminderService>();

// email service for sending mails
builder.Services.AddTransient<DM2Projekt.Services.EmailService>();

var app = builder.Build();

// apply migrations and seed data
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<DM2ProjektContext>();

    context.Database.Migrate(); // migrate db if needed
    SeedData.Initialize(services); // put starter data
}

// error handling if prod
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// enable session
app.UseSession();

app.UseAuthorization();

// map razor pages
app.MapRazorPages();

app.Run();