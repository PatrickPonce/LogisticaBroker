using Microsoft.AspNetCore.Mvc;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using Microsoft.AspNetCore.Authorization;
using LogisticaBroker.Services; // Para notificar

namespace LogisticaBroker.Controllers
{
    [Authorize(Roles = Roles.Admin)]
    public class DispatchCostsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DispatchCostsController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("DispatchId,Concept,Amount,DueDate,Notes")] DispatchCost cost, 
            IFormFile facturaFile) // <--- El archivo de la factura
        {
            if (facturaFile == null || facturaFile.Length == 0)
            {
                ModelState.AddModelError("facturaFile", "Debe adjuntar el documento de la factura.");
            }

            if (ModelState.IsValid)
            {
                // 1. Guardar el archivo físico
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
                
                var uniqueFileName = Guid.NewGuid().ToString() + "_" + facturaFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await facturaFile.CopyToAsync(stream);
                }

                // 2. Crear el registro del Documento
                var newDocument = new Document
                {
                    DispatchId = cost.DispatchId,
                    FileName = facturaFile.FileName,
                    FilePath = "/uploads/" + uniqueFileName,
                    ContentType = facturaFile.ContentType,
                    FileSize = facturaFile.Length,
                    DocType = Models.Enums.DocumentType.Invoice, // Factura
                    UploadedAt = DateTime.UtcNow
                };
                _context.Documents.Add(newDocument);
                await _context.SaveChangesAsync();

                // 3. Vincular Documento al Costo y guardar Costo
                cost.DocumentId = newDocument.Id;
                cost.CreatedAt = DateTime.UtcNow;
                _context.DispatchCosts.Add(cost);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Costo registrado y factura subida.";
            } 
            else 
            {
                TempData["Error"] = "Error al registrar el costo. Revise los campos.";
            }
            return RedirectToAction("Details", "Dispatches", new { id = cost.DispatchId });
        }
        
        // (Podrías añadir un método Delete si lo necesitas)
    }
}