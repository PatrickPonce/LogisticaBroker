using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using LogisticaBroker.Models.Enums;
using LogisticaBroker.Services;
using X.PagedList;    // Para las interfaces básicas
using X.PagedList.EF;
using ClosedXML.Excel;
using System.IO;

namespace LogisticaBroker.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class DispatchesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public DispatchesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: Dispatches
        public async Task<IActionResult> Index(string? searchTerm, int? clientId, DispatchStatus? status, int? page)
        {
            // 1. Iniciar la consulta base (aún no se ejecuta en BD)
            var query = _context.Dispatches
                .Include(d => d.Client)
                .AsQueryable();

            // 2. Aplicar filtros si existen
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                // Busca por número de despacho, BL o contenedor
                query = query.Where(d => 
                    d.DispatchNumber.ToLower().Contains(searchTerm) ||
                    (d.BLNumber != null && d.BLNumber.ToLower().Contains(searchTerm)) ||
                    (d.ContainerNumber != null && d.ContainerNumber.ToLower().Contains(searchTerm))
                );
            }

            if (clientId.HasValue)
            {
                query = query.Where(d => d.ClientId == clientId);
            }

            if (status.HasValue)
            {
                query = query.Where(d => d.Status == status);
            }

            // 3. Ordenar por defecto (los más nuevos primero)
            query = query.OrderByDescending(d => d.CreatedAt);

            // 4. Preparar datos para los filtros en la vista
            ViewData["ClientId"] = new SelectList(_context.Clients.OrderBy(c => c.CompanyName), "Id", "CompanyName", clientId);
            ViewBag.CurrentSearch = searchTerm;
            ViewBag.CurrentClient = clientId;
            ViewBag.CurrentStatus = status;

            // 5. Paginación
            int pageSize = 10; // Número de registros por página
            int pageNumber = (page ?? 1);
            
            // ToPagedListAsync ejecuta la consulta final optimizada
            var pagedModel = await query.ToPagedListAsync(pageNumber, pageSize);

            return View(pagedModel);
        }

        // GET: Dispatches/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var dispatch = await _context.Dispatches
                .Include(d => d.Client) // Importante para ver quién es el cliente en detalles
                .Include(d => d.Documents)
                .Include(d => d.Timeline) // <--- CARGAR EL TIMELINE
                    .ThenInclude(t => t.ChangedBy)
                .Include(d => d.Timeline)           // <--- NUEVO: Incluir...
                    .ThenInclude(t => t.Documents)
                .Include(d => d.Payments)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dispatch == null) return NotFound();

            dispatch.Timeline = dispatch.Timeline.OrderByDescending(t => t.ChangedAt).ToList();

            return View(dispatch);
        }

        // GET: Dispatches/Create
        public IActionResult Create()
        {
            // Preparamos la lista desplegable de clientes
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName");
            return View();
        }

        // POST: Dispatches/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DispatchNumber,BLNumber,ClientId,Supplier,ShippingLine,ArrivalDate,Channel,Status,ContainerNumber,Port,Weight,Value")] Dispatch dispatch)
        {
            if (ModelState.IsValid)
            {
                _context.Add(dispatch);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // Si algo falla, recargamos la lista de clientes
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", dispatch.ClientId);
            return View(dispatch);
        }

        // GET: Dispatches/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var dispatch = await _context.Dispatches.FindAsync(id);
            if (dispatch == null) return NotFound();

            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", dispatch.ClientId);
            return View(dispatch);
        }

        // POST: Dispatches/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DispatchNumber,BLNumber,ClientId,Supplier,ShippingLine,ArrivalDate,Channel,Status,ContainerNumber,Port,Weight,Value,CreatedAt")] Dispatch dispatch)
        {
            if (id != dispatch.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    // Aseguramos que la fecha de actualización sea correcta
                    dispatch.UpdatedAt = DateTime.UtcNow; 
                    
                    _context.Update(dispatch);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DispatchExists(dispatch.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ClientId"] = new SelectList(_context.Clients, "Id", "CompanyName", dispatch.ClientId);
            return View(dispatch);
        }

        // GET: Dispatches/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var dispatch = await _context.Dispatches
                .Include(d => d.Client)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dispatch == null) return NotFound();

            return View(dispatch);
        }

        // POST: Dispatches/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var dispatch = await _context.Dispatches.FindAsync(id);
            if (dispatch != null)
            {
                _context.Dispatches.Remove(dispatch);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool DispatchExists(int id)
        {
            return _context.Dispatches.Any(e => e.Id == id);
        }

        // POST: Dispatches/ChangeStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeStatus(int dispatchId, DispatchStatus newStatus, string? notes, int[] selectedDocs)
        {
            var dispatch = await _context.Dispatches.FindAsync(dispatchId);
            if (dispatch == null) return NotFound();

            // Solo registramos si el estado es diferente o si hay una nota
            if (dispatch.Status != newStatus || !string.IsNullOrEmpty(notes) || (selectedDocs != null && selectedDocs.Any()))
            {
                var currentUser = await _userManager.GetUserAsync(User);

                // 1. Crear registro en el timeline
                var timelineEntry = new DispatchTimeline
                {
                    DispatchId = dispatchId,
                    Status = newStatus,
                    Notes = notes,
                    ChangedById = currentUser?.Id,
                    ChangedAt = DateTime.UtcNow
                };

                // 2. Asociar documentos seleccionados si los hay
                if (selectedDocs != null && selectedDocs.Any())
                {
                    // Buscamos los documentos en la BD para asegurarnos de que existen y pertenecen a este despacho
                    var docsToLink = await _context.Documents
                        .Where(d => selectedDocs.Contains(d.Id) && d.DispatchId == dispatchId)
                        .ToListAsync();

                    foreach (var doc in docsToLink)
                    {
                        timelineEntry.Documents.Add(doc);
                    }
                }

                _context.Add(timelineEntry);

                // 3. Actualizar el despacho si cambió el estado
                if (dispatch.Status != newStatus)
                {
                    dispatch.Status = newStatus;
                    dispatch.UpdatedAt = DateTime.UtcNow;
                    _context.Update(dispatch);
                }

                await _context.SaveChangesAsync();

                // --- NUEVO: NOTIFICAR AL CLIENTE ---
                // 1. Averiguar quién es el usuario del cliente
                var client = await _context.Clients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == dispatch.ClientId);

                if (client != null && !string.IsNullOrEmpty(client.UserId))
                {
                    await _notificationService.NotifyUserAsync(client.UserId, "Actualización de Estado", $"Su despacho {dispatch.DispatchNumber} ahora está: {newStatus}");
                }
                // ------------------------------------

                TempData["Success"] = "Estado actualizado y cliente notificado.";
            }

            return RedirectToAction(nameof(Details), new { id = dispatchId });
        }

        // GET: Dispatches/ExportToExcel
        public async Task<IActionResult> ExportToExcel(string? searchTerm, int? clientId, DispatchStatus? status)
        {
            // 1. Reutilizamos la misma lógica de filtrado que el Index
            var query = _context.Dispatches
                .Include(d => d.Client)
                .AsNoTracking() // Más rápido para solo lectura
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(d =>
                    d.DispatchNumber.ToLower().Contains(searchTerm) ||
                    (d.BLNumber != null && d.BLNumber.ToLower().Contains(searchTerm)) ||
                    (d.ContainerNumber != null && d.ContainerNumber.ToLower().Contains(searchTerm))
                );
            }
            if (clientId.HasValue) query = query.Where(d => d.ClientId == clientId);
            if (status.HasValue) query = query.Where(d => d.Status == status);

            // Ordenamos por fecha
            var dispatches = await query.OrderByDescending(d => d.CreatedAt).ToListAsync();

            // 2. Crear el archivo Excel usando ClosedXML
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Despachos");

                // Encabezados
                worksheet.Cell(1, 1).Value = "Nro. Despacho";
                worksheet.Cell(1, 2).Value = "Cliente";
                worksheet.Cell(1, 3).Value = "RUC";
                worksheet.Cell(1, 4).Value = "Estado";
                worksheet.Cell(1, 5).Value = "Canal";
                worksheet.Cell(1, 6).Value = "BL Number";
                worksheet.Cell(1, 7).Value = "Contenedor";
                worksheet.Cell(1, 8).Value = "Llegada";
                worksheet.Cell(1, 9).Value = "Naviera";

                // Estilo para el encabezado
                var headerRange = worksheet.Range("A1:I1");
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

                // Llenar datos
                int row = 2;
                foreach (var item in dispatches)
                {
                    worksheet.Cell(row, 1).Value = item.DispatchNumber;
                    worksheet.Cell(row, 2).Value = item.Client?.CompanyName;
                    worksheet.Cell(row, 3).Value = item.Client?.RUC;
                    // ToString() para los Enums muestra el texto
                    worksheet.Cell(row, 4).Value = item.Status.ToString();
                    worksheet.Cell(row, 5).Value = item.Channel.ToString();
                    worksheet.Cell(row, 6).Value = item.BLNumber;
                    worksheet.Cell(row, 7).Value = item.ContainerNumber;
                    worksheet.Cell(row, 8).Value = item.ArrivalDate.HasValue ? item.ArrivalDate.Value.ToLocalTime() : "";
                    worksheet.Cell(row, 9).Value = item.ShippingLine;
                    row++;
                }

                // Ajustar ancho de columnas automáticamente
                worksheet.Columns().AdjustToContents();

                // Preparar el archivo para descarga
                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    string fileName = $"Despachos_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";

                    return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                }
            }
        }
        
        // GET: Dispatches/Preview/5
        public async Task<IActionResult> Preview(int? id)
        {
            if (id == null) return NotFound();

            var dispatch = await _context.Dispatches
                .Include(d => d.Client)
                // Incluimos lo básico para mostrar en el modal
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (dispatch == null) return NotFound();

            // Devolvemos una Vista Parcial (solo el HTML interno del modal)
            return PartialView("_Preview", dispatch);
        }

    }
}