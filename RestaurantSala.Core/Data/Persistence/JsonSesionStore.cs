using System.IO;
using System.Runtime.Serialization.Json;
using RestaurantSala.Core.Data.Dto;
using RestaurantSala.Core.Models;

namespace RestaurantSala.Core.Data.Persistence
{
    public static class JsonSesionStore
    {
        public static void Guardar(Sesion sesion, string ruta)
        {
            var dto = DtoMapper.ToDto(sesion);
            var ser = new DataContractJsonSerializer(typeof(SesionDto));
            using (var fs = new FileStream(ruta, FileMode.Create, FileAccess.Write))
            {
                ser.WriteObject(fs, dto);
            }
        }

        public static Sesion Cargar(string ruta)
        {
            var ser = new DataContractJsonSerializer(typeof(SesionDto));
            using (var fs = new FileStream(ruta, FileMode.Open, FileAccess.Read))
            {
                var dto = (SesionDto)ser.ReadObject(fs);
                return DtoMapper.FromDto(dto);
            }
        }
    }
}