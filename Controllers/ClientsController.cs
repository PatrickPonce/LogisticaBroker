using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using LogisticaBroker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using X.PagedList;
using X.PagedList.EF;

namespace LogisticaBroker.Controllers
{
    [Authorize(Roles = Roles.Admin)] // Protege todas las acciones para que solo usuarios logueados accedan
    public class ClientsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ClientsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Clients
        public async Task<IActionResult> Index(string? searchTerm, int? page)
        {
            var query = _context.Clients.AsQueryable();

            // Lógica de Filtro
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(c => 
                    c.CompanyName.ToLower().Contains(searchTerm) ||
                    c.RUC.Contains(searchTerm) ||
                    c.Email.ToLower().Contains(searchTerm)
                );
            }

            // Ordenar por los más recientes primero por defecto
            query = query.OrderByDescending(c => c.CreatedAt);

            // Guardar la búsqueda actual para mantenerla en la vista
            ViewBag.CurrentSearch = searchTerm;

            // Paginación
            int pageSize = 10;
            int pageNumber = (page ?? 1);

            return View(await query.ToPagedListAsync(pageNumber, pageSize));
        }

        // GET: Clients/Details/5 (Ver detalles de uno)
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return NotFound();

            return View(client);
        }

        // GET: Clients/Create (Mostrar formulario vacío)
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clients/Create (Recibir datos del formulario y guardar)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyName,RUC,Email,Phone,Address,ContactPerson,Notes")] Client client)
        {
            // --- INICIO DE VALIDACIÓN PERSONALIZADA ---
            // Verificamos si el RUC ya existe ANTES de validar el modelo
            if (!string.IsNullOrEmpty(client.RUC))
            {
                var rucExists = await _context.Clients.AnyAsync(c => c.RUC == client.RUC);
                if (rucExists)
                {
                    // (Criterio de Aceptación)
                    ModelState.AddModelError("RUC", "Este RUC ya se encuentra registrado en el sistema.");
                }
            }

            // Verificamos si el Email ya existe
            if (!string.IsNullOrEmpty(client.Email))
            {
                var emailExists = await _context.Clients.AnyAsync(c => c.Email == client.Email);
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Este correo electrónico ya está en uso por otro cliente.");
                }
            }
            // --- FIN DE VALIDACIÓN PERSONALIZADA ---

            if (ModelState.IsValid)
            {
                _context.Add(client);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            
            // Si llegamos aquí, algo falló (ya sea la validación [Required] o la nuestra)
            // Devolvemos la vista con los mensajes de error.
            return View(client);
        }

        // GET: Clients/Edit/5 (Mostrar formulario con datos existentes)
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Edit/5 (Guardar cambios)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyName,RUC,Email,Phone,Address,ContactPerson,Notes,CreatedAt")] Client client)
        {
            if (id != client.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(client);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClientExists(client.Id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(client);
        }

        // GET: Clients/Delete/5 (Pantalla de confirmación de borrado)
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var client = await _context.Clients.FirstOrDefaultAsync(m => m.Id == id);
            if (client == null) return NotFound();

            return View(client);
        }

        // POST: Clients/Delete/5 (Ejecutar borrado real)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client != null)
            {
                _context.Clients.Remove(client);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: Clients/CreatePortalUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePortalUser(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            if (!string.IsNullOrEmpty(client.UserId))
            {
                TempData["Error"] = "Este cliente ya tiene un usuario de portal asignado.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // 1. Crear el usuario de Identity
            var user = new ApplicationUser
            {
                UserName = client.Email,
                Email = client.Email,
                FullName = client.ContactPerson ?? client.CompanyName,
                EmailConfirmed = true // Auto-confirmamos para facilitar el login inmediato
            };

            // ¡IMPORTANTE! En producción, envía un email con link de reset password en lugar de usar una fija.
            var tempPassword = "Portal" + DateTime.Now.Year + "!";
            var result = await _userManager.CreateAsync(user, tempPassword);

            if (result.Succeeded)
            {
                // 2. Asignar rol de Cliente
                await _userManager.AddToRoleAsync(user, Roles.Client);

                // 3. Vincular el Cliente con el nuevo Usuario
                client.UserId = user.Id;
                _context.Update(client);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Usuario creado. Email: {user.Email}, Contraseña temporal: {tempPassword}";
            }
            else
            {
                TempData["Error"] = "Error al crear usuario: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Details), new { id });
        }
        
        // POST: Clients/ResetPortalAccess/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPortalAccess(int id)
        {
            var client = await _context.Clients.FindAsync(id);
            if (client == null) return NotFound();

            if (string.IsNullOrEmpty(client.UserId))
            {
                TempData["Error"] = "Este cliente no tiene usuario asignado. Use 'Generar Acceso' primero.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // 1. Buscar el usuario asociado
            var user = await _userManager.FindByIdAsync(client.UserId);
            if (user == null)
            {
                TempData["Error"] = "El usuario asociado no se encontró en el sistema.";
                return RedirectToAction(nameof(Details), new { id });
            }

            // 2. Generar nueva contraseña temporal
            var newTempPassword = "Reset" + DateTime.Now.Ticks.ToString().Substring(10) + "!";

            // 3. Forzar el cambio de contraseña usando un token de reset
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newTempPassword);

            if (result.Succeeded)
            {
                TempData["Success"] = $"Acceso regenerado. Nuevas credenciales:\nEmail: {user.Email}\nContraseña: {newTempPassword}";
            }
            else
            {
                TempData["Error"] = "Error al resetear contraseña: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool ClientExists(int id)
        {
            return _context.Clients.Any(e => e.Id == id);
        }
    }
}