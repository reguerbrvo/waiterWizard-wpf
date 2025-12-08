using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using global::RestaurantSala.Core.Models;
using RestaurantSala.Core.Models;

namespace RestaurantSala.Core.Data.Dto
{
    [DataContract]
    public class SesionDto
    {
        [DataMember] public Guid Id { get; set; }
        [DataMember] public DateTime FechaInicio { get; set; }
        [DataMember] public DateTime? FechaFin { get; set; }
        [DataMember] public List<MesaDto> Mesas { get; set; }
        [DataMember] public List<PlatoDto> Carta { get; set; }
    }

    [DataContract]
    public class MesaDto
    {
        [DataMember] public int Id { get; set; }
        [DataMember] public string Nombre { get; set; }
        [DataMember] public int CapacidadMaxima { get; set; }
        [DataMember] public EstadoMesa Estado { get; set; }
        [DataMember] public int ComensalesActuales { get; set; }
        [DataMember] public ComandaDto ComandaActual { get; set; }
        [DataMember] public List<ComandaDto> ComandasHistorial { get; set; }
    }

    [DataContract]
    public class ComandaDto
    {
        [DataMember] public int MesaId { get; set; }
        [DataMember] public DateTime FechaHora { get; set; }
        [DataMember] public List<LineaComandaDto> Lineas { get; set; }
    }

    [DataContract]
    public class LineaComandaDto
    {
        [DataMember] public string PlatoCodigo { get; set; }
        [DataMember] public int Cantidad { get; set; }
    }

    [DataContract]
    public class PlatoDto
    {
        [DataMember] public string Codigo { get; set; }
        [DataMember] public string Nombre { get; set; }
        [DataMember] public CategoriaPlato Categoria { get; set; }
    }
}
