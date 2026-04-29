using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;

namespace LogisticaBroker.Controllers.Api
{
    public record TimelineEventDto(
        int Id,
        int DispatchId,
        string Status,
        string? Notes,
        string? ChangedByEmail,
        DateTime ChangedAt
    );

    [ApiController]
    [Route("api/dispatches/{dispatchId:int}/timeline")]
    [Authorize]
    [Produces("application/json")]
    public class TimelineApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TimelineApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/dispatches/5/timeline
        // T46: Historial de hitos de un despacho específico.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TimelineEventDto>>> GetAll(int dispatchId)
        {
            if (!await _context.Dispatches.AnyAsync(d => d.Id == dispatchId))
                return NotFound(new { error = "Despacho no encontrado." });

            var events = await _context.DispatchTimelines
                .AsNoTracking()
                .Include(t => t.ChangedBy)
                .Where(t => t.DispatchId == dispatchId)
                .OrderBy(t => t.ChangedAt)
                .Select(t => new TimelineEventDto(
                    t.Id,
                    t.DispatchId,
                    t.Status.ToString(),
                    t.Notes,
                    t.ChangedBy != null ? t.ChangedBy.Email : null,
                    t.ChangedAt))
                .ToListAsync();

            return Ok(events);
        }
    }
}
