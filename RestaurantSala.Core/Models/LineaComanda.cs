using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSala.Core.Models
{
    public sealed class LineaComanda
    {
        public Plato Plato { get; set; }
        public int Cantidad { get; set; }
    }
}
