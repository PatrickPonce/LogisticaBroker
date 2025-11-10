using LogisticaBroker.Data;
using LogisticaBroker.Models;

namespace LogisticaBroker.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task NotifyUserAsync(string userId, string title, string message, int? dispatchId = null, string type = "info")
        {
            var notif = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                RelatedDispatchId = dispatchId,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync();
        }
    }
}