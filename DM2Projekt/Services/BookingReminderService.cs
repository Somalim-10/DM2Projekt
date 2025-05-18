using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Services;

public class BookingReminderService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BookingReminderService> _logger;

    public BookingReminderService(IServiceProvider services, ILogger<BookingReminderService> logger)
    {
        _services = services;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // do the thing
            await RunReminderCheckAsync();

            // wait 30 mins and then do the thing again
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }

    public async Task RunReminderCheckAsync()
    {
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DM2ProjektContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        var now = DateTime.Now;
        var in24Hours = now.AddHours(24);

        // grab bookings that are coming up in the next 24 hours and haven’t been reminded yet
        var upcomingBookings = await GetBookingsThatNeedRemindersAsync(context, now, in24Hours);

        foreach (var booking in upcomingBookings)
        {
            try
            {
                await SendRemindersToGroupAsync(emailService, booking);
                booking.ReminderSent = true; // so we don’t spam them next time
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"⚠️ Reminder failed for booking {booking.BookingId}. Skipping it.");
            }
        }

        await context.SaveChangesAsync(); // save the updated ReminderSent flags
    }

    private async Task<List<Booking>> GetBookingsThatNeedRemindersAsync(DM2ProjektContext context, DateTime now, DateTime cutoff)
    {
        return await context.Booking
            .Include(b => b.Room)
            .Include(b => b.Group)
                .ThenInclude(g => g.UserGroups)
                    .ThenInclude(ug => ug.User)
            .Where(b =>
                b.StartTime > now &&
                b.StartTime <= cutoff &&
                !b.ReminderSent)
            .ToListAsync();
    }

    private async Task SendRemindersToGroupAsync(EmailService emailService, Booking booking)
    {
        var users = booking.Group.UserGroups.Select(ug => ug.User).ToList();

        foreach (var user in users)
        {
            // every single user gets their own email
            await emailService.SendReminderEmailAsync(
                user.Email,
                user.FirstName,
                booking.Room.RoomName,
                booking.StartTime!.Value
            );

            _logger.LogInformation($"✅ Sent reminder to {user.Email} for booking {booking.BookingId}");
        }
    }
}
