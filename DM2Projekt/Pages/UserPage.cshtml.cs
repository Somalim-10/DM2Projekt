using DM2Projekt.Data;
using DM2Projekt.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace DM2Projekt.Pages
{
    public class UserPageModel : PageModel
    {
        private readonly DM2ProjektContext _context;

        public UserPageModel(DM2ProjektContext context)
        {
            _context = context;
        }

        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserRole { get; set; }

        public List<Booking> UpcomingBookings { get; set; } = new();
        public List<Group> UserGroups { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                // get user info
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId.Value);
                if (user != null)
                {
                    UserName = $"{user.FirstName} {user.LastName}";
                    UserEmail = user.Email;
                    UserRole = user.Role.ToString();
                }

                // get upcoming bookings
                var now = DateTime.Now;
                UpcomingBookings = await _context.Booking
                    .Include(b => b.Room)
                    .Include(b => b.Group)
                    .Where(b => b.CreatedByUserId == userId && b.EndTime > now)
                    .OrderBy(b => b.StartTime)
                    .ToListAsync();

                // get user's groups
                UserGroups = await _context.UserGroup
                    .Where(ug => ug.UserId == userId)
                    .Include(ug => ug.Group)
                    .Select(ug => ug.Group)
                    .ToListAsync();
            }
        }
    }
}
