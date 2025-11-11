using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace LogisticaBroker.Controllers
{
    [Authorize]
    public class DocumentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<ApplicationUser> _userManager;

        public DocumentsController(ApplicationDbContext context, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _env = env;
            _userManager = userManager;
        }

        // POST: Documents/Upload
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(int dispatchId, Document document, IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Por favor seleccione un archivo válido.";
                return RedirectToAction("Details", "Dispatches", new { id = dispatchId });
            }

            // 1. Crear ruta de guardado única
            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            // Usamos Guid para evitar nombres duplicados
            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            // 2. Guardar archivo en disco
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // 3. Guardar metadatos en BD
            document.FileName = file.FileName;
            document.FilePath = "/uploads/" + uniqueFileName; // Ruta relativa para web
            document.ContentType = file.ContentType;
            document.FileSize = file.Length;
            document.DispatchId = dispatchId;
            document.UploadedAt = DateTime.UtcNow;

            _context.Add(document);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Documento subido correctamente.";
            return RedirectToAction("Details", "Dispatches", new { id = dispatchId });
        }

        // GET: Documents/Download/5
        public async Task<IActionResult> Download(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents.FindAsync(id);
            if (document == null) return NotFound();

            var filePath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound("El archivo físico no existe en el servidor.");

            return PhysicalFile(filePath, document.ContentType ?? "application/octet-stream", document.FileName);
        }

        // GET: Documents/Preview/5
        [HttpGet]
        public async Task<IActionResult> Preview(int? id)
        {
            if (id == null) return NotFound();

            var document = await _context.Documents
                .Include(d => d.Dispatch) // Incluimos el despacho para seguridad
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (document == null) return NotFound();

            // --- Verificación de Seguridad (Opcional pero RECOMENDADA) ---
            // Asegurarnos que el cliente solo vea sus propios documentos
            if (User.IsInRole(Roles.Client))
            {
                var user = await _userManager.GetUserAsync(User);
                var client = await _context.Clients
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.UserId == user.Id);
                    
                if (document.Dispatch?.ClientId != client?.Id)
                {
                    return Forbid(); // El documento no le pertenece
                }
            }
            // --- Fin Verificación de Seguridad ---

            var filePath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
                return NotFound("El archivo físico no existe en el servidor.");

            // Al no pasar un 'fileName' (tercer argumento), el navegador
            // intentará mostrarlo (Content-Disposition: inline)
            return PhysicalFile(filePath, document.ContentType ?? "application/octet-stream");
        }

        // POST: Documents/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var document = await _context.Documents.FindAsync(id);
            if (document != null)
            {
                // Opcional: Borrar también el archivo físico
                var physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(physicalPath))
                {
                    System.IO.File.Delete(physicalPath);
                }

                _context.Documents.Remove(document);
                await _context.SaveChangesAsync();
            }
            // Redirigir al despacho al que pertenecía
            return RedirectToAction("Details", "Dispatches", new { id = document?.DispatchId });
        }
    }
}