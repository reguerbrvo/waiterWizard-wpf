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
            this.DataContext = _vm;
        }

        private void BtnAgregar_Click(object sender, RoutedEventArgs e)
        {
            _vm.AgregarLinea();
        }

        private void BtnQuitar_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button; if (btn == null) return;
            var linea = btn.DataContext as LineaComanda;
            _vm.QuitarLinea(linea);
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}