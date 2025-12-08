using System.Linq;
using global::RestaurantSala.Core.Models;

namespace RestaurantSala.Core.Data.Dto
{
    public static class DtoMapper
    {
        public static SesionDto ToDto(Sesion s)
        {
            var dto = new SesionDto
            {
                Id = s.Id,
                FechaInicio = s.FechaInicio,
                FechaFin = s.FechaFin,
                Carta = s.Carta.Select(p => new PlatoDto
                {
                    Codigo = p.Codigo,
                    Nombre = p.Nombre,
                    Categoria = p.Categoria
                }).ToList(),
                Mesas = s.Mesas.Select(m => new MesaDto
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    CapacidadMaxima = m.CapacidadMaxima,
                    Estado = m.Estado,
                    ComensalesActuales = m.ComensalesActuales,
                    ComandaActual = m.ComandaActual == null ? null : new ComandaDto
                    {
                        MesaId = m.ComandaActual.MesaId,
                        FechaHora = m.ComandaActual.FechaHora,
                        Lineas = m.ComandaActual.Lineas.Select(l => new LineaComandaDto
                        {
                            PlatoCodigo = l.Plato != null ? l.Plato.Codigo : null,
                            Cantidad = l.Cantidad
                        }).ToList()
                    },
                    ComandasHistorial = m.ComandasHistorial.Select(c => new ComandaDto
                    {
                        MesaId = c.MesaId,
                        FechaHora = c.FechaHora,
                        Lineas = c.Lineas.Select(l => new LineaComandaDto
                        {
                            PlatoCodigo = l.Plato != null ? l.Plato.Codigo : null,
                            Cantidad = l.Cantidad
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
            return dto;
        }

        public static Sesion FromDto(SesionDto dto)
        {
            var s = new Sesion
            {
                Id = dto.Id,
                FechaInicio = dto.FechaInicio,
                FechaFin = dto.FechaFin
            };

            // Carta
            foreach (var p in dto.Carta)
                s.Carta.Add(new Plato { Codigo = p.Codigo, Nombre = p.Nombre, Categoria = p.Categoria });

            // Mesas
            foreach (var m in dto.Mesas)
            {
                var mesa = new Mesa
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    CapacidadMaxima = m.CapacidadMaxima,
                    Estado = m.Estado,
                    ComensalesActuales = m.ComensalesActuales
                };

                // Historial primero (para poder reutilizar objetos Plato de la carta)
                foreach (var c in m.ComandasHistorial ?? new System.Collections.Generic.List<ComandaDto>())
                {
                    var com = new Comanda
                    {
                        MesaId = c.MesaId,
                        FechaHora = c.FechaHora,
                        Lineas = c.Lineas.Select(l => new LineaComanda
                        {
                            Plato = string.IsNullOrEmpty(l.PlatoCodigo) ? null : s.Carta.FirstOrDefault(p => p.Codigo == l.PlatoCodigo),
                            Cantidad = l.Cantidad
                        }).ToList()
                    };
                    mesa.ComandasHistorial.Add(com);
                }

                // Comanda actual
                if (m.ComandaActual != null)
                {
                    mesa.ComandaActual = new Comanda
                    {
                        MesaId = m.ComandaActual.MesaId,
                        FechaHora = m.ComandaActual.FechaHora,
                        Lineas = m.ComandaActual.Lineas.Select(l => new LineaComanda
                        {
                            Plato = string.IsNullOrEmpty(l.PlatoCodigo) ? null : s.Carta.FirstOrDefault(p => p.Codigo == l.PlatoCodigo),
                            Cantidad = l.Cantidad
                        }).ToList()
                    };
                }

                s.Mesas.Add(mesa);
            }

            return s;
        }
    }
}
