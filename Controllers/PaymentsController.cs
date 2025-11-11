using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using Microsoft.AspNetCore.Authorization;
using LogisticaBroker.Services;

namespace LogisticaBroker.Controllers
{
    [Authorize(Roles = Roles.Admin)] // Solo admins gestionan pagos
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env; // NUEVO
        private readonly NotificationService _notificationService; // NUEVO

        public PaymentsController(ApplicationDbContext context, IWebHostEnvironment env, NotificationService notificationService)
        {
            _context = context;
            _env = env;
            _notificationService = notificationService;
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
        // Añade "Concept" a la lista
        [Bind("DispatchId,Amount,PaidDate,Notes,Concept")] Payment payment, 
        IFormFile comprobanteFile)
        {
            if (comprobanteFile == null || comprobanteFile.Length == 0)
            {
                ModelState.AddModelError("comprobanteFile", "Debe adjuntar un archivo de comprobante.");
            }

            if (ModelState.IsValid)
            {
                // 1. Guardar el archivo físico (Lógica de PARTE 5)
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + comprobanteFile.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await comprobanteFile.CopyToAsync(stream);
                }

                // 2. Crear el registro del Documento
                var newDocument = new Document
                {
                    DispatchId = payment.DispatchId,
                    FileName = comprobanteFile.FileName,
                    FilePath = "/uploads/" + uniqueFileName, // Ruta relativa
                    ContentType = comprobanteFile.ContentType,
                    FileSize = comprobanteFile.Length,
                    DocType = Models.Enums.DocumentType.PaymentProof, // (CRITERIO 1)
                    UploadedAt = DateTime.UtcNow
                };
                _context.Documents.Add(newDocument);
                await _context.SaveChangesAsync(); // Guardamos para obtener el ID

                // 3. Vincular Documento al Pago y guardar Pago (CRITERIO 2)
                payment.DocumentId = newDocument.Id;
                payment.CreatedAt = DateTime.UtcNow;
                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                // 4. Notificar (Opcional)
                var dispatch = await _context.Dispatches.Include(d => d.Client)
                                        .FirstOrDefaultAsync(d => d.Id == payment.DispatchId);
                if (dispatch?.Client?.UserId != null)
                {
                    await _notificationService.NotifyUserAsync(
                        dispatch.Client.UserId,
                        "Nuevo Pago Registrado",
                        $"Se registró un pago de ${payment.Amount} para el despacho {dispatch.DispatchNumber}.",
                        dispatch.Id, "success");
                }

                TempData["Success"] = "Pago registrado y comprobante subido.";
                return RedirectToAction("Details", "Dispatches", new { id = payment.DispatchId });
            }

            TempData["Error"] = "No se pudo registrar el pago. Revise los campos.";
            return RedirectToAction("Details", "Dispatches", new { id = payment.DispatchId });
        }

        // POST: Payments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Encontrar el registro del pago
            var payment = await _context.Payments.FindAsync(id);
            if (payment == null)
            {
                TempData["Error"] = "No se encontró el pago a eliminar.";
                return RedirectToAction("Index", "Home");
            }

            int dispatchId = payment.DispatchId;
            int? documentId = payment.DocumentId;

            // 2. Eliminar el registro del pago
            _context.Payments.Remove(payment);

            // 3. Eliminar el documento asociado (comprobante)
            if (documentId.HasValue)
            {
                var document = await _context.Documents.FindAsync(documentId.Value);
                if (document != null)
                {
                    // 3a. Borrar el archivo físico
                    var physicalPath = Path.Combine(_env.WebRootPath, document.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                    // 3b. Borrar el registro del documento
                    _context.Documents.Remove(document);
                }
            }

            // 4. Guardar cambios
            await _context.SaveChangesAsync();
            TempData["Success"] = "Pago/Comprobante eliminado correctamente.";

            return RedirectToAction("Details", "Dispatches", new { id = dispatchId });
        }
    }
}