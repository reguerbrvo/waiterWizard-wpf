using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using RestaurantSala.Core.Models;

namespace RestaurantSala
{
    public class ComandaEditorViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Plato> Carta { get; private set; }
        public ObservableCollection<LineaComanda> Lineas { get; private set; }

        private Plato _platoSeleccionado;
        public Plato PlatoSeleccionado
        {
            get { return _platoSeleccionado; }
            set { _platoSeleccionado = value; OnPropertyChanged(); }
        }

        private int _cantidad = 1;
        public int Cantidad
        {
            get { return _cantidad; }
            set { _cantidad = value < 1 ? 1 : value; OnPropertyChanged(); }
        }

        public ComandaEditorViewModel(IEnumerable<Plato> carta, IEnumerable<LineaComanda> lineasIniciales)
        {
            Carta = new ObservableCollection<Plato>(carta);
            Lineas = new ObservableCollection<LineaComanda>(lineasIniciales ?? new List<LineaComanda>());
            PlatoSeleccionado = Carta.FirstOrDefault();
        }

        public void AgregarLinea()
        {
            if (PlatoSeleccionado == null) return;
            // Si ya existe ese plato, sumamos cantidades
            var existente = Lineas.FirstOrDefault(l => l.Plato.Codigo == PlatoSeleccionado.Codigo);
            if (existente != null)
            {
                existente.Cantidad += Cantidad;
                // Notificar cambio manual (LineaComanda no implementa INotifyPropertyChanged)
                var idx = Lineas.IndexOf(existente);
                Lineas.RemoveAt(idx);
                Lineas.Insert(idx, existente);
            }
            else
            {
                Lineas.Add(new LineaComanda { Plato = PlatoSeleccionado, Cantidad = Cantidad });
            }
        }

        public void QuitarLinea(LineaComanda linea)
        {
            if (linea == null) return;
            Lineas.Remove(linea);
        }

        // Devuelve una copia List<> para asignar a ComandaActual.Lineas
        public List<LineaComanda> ConstruirResultado()
        {
            return Lineas.Select(l => new LineaComanda { Plato = l.Plato, Cantidad = l.Cantidad }).ToList();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string prop = null)
        {
            var h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(prop));
        }
    }
}