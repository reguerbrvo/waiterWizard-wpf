using System;
using System.Collections.Generic;
using System.Linq;
using RestaurantSala.Core.Models;

namespace RestaurantSala.Core.Data
{
    public static class DemoData
    {
        public static Sesion CrearSesionDemo()
        {
            var sesion = new Sesion
            {
                FechaInicio = DateTime.Today.AddHours(13)
            };

            var carta = new List<Plato>
            {
                new Plato { Codigo = "P01", Nombre = "Ensalada mixta", Categoria = CategoriaPlato.Primero },
                new Plato { Codigo = "P02", Nombre = "Sopa de verduras", Categoria = CategoriaPlato.Primero },
                new Plato { Codigo = "P03", Nombre = "Gazpacho", Categoria = CategoriaPlato.Primero },

                new Plato { Codigo = "S01", Nombre = "Pollo asado", Categoria = CategoriaPlato.Segundo },
                new Plato { Codigo = "S02", Nombre = "Merluza a la plancha", Categoria = CategoriaPlato.Segundo },
                new Plato { Codigo = "S03", Nombre = "Lasaña", Categoria = CategoriaPlato.Segundo },

                new Plato { Codigo = "D01", Nombre = "Flan", Categoria = CategoriaPlato.Postre },
                new Plato { Codigo = "D02", Nombre = "Fruta de temporada", Categoria = CategoriaPlato.Postre },
                new Plato { Codigo = "D03", Nombre = "Yogur", Categoria = CategoriaPlato.Postre },
            };
            sesion.Carta.AddRange(carta);

            sesion.Mesas.AddRange(new[]
            {
                new Mesa { Id = 1, Nombre = "Mesa 1", CapacidadMaxima = 2, Estado = EstadoMesa.OcupadaConComanda, ComensalesActuales = 2 },
                new Mesa { Id = 2, Nombre = "Mesa 2", CapacidadMaxima = 4, Estado = EstadoMesa.OcupadaSinComanda, ComensalesActuales = 3 },
                new Mesa { Id = 3, Nombre = "Mesa 3", CapacidadMaxima = 6, Estado = EstadoMesa.Reservada, ComensalesActuales = 4 },
                new Mesa { Id = 4, Nombre = "Mesa 4", CapacidadMaxima = 4, Estado = EstadoMesa.Libre },
                new Mesa { Id = 5, Nombre = "Mesa 5", CapacidadMaxima = 8, Estado = EstadoMesa.OcupadaConComanda, ComensalesActuales = 5 },
                new Mesa { Id = 6, Nombre = "Mesa 6", CapacidadMaxima = 2, Estado = EstadoMesa.Libre }
            });

            var m1 = sesion.Mesas.First(m => m.Id == 1);
            var m2 = sesion.Mesas.First(m => m.Id == 2);
            var m3 = sesion.Mesas.First(m => m.Id == 3);
            var m5 = sesion.Mesas.First(m => m.Id == 5);

            m1.ComandaActual = new Comanda
            {
                MesaId = 1,
                FechaHora = DateTime.Today.AddHours(13).AddMinutes(15),
                Lineas = new List<LineaComanda>
                {
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "P01"), Cantidad = 2 },
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "S02"), Cantidad = 2 },
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "D02"), Cantidad = 2 }
                }
            };
            m1.ComandasHistorial.Add(m1.ComandaActual);

            var comandaAnteriorM5 = new Comanda
            {
                MesaId = 5,
                FechaHora = DateTime.Today.AddHours(12),
                Lineas = new List<LineaComanda>
                {
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "P03"), Cantidad = 2 },
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "S01"), Cantidad = 2 },
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "D01"), Cantidad = 2 },
                }
            };
            m5.ComandasHistorial.Add(comandaAnteriorM5);

            m5.ComandaActual = new Comanda
            {
                MesaId = 5,
                FechaHora = DateTime.Today.AddHours(14),
                Lineas = new List<LineaComanda>
                {
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "P02"), Cantidad = 3 },
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "S03"), Cantidad = 3 },
                    new LineaComanda { Plato = carta.Single(p => p.Codigo == "D03"), Cantidad = 2 },
                }
            };
            m5.ComandasHistorial.Add(m5.ComandaActual);

            return sesion;
        }
    }
}