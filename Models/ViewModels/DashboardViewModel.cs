using LogisticaBroker.Models;

namespace LogisticaBroker.Models.ViewModels
{
    public class DashboardViewModel
    {
        // Contadores Principales
        public int TotalClients { get; set; }
        public int ActiveDispatches { get; set; }
        public int DispatchesInCustoms { get; set; }
        public int CompletedDispatches { get; set; }

        // Listas para tablas resumen
        public List<Dispatch> RecentDispatches { get; set; } = new List<Dispatch>();
        
        // Opcional: Para gráficos futuros
        // public Dictionary<string, int> DispatchesByStatus { get; set; }
    }
}