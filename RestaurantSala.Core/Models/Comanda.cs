using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantSala.Core.Models;

namespace RestaurantSala.Core.Models
{
    public sealed class Comanda
    {
        public int MesaId { get; set; }
        public DateTime FechaHora { get; set; } = DateTime.Now;
        public List<LineaComanda> Lineas { get; set; } = new List<LineaComanda>();

        public int TotalPlatos()
        {
            return Lineas.Sum(l => l.Cantidad);
        }

        public int TotalPorCategoria(CategoriaPlato cat)
        {
            return Lineas.Where(l => l.Plato != null && l.Plato.Categoria == cat).Sum(l => l.Cantidad);
        }
    }
}