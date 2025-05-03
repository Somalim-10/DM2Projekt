using DM2Projekt.Data;
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

        public async Task OnGetAsync()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId.HasValue)
            {
                var user = await _context.User.FirstOrDefaultAsync(u => u.UserId == userId.Value);
                if (user != null)
                {
                    UserName = $"{user.FirstName} {user.LastName}";
                    UserEmail = user.Email;
                    UserRole = user.Role.ToString();
                }
            }
        }
    }
}
