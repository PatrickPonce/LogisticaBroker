using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using LogisticaBroker.Models.Enums;
using LogisticaBroker.Models.ViewModels.Api;

namespace LogisticaBroker.Controllers.Api
{
    [ApiController]
    [Route("api/dispatches/{dispatchId:int}/dam")]
    [Authorize]
    [Produces("application/json")]
    public class DamApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DamApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET api/dispatches/5/dam
        // T39: Precarga datos del despacho/importador para el formulario.
        [HttpGet]
        public async Task<ActionResult<DamDto>> Get(int dispatchId)
        {
            var dam = await _context.Dams
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.DispatchId == dispatchId);

            if (dam is null) return NotFound(new { error = "No existe DAM para este despacho." });
            return Ok(ToDto(dam));
        }

        // POST api/dispatches/5/dam
        // T42: Registrar borrador DAM.
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DamDto>> Create(int dispatchId, [FromBody] DamCreateDto dto)
        {
            if (dto.DispatchId != dispatchId)
                return BadRequest(new { error = "El dispatchId del cuerpo no coincide con la ruta." });

            if (!await _context.Dispatches.AnyAsync(d => d.Id == dispatchId))
                return NotFound(new { error = "Despacho no encontrado." });

            if (await _context.Dams.AnyAsync(d => d.DispatchId == dispatchId))
                return Conflict(new { error = "Ya existe una DAM para este despacho. Use PATCH para actualizar." });

            var dam = new Dam
            {
                DispatchId = dispatchId,
                FobValue = dto.FobValue,
                FreightValue = dto.FreightValue,
                InsuranceValue = dto.InsuranceValue,
                CustomsRegime = dto.CustomsRegime,
                EntryCustomsOffice = dto.EntryCustomsOffice,
                Notes = dto.Notes,
                Status = DamStatus.Draft,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Dams.Add(dam);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { dispatchId }, ToDto(dam));
        }

        // PATCH api/dispatches/5/dam
        // T41: Actualizar borrador (autoguardado asíncrono).
        [HttpPatch]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DamDto>> Update(int dispatchId, [FromBody] DamUpdateDto dto)
        {
            var dam = await _context.Dams.FirstOrDefaultAsync(d => d.DispatchId == dispatchId);
            if (dam is null) return NotFound(new { error = "No existe DAM para este despacho." });

            if (dam.Status == DamStatus.Final)
                return Conflict(new { error = "La DAM ya está finalizada y no puede modificarse." });

            if (dto.FobValue.HasValue) dam.FobValue = dto.FobValue.Value;
            if (dto.FreightValue.HasValue) dam.FreightValue = dto.FreightValue.Value;
            if (dto.InsuranceValue.HasValue) dam.InsuranceValue = dto.InsuranceValue.Value;
            if (dto.CustomsRegime is not null) dam.CustomsRegime = dto.CustomsRegime;
            if (dto.EntryCustomsOffice is not null) dam.EntryCustomsOffice = dto.EntryCustomsOffice;
            if (dto.Notes is not null) dam.Notes = dto.Notes;

            dam.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ToDto(dam));
        }

        // POST api/dispatches/5/dam/finalize
        // Cambiar estado del borrador a Final.
        [HttpPost("finalize")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<DamDto>> Finalize(int dispatchId)
        {
            var dam = await _context.Dams.FirstOrDefaultAsync(d => d.DispatchId == dispatchId);
            if (dam is null) return NotFound(new { error = "No existe DAM para este despacho." });

            if (dam.Status == DamStatus.Final)
                return Ok(ToDto(dam)); // idempotente

            dam.Status = DamStatus.Final;
            dam.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(ToDto(dam));
        }

        private static DamDto ToDto(Dam d) => new(
            d.Id, d.DispatchId,
            d.FobValue, d.FreightValue, d.InsuranceValue, d.CifValue,
            d.CustomsRegime, d.EntryCustomsOffice, d.Notes,
            d.Status.ToString(), d.CreatedAt, d.UpdatedAt);
    }
}
