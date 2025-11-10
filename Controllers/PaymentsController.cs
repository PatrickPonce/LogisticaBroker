using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using Microsoft.AspNetCore.Authorization;

namespace LogisticaBroker.Controllers
{
    [Authorize(Roles = Roles.Admin)] // Solo admins gestionan pagos
    public class PaymentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PaymentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: Payments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DispatchId,PaymentType,Amount,Status,DueDate,PaidDate,Notes")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                // Forzar fechas a UTC si PostgreSQL reclama
                if (payment.DueDate.HasValue) payment.DueDate = DateTime.SpecifyKind(payment.DueDate.Value, DateTimeKind.Utc);
                if (payment.PaidDate.HasValue) payment.PaidDate = DateTime.SpecifyKind(payment.PaidDate.Value, DateTimeKind.Utc);
                
                payment.CreatedAt = DateTime.UtcNow;
                _context.Add(payment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pago registrado correctamente.";
            }
            else
            {
                TempData["Error"] = "Error al registrar el pago. Verifique los campos.";
            }
            return RedirectToAction("Details", "Dispatches", new { id = payment.DispatchId });
        }

        // POST: Payments/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var payment = await _context.Payments.FindAsync(id);
            if (payment != null)
            {
                _context.Payments.Remove(payment);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Pago eliminado.";
            }
            return RedirectToAction("Details", "Dispatches", new { id = payment?.DispatchId });
        }
    }
}