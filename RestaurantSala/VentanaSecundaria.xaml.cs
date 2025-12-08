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
            DataContext = _vm;

            Loaded += OnLoaded;
            _vm.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SincronizarSeleccionTabla();
        }

        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SalaViewModel.MesaSeleccionada))
            {
                SincronizarSeleccionTabla();
            }
        }

        private void SincronizarSeleccionTabla()
        {
            if (_vm.MesaSeleccionada == null) { dgMesas.UnselectAll(); return; }
            var target = _vm.MesaSeleccionada;
            dgMesas.SelectedItem = target;
            dgMesas.ScrollIntoView(target);
        }

        private void OnMesasSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dgMesas.SelectedItem is Core.Models.Mesa mesa)
            {
                _vm.MesaSeleccionada = mesa;
            }
        }
    }
}