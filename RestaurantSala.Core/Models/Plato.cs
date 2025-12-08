using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSala.Core.Models
{
    public sealed class Plato
    {
        public string Codigo { get; set; } = string.Empty; 
        public string Nombre { get; set; } = string.Empty;
        public CategoriaPlato Categoria { get; set; }
    }
}
