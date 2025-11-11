using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using LogisticaBroker.Models.ViewModels;
using LogisticaBroker.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LogisticaBroker.Controllers
{
    [Authorize] // Forzamos que deba iniciar sesión para ver el Dashboard
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            // --- INICIO DE LA VALIDACIÓN DE ROL ---
            // Verificamos si el usuario actual tiene el rol "Client"
            if (User.IsInRole(Roles.Client))
            {
                // Si es Cliente, lo redirigimos inmediatamente a su portal.
                return RedirectToAction("Index", "Portal");
            }
            // --- FIN DE LA VALIDACIÓN DE ROL ---

            // Si llegamos aquí, significa que el usuario NO es un Cliente.
            // Como el controlador tiene [Authorize], asumimos que es un Admin.
            // (Puedes hacerlo más estricto con: if (User.IsInRole(Roles.Admin)))
            
            // 1. Preparar el ViewModel del Dashboard
            var viewModel = new DashboardViewModel();

            // 2. Obtener contadores (KPIs)
            viewModel.TotalClients = await _context.Clients.CountAsync();
            
            viewModel.ActiveDispatches = await _context.Dispatches
                .Where(d => d.Status != DispatchStatus.Completed && d.Status != DispatchStatus.Released)
                .CountAsync();

            viewModel.DispatchesInCustoms = await _context.Dispatches
                .Where(d => d.Status == DispatchStatus.Customs)
                .CountAsync();

            viewModel.CompletedDispatches = await _context.Dispatches
                .Where(d => d.Status == DispatchStatus.Completed)
                .CountAsync();

            // 3. Obtener los 5 despachos más recientes
            viewModel.RecentDispatches = await _context.Dispatches
                .Include(d => d.Client)
                .OrderByDescending(d => d.UpdatedAt) 
                .Take(5)
                .ToListAsync();

            // Devolvemos la vista del Panel de Control solo al Admin
            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}