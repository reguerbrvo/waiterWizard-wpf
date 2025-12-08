using System.Windows;
using System.Windows.Controls;
using RestaurantSala.Core.Models;

namespace RestaurantSala
{
    public partial class ComandaDialog : Window
    {
        private readonly ComandaEditorViewModel _vm;
        public ComandaDialog(ComandaEditorViewModel vm)
        {
            InitializeComponent();
            _vm = vm;
            DataContext = _vm;
        }

        private void OnAgregarClick(object sender, RoutedEventArgs e)
        {
            _vm.AgregarLinea();
        }

        private void OnQuitarClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button { DataContext: LineaComanda linea }))
            {
                return;
            }
            _vm.QuitarLinea(linea);
        }

        private void OnOkClick(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
