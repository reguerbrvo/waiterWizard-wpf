using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestaurantSala.Core.Models
{
    public sealed class Mesa : INotifyPropertyChanged
    {
        private int _id;
        private string _nombre;
        private int _capacidadMaxima;
        private EstadoMesa _estado = EstadoMesa.Libre;
        private int _comensalesActuales;
        private Comanda _comandaActual;

        public int Id
        {
            get { return _id; }
            set { Set(ref _id, value); }
        }

        public string Nombre
        {
            get { return _nombre; }
            set { Set(ref _nombre, value); }
        }

        public int CapacidadMaxima
        {
            get { return _capacidadMaxima; }
            set { Set(ref _capacidadMaxima, value); }
        }

        public EstadoMesa Estado
        {
            get { return _estado; }
            set { Set(ref _estado, value); }
        }

        // Nº de comensales sentados actualmente (válido en Reservada/Ocupada*)
        public int ComensalesActuales
        {
            get { return _comensalesActuales; }
            set { Set(ref _comensalesActuales, value); }
        }

        // Comanda activa (si Estado == OcupadaConComanda). Puede ser null.
        public Comanda ComandaActual
        {
            get { return _comandaActual; }
            set { Set(ref _comandaActual, value); }
        }

        // Historial de comandas de la sesión (incluye grupos anteriores de la misma mesa)
        public List<Comanda> ComandasHistorial { get; } = new List<Comanda>();

        // INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            var h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(prop));
        }
        private bool Set<T>(ref T field, T value, [CallerMemberName] string prop = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(prop);
            return true;
        }
    }
}
