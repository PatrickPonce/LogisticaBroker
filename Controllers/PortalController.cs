using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LogisticaBroker.Models;

namespace LogisticaBroker.Controllers
{
    [Authorize(Roles = Roles.Client)] // <--- ¡SOLO CLIENTES!
    public class PortalController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PortalController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Método auxiliar para obtener el ID del cliente actual
        private async Task<int?> GetCurrentClientIdAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return null;

            var client = await _context.Clients.FirstOrDefaultAsync(c => c.UserId == user.Id);
            return client?.Id;
        }

        // GET: Portal (Dashboard del Cliente)
        public async Task<IActionResult> Index()
        {
            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null)
            {
                return View("ErrorAccess", "No se encontró un perfil de cliente asociado a tu usuario.");
            }

            // Cargar SOLO los despachos de ESTE cliente
            var myDispatches = await _context.Dispatches
                .Include(d => d.Timeline.OrderByDescending(t => t.ChangedAt).Take(1)) // Último estado
                .Where(d => d.ClientId == clientId)
                .OrderByDescending(d => d.UpdatedAt)
                .ToListAsync();

            return View(myDispatches);
        }

        // GET: Portal/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var clientId = await GetCurrentClientIdAsync();
            if (clientId == null) return Forbid(); // Seguridad extra

            var dispatch = await _context.Dispatches
                .Include(d => d.Documents)
                .Include(d => d.Timeline)
                    .ThenInclude(t => t.ChangedBy)
                .Include(d => d.Timeline)           // <--- NUEVO:
                    .ThenInclude(t => t.Documents)
                .Include(d => d.Payments)
                .FirstOrDefaultAsync(m => m.Id == id && m.ClientId == clientId); // <--- FILTRO CRÍTICO DE SEGURIDAD

            if (dispatch == null) return NotFound();

            return View(dispatch);
        }
    }
}