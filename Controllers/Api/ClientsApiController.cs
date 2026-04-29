using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using LogisticaBroker.Models.ViewModels.Api;
using LogisticaBroker.Services;

namespace LogisticaBroker.Controllers.Api
{
    [ApiController]
    [Route("api/clients")]
    [Authorize]
    [Produces("application/json")]
    public class ClientsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _env;

        public ClientsApiController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _emailSender = emailSender;
            _env = env;
        }

        // GET api/clients
        [HttpGet]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<IEnumerable<ClientDto>>> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            if (pageSize > 100) pageSize = 100;

            var query = _context.Clients.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.ToLower();
                query = query.Where(c =>
                    c.CompanyName.ToLower().Contains(term) ||
                    c.RUC.Contains(term) ||
                    c.Email.ToLower().Contains(term));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => ToDto(c))
                .ToListAsync();

            Response.Headers["X-Total-Count"] = total.ToString();
            Response.Headers["X-Page"] = page.ToString();
            Response.Headers["X-Page-Size"] = pageSize.ToString();

            return Ok(items);
        }

        // GET api/clients/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClientDto>> GetById(int id)
        {
            var client = await _context.Clients.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (client is null) return NotFound();
            return Ok(ToDto(client));
        }

        // POST api/clients
        // T15, T17, T18, T19: Registrar empresa, crear usuario Identity, enviar credenciales.
        [HttpPost]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ClientDto>> Create([FromBody] ClientCreateDto dto)
        {
            // T16: Validar RUC único
            if (await _context.Clients.AnyAsync(c => c.RUC == dto.RUC))
                return Conflict(new { error = "Ya existe un cliente con ese RUC." });

            if (await _userManager.FindByEmailAsync(dto.Email) is not null)
                return Conflict(new { error = "Ya existe un usuario con ese correo electrónico." });

            // T17: Generar contraseña temporal segura
            var tempPassword = PasswordGenerator.Generate();

            // Crear usuario Identity
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var createResult = await _userManager.CreateAsync(user, tempPassword);
            if (!createResult.Succeeded)
            {
                var errors = createResult.Errors.Select(e => e.Description);
                return UnprocessableEntity(new { error = "No se pudo crear el usuario.", details = errors });
            }

            await _userManager.AddToRoleAsync(user, Roles.Client);

            // T18: Insertar cliente vinculado al usuario
            var client = new Client
            {
                CompanyName = dto.CompanyName,
                RUC = dto.RUC,
                Email = dto.Email,
                Phone = dto.Phone,
                Address = dto.Address,
                ContactPerson = dto.ContactPerson,
                Notes = dto.Notes,
                UserId = user.Id
            };

            _context.Clients.Add(client);
            await _context.SaveChangesAsync();

            // T19: Enviar correo de bienvenida con credenciales
            try
            {
                var loginUrl = $"{Request.Scheme}://{Request.Host}/Identity/Account/Login";
                var html = await BuildWelcomeEmailAsync(dto.CompanyName, dto.Email, tempPassword, loginUrl);
                await _emailSender.SendEmailAsync(dto.Email, "Bienvenido a Logística Broker S.A.C.", html);
            }
            catch (Exception ex)
            {
                // El cliente fue creado; solo loguear el fallo de correo sin abortar
                var logger = HttpContext.RequestServices.GetRequiredService<ILogger<ClientsApiController>>();
                logger.LogWarning(ex, "No se pudo enviar el correo de bienvenida a {Email}", dto.Email);
            }

            return CreatedAtAction(nameof(GetById), new { id = client.Id }, ToDto(client));
        }

        // PUT api/clients/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<ActionResult<ClientDto>> Update(int id, [FromBody] ClientUpdateDto dto)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client is null) return NotFound();

            client.CompanyName = dto.CompanyName;
            client.Email = dto.Email;
            client.Phone = dto.Phone;
            client.Address = dto.Address;
            client.ContactPerson = dto.ContactPerson;
            client.Notes = dto.Notes;

            await _context.SaveChangesAsync();
            return Ok(ToDto(client));
        }

        // DELETE api/clients/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = Roles.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client is null) return NotFound();

            _context.Clients.Remove(client);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // --- Helpers ---

        private async Task<string> BuildWelcomeEmailAsync(
            string companyName, string email, string tempPassword, string loginUrl)
        {
            var templatePath = Path.Combine(_env.ContentRootPath, "Services", "Templates", "WelcomeEmail.html");
            var html = await System.IO.File.ReadAllTextAsync(templatePath);

            return html
                .Replace("{{CompanyName}}", System.Net.WebUtility.HtmlEncode(companyName))
                .Replace("{{Email}}", System.Net.WebUtility.HtmlEncode(email))
                .Replace("{{TempPassword}}", System.Net.WebUtility.HtmlEncode(tempPassword))
                .Replace("{{LoginUrl}}", loginUrl)
                .Replace("{{Year}}", DateTime.UtcNow.Year.ToString());
        }

        private static ClientDto ToDto(Client c) => new(
            c.Id, c.CompanyName, c.RUC, c.Email,
            c.Phone, c.Address, c.ContactPerson, c.Notes, c.CreatedAt);
    }
}
