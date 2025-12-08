using System;
using System.Collections.Generic;

namespace RestaurantSala.Core.Models
{
    public sealed class Sesion
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime FechaInicio { get; set; } = DateTime.Now;
        public DateTime? FechaFin { get; set; }

        public List<Mesa> Mesas { get; } = new List<Mesa>();
        public List<Plato> Carta { get; } = new List<Plato>();
    }
}
