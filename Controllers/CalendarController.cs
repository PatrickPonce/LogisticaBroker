using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LogisticaBroker.Controllers
{
    [Authorize]
    public class CalendarController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CalendarController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Calendar/GetEvents
        // Este método alimenta al FullCalendar
        public async Task<IActionResult> GetEvents(DateTime start, DateTime end)
        {
            // Si es cliente, solo ve sus propios eventos relacionados a sus despachos
            var user = await _userManager.GetUserAsync(User);
            var isClient = await _userManager.IsInRoleAsync(user!, "Client");

            var query = _context.CalendarEvents.AsQueryable();

            if (isClient)
            {
                // Lógica para cliente: eventos donde el despacho le pertenece
                // (Simplificado para este ejemplo, podrías refinarlo)
                 query = query.Include(e => e.Dispatch)
                              .Where(e => e.Dispatch != null && e.Dispatch.Client != null && e.Dispatch.Client.UserId == user!.Id);
            }

            var events = await query
                .Where(e => e.Start >= start && e.Start <= end)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.Start.ToString("yyyy-MM-ddTHH:mm:ss"), // Formato ISO vital para FullCalendar
                    end = e.End.HasValue ? e.End.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    dispatchId = e.DispatchId,
                    color = e.Color,
                    description = e.Description,
                    dispatchNumber = e.Dispatch != null ? e.Dispatch.DispatchNumber : ""
                })
                .ToListAsync();

            return Json(events);
        }

        // POST: /Calendar/Create
        [HttpPost]
        [Authorize(Roles = "Admin")] // Solo admins crean eventos por ahora
        public async Task<IActionResult> Create([FromBody] CalendarEvent calendarEvent)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                calendarEvent.CreatedById = user!.Id;
                calendarEvent.CreatedAt = DateTime.UtcNow;

                // Asegurar UTC si Postgres reclama
                calendarEvent.Start = DateTime.SpecifyKind(calendarEvent.Start, DateTimeKind.Utc);
                if (calendarEvent.End.HasValue)
                    calendarEvent.End = DateTime.SpecifyKind(calendarEvent.End.Value, DateTimeKind.Utc);

                _context.Add(calendarEvent);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Evento creado" });
            }
            return Json(new { success = false, message = "Datos inválidos" });
        }

        // GET: /Calendar/GetActiveDispatches
        [HttpGet]
        public async Task<IActionResult> GetActiveDispatches()
        {
            try
            {
                // 1. Traer los datos crudos de la BD (solo lo necesario)
                var rawDispatches = await _context.Dispatches
                    .AsNoTracking()
                    .Include(d => d.Client)
                    .Where(d => d.Status != Models.Enums.DispatchStatus.Completed)
                    // Seleccionamos solo los campos que necesitamos para evitar traer todo el objeto
                    .Select(d => new
                    {
                        d.Id,
                        d.DispatchNumber,
                        ClientName = d.Client != null ? d.Client.CompanyName : "Sin Cliente"
                    })
                    .ToListAsync(); // <-- EJECUTAMOS LA CONSULTA AQUÍ

                // 2. Hacer la transformación de texto en memoria (ya fuera de la BD)
                var finalResult = rawDispatches
                    .Select(d => new
                    {
                        id = d.Id,
                        text = $"{d.DispatchNumber} - {d.ClientName}"
                    })
                    .OrderBy(d => d.text) // Ordenamos en memoria
                    .ToList();

                return Json(finalResult);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR GETTING DISPATCHES]: {ex.Message}");
                // Es útil ver también el InnerException si existe
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[INNER ERROR]: {ex.InnerException.Message}");
                }
                return StatusCode(500, new { error = "Error interno al cargar despachos" });
            }
        }
        
        // GET: /Calendar/GetUpcomingTasks
        [HttpGet]
        public async Task<IActionResult> GetUpcomingTasks(string? filter)
        {
            var user = await _userManager.GetUserAsync(User);
            var isClient = await _userManager.IsInRoleAsync(user!, "Client");

            var query = _context.CalendarEvents
                .Include(e => e.Dispatch)
                .Where(e => !e.IsCompleted); // Solo tareas pendientes

            // Si es cliente, filtrar solo las suyas
            if (isClient)
            {
                query = query.Where(e => e.Dispatch != null && e.Dispatch.Client != null && e.Dispatch.Client.UserId == user!.Id);
            }

            // Filtro de texto (busca en título o número de despacho)
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
                query = query.Where(e => 
                    e.Title.ToLower().Contains(filter) || 
                    (e.Dispatch != null && e.Dispatch.DispatchNumber.ToLower().Contains(filter))
                );
            }

            var tasks = await query
                .OrderBy(e => e.Start) // Las más próximas primero
                .Take(10) // Limitamos a 10 para no saturar
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                    end = e.End.HasValue ? e.End.Value.ToString("yyyy-MM-ddTHH:mm:ss") : null,
                    description = e.Description,
                    dispatchNumber = e.Dispatch != null ? e.Dispatch.DispatchNumber : null,
                    dispatchId = e.DispatchId,
                    color = e.Color,
                    isOverdue = e.Start < DateTime.UtcNow // Para marcar en rojo si está vencida
                })
                .ToListAsync();

            return Json(tasks);
        }

        // POST: /Calendar/ToggleComplete
        [HttpPost]
        public async Task<IActionResult> ToggleComplete(int id)
        {
            var evt = await _context.CalendarEvents.FindAsync(id);
            if (evt == null) return NotFound();

            evt.IsCompleted = !evt.IsCompleted;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: /Calendar/GetCompletedTasks
        [HttpGet]
        public async Task<IActionResult> GetCompletedTasks(string? filter)
        {
            var user = await _userManager.GetUserAsync(User);
            var isClient = await _userManager.IsInRoleAsync(user!, "Client");

            var query = _context.CalendarEvents
                .Include(e => e.Dispatch)
                .Where(e => e.IsCompleted == true); // <-- SOLO COMPLETADAS

            // Si es cliente, filtrar solo las suyas
            if (isClient)
            {
                query = query.Where(e => e.Dispatch != null && e.Dispatch.Client != null && e.Dispatch.Client.UserId == user!.Id);
            }

            // Filtro de texto
            if (!string.IsNullOrEmpty(filter))
            {
                filter = filter.ToLower();
                query = query.Where(e =>
                    e.Title.ToLower().Contains(filter) ||
                    (e.Dispatch != null && e.Dispatch.DispatchNumber.ToLower().Contains(filter))
                );
            }

            // Ordenar por las más recientes primero y limitar
            var tasks = await query
                .OrderByDescending(e => e.Start)
                .Take(10)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.Start.ToString("yyyy-MM-ddTHH:mm:ss"),
                    dispatchNumber = e.Dispatch != null ? e.Dispatch.DispatchNumber : null,
                    dispatchId = e.DispatchId,
                    color = e.Color
                })
                .ToListAsync();

            return Json(tasks);
        }
        
        // POST: /Calendar/DeleteEvent/5
        [HttpPost]
        [Authorize(Roles = Roles.Admin)] // Solo los Admins pueden eliminar
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var evt = await _context.CalendarEvents.FindAsync(id);
            if (evt == null) 
            {
                return NotFound(new { success = false, message = "Evento no encontrado." });
            }

            // Aquí podrías añadir lógica extra si es necesario (ej. verificar permisos)

            _context.CalendarEvents.Remove(evt);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Evento eliminado." });
        }

    }
}