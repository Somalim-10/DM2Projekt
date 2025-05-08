using DM2Projekt.Data;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Services;

// this service is always running in the background while the app is alive
// it looks for upcoming bookings and emails all users in the group
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
            using var scope = _services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<DM2ProjektContext>();
            var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();

            var now = DateTime.Now;
            var tomorrow = now.AddHours(24);

            // grab bookings starting in next 24h that haven't been reminded yet
            var bookings = await context.Booking
                .Include(b => b.Room)
                .Include(b => b.Group)
                    .ThenInclude(g => g.UserGroups)
                        .ThenInclude(ug => ug.User)
                .Where(b =>
                    b.StartTime > now &&
                    b.StartTime <= tomorrow &&
                    !b.ReminderSent)
                .ToListAsync();

            foreach (var booking in bookings)
            {
                try
                {
                    var groupUsers = booking.Group.UserGroups.Select(ug => ug.User).ToList();

                    foreach (var user in groupUsers)
                    {
                        var firstName = user.FirstName;

                        // fire off the email to this user
                        await emailService.SendReminderEmailAsync(
                            toEmail: user.Email,
                            firstName: firstName,
                            roomName: booking.Room.RoomName,
                            startTime: booking.StartTime!.Value
                        );

                        _logger.LogInformation($"✅ Reminder sent to {user.Email} for booking {booking.BookingId}");
                    }

                    // only mark as reminded once *everyone* has been emailed
                    booking.ReminderSent = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"⚠️ Failed to send reminders for booking {booking.BookingId}");
                }
            }

            await context.SaveChangesAsync();

            // wait 30 min before doing this all over again
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken);
        }
    }
}
