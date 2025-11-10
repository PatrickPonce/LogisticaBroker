using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LogisticaBroker.Models;

namespace LogisticaBroker.Controllers
{
    [Authorize]
    public class NotificationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Notifications (Ver todas)
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var notifs = await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50) // Límite razonable
                .ToListAsync();

            return View(notifs);
        }

        // GET: Marcar como leída y redirigir
        public async Task<IActionResult> ReadAndRedirect(int id)
        {
            var userId = _userManager.GetUserId(User);
            var notif = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notif != null)
            {
                notif.IsRead = true;
                await _context.SaveChangesAsync();

                // Si tiene un despacho relacionado, vamos allá
                if (notif.RelatedDispatchId.HasValue)
                {
                    // Verificamos rol para saber a qué controller redirigir
                    if (User.IsInRole("Client"))
                    {
                        return RedirectToAction("Details", "Portal", new { id = notif.RelatedDispatchId });
                    }
                    return RedirectToAction("Details", "Dispatches", new { id = notif.RelatedDispatchId });
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}