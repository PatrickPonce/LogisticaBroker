using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using LogisticaBroker.Models.ViewModels.Api;

namespace LogisticaBroker.Controllers.Api
{
    [ApiController]
    [Route("api/dispatches/{dispatchId:int}/costs")]
    [Authorize]
    [Produces("application/json")]
    public class DispatchCostsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DispatchCostsApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/dispatches/5/costs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DispatchCostDto>>> GetAll(int dispatchId)
        {
            if (!await _context.Dispatches.AnyAsync(d => d.Id == dispatchId))
                return NotFound(new { error = "Despacho no encontrado." });

            var costs = await _context.DispatchCosts
                .AsNoTracking()
                .Where(c => c.DispatchId == dispatchId)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => ToDto(c))
                .ToListAsync();

            return Ok(costs);
        }

        // GET api/dispatches/5/costs/2
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DispatchCostDto>> GetById(int dispatchId, int id)
        {
            var cost = await _context.DispatchCosts
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.DispatchId == dispatchId);

            if (cost is null) return NotFound();
            return Ok(ToDto(cost));
        }

        // POST api/dispatches/5/costs
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DispatchCostDto>> Create(int dispatchId, [FromBody] DispatchCostCreateDto dto)
        {
            if (dto.DispatchId != dispatchId)
                return BadRequest(new { error = "El dispatchId del cuerpo no coincide con la ruta." });

            if (!await _context.Dispatches.AnyAsync(d => d.Id == dispatchId))
                return NotFound(new { error = "Despacho no encontrado." });

            var cost = new DispatchCost
            {
                DispatchId = dispatchId,
                Concept = dto.Concept,
                Amount = dto.Amount,
                Notes = dto.Notes,
                DueDate = dto.DueDate
            };

            _context.DispatchCosts.Add(cost);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { dispatchId, id = cost.Id }, ToDto(cost));
        }

        // DELETE api/dispatches/5/costs/2
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int dispatchId, int id)
        {
            var cost = await _context.DispatchCosts
                .FirstOrDefaultAsync(c => c.Id == id && c.DispatchId == dispatchId);

            if (cost is null) return NotFound();

            _context.DispatchCosts.Remove(cost);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private static DispatchCostDto ToDto(DispatchCost c) => new(
            c.Id, c.DispatchId, c.Concept.ToString(),
            c.Amount, c.Notes, c.DueDate, c.CreatedAt);
    }
}
