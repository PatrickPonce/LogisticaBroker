using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using LogisticaBroker.Models.Enums;
using LogisticaBroker.Models.ViewModels.Api;

namespace LogisticaBroker.Controllers.Api
{
    [ApiController]
    [Route("api/dispatches")]
    [Authorize]
    [Produces("application/json")]
    public class DispatchesApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DispatchesApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/dispatches
        // T45: Si el usuario tiene rol Cliente, solo ve sus propios despachos.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DispatchDto>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] int? clientId,
            [FromQuery] string? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageSize > 100) pageSize = 100;

            IQueryable<Dispatch> filtered = _context.Dispatches
                .AsNoTracking()
                .Include(d => d.Client);

            // T45: Clientes solo ven sus propios despachos
            if (User.IsInRole(Roles.Cliente))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                filtered = filtered.Where(d => d.Client != null && d.Client.UserId == userId);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                filtered = filtered.Where(d =>
                    d.DispatchNumber.ToLower().Contains(term) ||
                    (d.TrackingCode != null && d.TrackingCode.ToLower().Contains(term)) ||
                    (d.BLNumber != null && d.BLNumber.ToLower().Contains(term)) ||
                    (d.ContainerNumber != null && d.ContainerNumber.ToLower().Contains(term)));
            }

            if (clientId.HasValue && User.IsInRole(Roles.Admin))
                filtered = filtered.Where(d => d.ClientId == clientId.Value);

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<DispatchStatus>(status, true, out var parsedStatus))
                filtered = filtered.Where(d => d.Status == parsedStatus);

            var total = await filtered.CountAsync();
            var items = await filtered
                .OrderByDescending(d => d.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(d => ToDto(d))
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(items);
        }

        // GET api/dispatches/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<DispatchDto>> GetById(int id)
        {
            var dispatch = await _context.Dispatches
                .AsNoTracking()
                .Include(d => d.Client)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dispatch is null) return NotFound();

            // Un cliente solo puede ver sus propios despachos
            if (User.IsInRole(Roles.Cliente))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (dispatch.Client?.UserId != userId)
                    return Forbid();
            }

            return Ok(ToDto(dispatch));
        }

        // POST api/dispatches
        // T31: Registrar apertura del despacho.
        // T32: Valida unicidad de BLNumber.
        // T33: Autogenera TrackingCode (ORD-YYYY-XXXX).
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DispatchDto>> Create([FromBody] DispatchCreateDto dto)
        {
            if (!await _context.Clients.AnyAsync(c => c.Id == dto.ClientId))
                return UnprocessableEntity(new { error = "El cliente especificado no existe." });

            // T32: Unicidad de BL
            if (!string.IsNullOrWhiteSpace(dto.BLNumber) &&
                await _context.Dispatches.AnyAsync(d => d.BLNumber == dto.BLNumber))
                return Conflict(new { error = $"Ya existe un despacho con el BL Number '{dto.BLNumber}'." });

            // T33: Autogenerar cÃ³digo de seguimiento ORD-YYYY-XXXX
            var trackingCode = await GenerateTrackingCodeAsync();

            var dispatch = new Dispatch
            {
                DispatchNumber = trackingCode,  // DispatchNumber = cÃ³digo interno
                TrackingCode = trackingCode,
                BLNumber = dto.BLNumber,
                ClientId = dto.ClientId,
                Supplier = dto.Supplier,
                ShippingLine = dto.ShippingLine,
                ArrivalDate = dto.ArrivalDate,
                Channel = dto.Channel,
                ContainerNumber = dto.ContainerNumber,
                Port = dto.Port,
                Weight = dto.Weight,
                Value = dto.Value,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Dispatches.Add(dispatch);
            await _context.SaveChangesAsync();

            await _context.Entry(dispatch).Reference(d => d.Client).LoadAsync();
            return CreatedAtAction(nameof(GetById), new { id = dispatch.Id }, ToDto(dispatch));
        }

        // PATCH api/dispatches/5
        [HttpPatch("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DispatchDto>> Update(int id, [FromBody] DispatchUpdateDto dto)
        {
            var dispatch = await _context.Dispatches
                .Include(d => d.Client)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dispatch is null) return NotFound();

            // T32: Validar unicidad de BL al actualizar
            if (!string.IsNullOrWhiteSpace(dto.BLNumber) && dto.BLNumber != dispatch.BLNumber &&
                await _context.Dispatches.AnyAsync(d => d.BLNumber == dto.BLNumber && d.Id != id))
                return Conflict(new { error = $"Ya existe un despacho con el BL Number '{dto.BLNumber}'." });

            if (dto.BLNumber is not null) dispatch.BLNumber = dto.BLNumber;
            if (dto.Supplier is not null) dispatch.Supplier = dto.Supplier;
            if (dto.ShippingLine is not null) dispatch.ShippingLine = dto.ShippingLine;
            if (dto.ArrivalDate.HasValue) dispatch.ArrivalDate = dto.ArrivalDate;
            if (dto.Channel.HasValue) dispatch.Channel = dto.Channel.Value;
            if (dto.Status.HasValue) dispatch.Status = dto.Status.Value;
            if (dto.ContainerNumber is not null) dispatch.ContainerNumber = dto.ContainerNumber;
            if (dto.Port is not null) dispatch.Port = dto.Port;
            if (dto.Weight.HasValue) dispatch.Weight = dto.Weight;
            if (dto.Value.HasValue) dispatch.Value = dto.Value;

            dispatch.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ToDto(dispatch));
        }

        // PUT api/dispatches/5/tariff
        // T35, T36, T37: Asignar partida arancelaria y cambiar estado a Classified.
        [HttpPut("{id:int}/tariff")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DispatchDto>> AssignTariff(int id, [FromBody] AssignTariffDto dto)
        {
            var dispatch = await _context.Dispatches
                .Include(d => d.Client)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dispatch is null) return NotFound();

            dispatch.TariffCode = dto.TariffCode;
            dispatch.Status = DispatchStatus.Classified;
            dispatch.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(ToDto(dispatch));
        }

        // DELETE api/dispatches/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var dispatch = await _context.Dispatches.FindAsync(id);
            if (dispatch is null) return NotFound();

            _context.Dispatches.Remove(dispatch);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Helpers ---

        private async Task<string> GenerateTrackingCodeAsync()
        {
            var year = DateTime.UtcNow.Year;
            var prefix = $"ORD-{year}-";

            // Busca el Ãºltimo nÃºmero secuencial del aÃ±o actual
            var lastCode = await _context.Dispatches
                .Where(d => d.TrackingCode != null && d.TrackingCode.StartsWith(prefix))
                .OrderByDescending(d => d.TrackingCode)
                .Select(d => d.TrackingCode)
                .FirstOrDefaultAsync();

            int nextSeq = 1;
            if (lastCode is not null && int.TryParse(lastCode[prefix.Length..], out var seq))
                nextSeq = seq + 1;

            return $"{prefix}{nextSeq:D4}";
        }

        private static DispatchDto ToDto(Dispatch d) => new(
            d.Id, d.DispatchNumber, d.TrackingCode, d.BLNumber,
            d.ClientId, d.Client?.CompanyName,
            d.Supplier, d.ShippingLine, d.ArrivalDate,
            d.Channel.ToString(), d.Status.ToString(),
            d.ContainerNumber, d.Port, d.Weight, d.Value,
            d.TariffCode, d.CreatedAt, d.UpdatedAt);
    }
}
