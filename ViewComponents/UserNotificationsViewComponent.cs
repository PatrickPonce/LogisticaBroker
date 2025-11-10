using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LogisticaBroker.Data;
using Microsoft.AspNetCore.Identity;
using LogisticaBroker.Models;
using System.Security.Claims;

namespace LogisticaBroker.ViewComponents
{
    public class UserNotificationsViewComponent : ViewComponent
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserNotificationsViewComponent(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = _userManager.GetUserId(HttpContext.User as ClaimsPrincipal);
            
            var notifications = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(5) // Solo mostramos las 5 últimas no leídas en el menú
                .ToListAsync();

            return View(notifications);
        }
    }
}