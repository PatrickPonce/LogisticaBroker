using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using LogisticaBroker.Models.ViewModels.Api;

namespace LogisticaBroker.Controllers.Api
{
    [ApiController]
    [Route("api/dispatches/{dispatchId:int}/payments")]
    [Authorize]
    [Produces("application/json")]
    public class PaymentsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PaymentsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/dispatches/5/payments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PaymentDto>>> GetAll(int dispatchId)
        {
            if (!await _context.Dispatches.AnyAsync(d => d.Id == dispatchId))
                return NotFound(new { error = "Despacho no encontrado." });

            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.DispatchId == dispatchId)
                .OrderByDescending(p => p.PaidDate)
                .Select(p => ToDto(p))
                .ToListAsync();

            return Ok(payments);
        }

        // GET api/dispatches/5/payments/3
        [HttpGet("{id:int}")]
        public async Task<ActionResult<PaymentDto>> GetById(int dispatchId, int id)
        {
            var payment = await _context.Payments
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id && p.DispatchId == dispatchId);

            if (payment is null) return NotFound();
            return Ok(ToDto(payment));
        }

        // POST api/dispatches/5/payments
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<PaymentDto>> Create(int dispatchId, [FromBody] PaymentCreateDto dto)
        {
            if (dto.DispatchId != dispatchId)
                return BadRequest(new { error = "El dispatchId del cuerpo no coincide con la ruta." });

            if (!await _context.Dispatches.AnyAsync(d => d.Id == dispatchId))
                return NotFound(new { error = "Despacho no encontrado." });

            var payment = new Payment
            {
                DispatchId = dispatchId,
                Amount = dto.Amount,
                PaidDate = dto.PaidDate,
                Notes = dto.Notes,
                Concept = dto.Concept
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { dispatchId, id = payment.Id }, ToDto(payment));
        }

        // DELETE api/dispatches/5/payments/3
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int dispatchId, int id)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == id && p.DispatchId == dispatchId);

            if (payment is null) return NotFound();

            _context.Payments.Remove(payment);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static PaymentDto ToDto(Payment p) => new(
            p.Id, p.DispatchId, p.Amount, p.PaidDate,
            p.Notes, p.Concept.ToString(), p.CreatedAt);
    }
}
