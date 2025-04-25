using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DM2Projekt.Data;
using DM2Projekt.Models;

namespace DM2Projekt.Pages.Rooms
{
    public class CreateModel : PageModel
    {
        private readonly DM2ProjektContext _context;

        public CreateModel(DM2ProjektContext context)
        {
            _context = context;
        }

        public IActionResult OnGet() => Page();

        [BindProperty]
        public Room Room { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await AddRoomWithSmartboardAsync(Room);

            return RedirectToPage("./Index");
        }

        private async Task AddRoomWithSmartboardAsync(Room room)
        {
            _context.Room.Add(room);
            await _context.SaveChangesAsync();

            var smartboard = new Smartboard
            {
                RoomId = room.RoomId,
                IsAvailable = true
            };

            _context.Smartboard.Add(smartboard);
            await _context.SaveChangesAsync();
        }
    }
}
