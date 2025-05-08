using DM2Projekt.Data;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Services;

// this background service quietly runs in the background while the app is up
// its job: check for upcoming bookings and send out reminder emails
public class BookingReminderService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<BookingReminderService> _logger;

    public BookingReminderService(IServiceProvider services, ILogger<BookingReminderService> logger)
    {
        _services = services;
        _logger = logger;
    }

    // this method is the main event — we use it to check for bookings and send reminders
    // it’s called by the background loop below, but we can also call it manually in tests, etc.
    public async Task RunReminderCheckAsync()
    {
        // grab a scoped version of our DB + services (since we're in a background thread)
        using var scope = _services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DM2ProjektContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

        var now = DateTime.Now;
        var tomorrow = now.AddHours(24); // 24 hours ahead

        // look for bookings happening in the next 24 hours that haven't been reminded yet
        var bookings = await context.Booking
            .Include(b => b.Room) // we need room name for the email
            .Include(b => b.Group) // need the group to get users
                .ThenInclude(g => g.UserGroups) // group->user mapping
                    .ThenInclude(ug => ug.User) // grab the actual user info
            .Where(b =>
                b.StartTime > now &&
                b.StartTime <= tomorrow &&
                !b.ReminderSent)
            .ToListAsync();

        // loop through each booking that needs a reminder
        foreach (var booking in bookings)
        {
            try
            {
                // get the actual users who are part of this group
                var users = booking.Group.UserGroups.Select(ug => ug.User).ToList();

                foreach (var user in users)
                {
                    // fire off a reminder email to each user
                    await emailService.SendReminderEmailAsync(
                        toEmail: user.Email,
                        firstName: user.FirstName,
                        roomName: booking.Room.RoomName,
                        startTime: booking.StartTime!.Value
                    );

                    // log that we sent it (optional but nice for debugging)
                    _logger.LogInformation($"✅ Reminder sent to {user.Email} for booking {booking.BookingId}");
                }

                // mark this booking as "reminder sent" so we don’t double-send later
                booking.ReminderSent = true;
            }
            catch (Exception ex)
            {
                // if anything breaks (like email fails), log it and move on
                _logger.LogError(ex, $"⚠️ Failed to send reminders for booking {booking.BookingId}");
            }
        }

        // save the "ReminderSent = true" changes back to the database
        await context.SaveChangesAsync();
    }

    // this is the thing that runs in the background automatically
    // it just loops forever (or until the app shuts down), checking every 30 mins
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // check for bookings and send reminders
            await RunReminderCheckAsync();

            // wait 30 minutes before running again — tweak this if needed
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
