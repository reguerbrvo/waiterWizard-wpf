using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSala.Core.Models
{
    public enum CategoriaPlato { Primero, Segundo, Postre }

    public enum EstadoMesa
    {
        Libre,
        Reservada,
        OcupadaSinComanda,
        OcupadaConComanda
    }
}
