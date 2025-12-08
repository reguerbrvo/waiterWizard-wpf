using System.Linq;
using RestaurantSala.Core.Models;

namespace RestaurantSala.Core.Utils
{
    public static class Estadisticas
    {
        public static int TotalPlatosServidos(Mesa mesa)
        {
            return mesa.ComandasHistorial.Sum(c => c.TotalPlatos());
        }

        public static (int primeros, int segundos, int postres) TotalesPorCategoria(Mesa mesa)
        {
            int p = mesa.ComandasHistorial.Sum(c => c.TotalPorCategoria(CategoriaPlato.Primero));
            int s = mesa.ComandasHistorial.Sum(c => c.TotalPorCategoria(CategoriaPlato.Segundo));
            int d = mesa.ComandasHistorial.Sum(c => c.TotalPorCategoria(CategoriaPlato.Postre));
            return (p, s, d);
        }
        public static (int primeros, int segundos, int postres) TotalesPorCategoriaActual(Mesa mesa)
        {
            if (mesa == null || mesa.ComandaActual == null) return (0, 0, 0);
            var c = mesa.ComandaActual;
            int p = c.TotalPorCategoria(CategoriaPlato.Primero);
            int s = c.TotalPorCategoria(CategoriaPlato.Segundo);
            int d = c.TotalPorCategoria(CategoriaPlato.Postre);
            return (p, s, d);
        }

    }
}