using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace RestaurantSala
{
    public partial class VentanaSecundaria : Window
    {
        private readonly SalaViewModel _vm;

        public VentanaSecundaria(SalaViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            this.DataContext = _vm; // MISMA instancia que la principal

            // Preseleccionar la fila de la mesa actualmente seleccionada
            this.Loaded += (s, e) => SincronizarSeleccionTabla();

            // Escuchar cambios para que al cambiar MesaSeleccionada desde Canvas se mueva la selección en la tabla
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(SalaViewModel.MesaSeleccionada))
                    SincronizarSeleccionTabla();
            };
        }

        private void SincronizarSeleccionTabla()
        {
            if (_vm.MesaSeleccionada == null) { dgMesas.UnselectAll(); return; }
            var target = _vm.MesaSeleccionada;
            dgMesas.SelectedItem = target;
            dgMesas.ScrollIntoView(target);
        }

        private void dgMesas_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var mesa = dgMesas.SelectedItem as Core.Models.Mesa;
            if (mesa != null)
            {
                // Cambiar la selección en la VM ==> principal resaltará por VM.PropertyChanged
                _vm.MesaSeleccionada = mesa;
            }
        }
    }
}