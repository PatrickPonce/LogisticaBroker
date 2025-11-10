using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using LogisticaBroker.Models.ViewModels;
using LogisticaBroker.Models.Enums;
using Microsoft.AspNetCore.Authorization;

namespace LogisticaBroker.Controllers
{
    [Authorize] // Forzamos que deba iniciar sesión para ver el Dashboard
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Preparar el ViewModel
            var viewModel = new DashboardViewModel();

            // 2. Obtener contadores (KPIs)
            viewModel.TotalClients = await _context.Clients.CountAsync();
            
            // Consideramos "Activos" a los que NO están completados ni liberados
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
                .OrderByDescending(d => d.UpdatedAt) // Ordenar por última actualización
                .Take(5)
                .ToListAsync();

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