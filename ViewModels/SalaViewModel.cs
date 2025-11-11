using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RestaurantSala.Core.Data;
using RestaurantSala.Core.Models;

namespace RestaurantSala
{
    public class SalaViewModel : ObservableObject
    {
        public Sesion Sesion { get; private set; }
        public ObservableCollection<Mesa> Mesas { get; private set; }

        public RelayCommand CmdEditarComanda { get; private set; }

        private Mesa _mesaSeleccionada;
        public Mesa MesaSeleccionada
        {
            get { return _mesaSeleccionada; }
            set { if (Set(ref _mesaSeleccionada, value)) { ActualizarComandos(); } }
        }

        private int _comensalesEntrada; // valor editable en UI para la mesa seleccionada
        public int ComensalesEntrada
        {
            get { return _comensalesEntrada; }
            set
            {
                if (Set(ref _comensalesEntrada, value))
                {
                    // Los CanExecute dependen del aforo => actualiza botones al teclear
                    ActualizarComandos();
                }
            }
        }

        // Comandos de estado
        public RelayCommand CmdReservar { get; private set; }
        public RelayCommand CmdOcuparSinComanda { get; private set; }
        public RelayCommand CmdOcuparConComanda { get; private set; }
        public RelayCommand CmdLiberar { get; private set; }
        public RelayCommand CmdNuevaSesion { get; private set; }

        public SalaViewModel()
        {
            Sesion = DemoData.CrearSesionDemo();
            Mesas = new ObservableCollection<Mesa>(Sesion.Mesas);

            CmdReservar = new RelayCommand(_ => CambiarAReservada(), _ => PuedeReservar());
            CmdOcuparSinComanda = new RelayCommand(_ => CambiarAOcupadaSinComanda(), _ => PuedeOcuparSinComanda());
            CmdOcuparConComanda = new RelayCommand(_ => CambiarAOcupadaConComanda(), _ => PuedeOcuparConComanda());
            CmdLiberar = new RelayCommand(_ => CambiarALibre(), _ => PuedeLiberar());
            CmdNuevaSesion = new RelayCommand(_ => NuevaSesion());
            CmdEditarComanda = new RelayCommand(_ => EditarComanda(), _ => MesaSeleccionada != null);
        }

        private void ActualizarComandos()
        {
            CmdReservar.RaiseCanExecuteChanged();
            CmdOcuparSinComanda.RaiseCanExecuteChanged();
            CmdOcuparConComanda.RaiseCanExecuteChanged();
            CmdLiberar.RaiseCanExecuteChanged();
            if (CmdEditarComanda != null) CmdEditarComanda.RaiseCanExecuteChanged();

        }

        private bool TieneMesaSel() { return MesaSeleccionada != null; }

        // --- Reglas de flujo ---
        private bool PuedeReservar()
        {
            return TieneMesaSel() && MesaSeleccionada.Estado == EstadoMesa.Libre && ComensalesEntradaDentroDeAforoMinimo();
        }
        private bool PuedeOcuparSinComanda()
        {
            // Desde Libre o Reservada, con comensales válidos
            if (!TieneMesaSel()) return false;
            var e = MesaSeleccionada.Estado;
            return (e == EstadoMesa.Libre || e == EstadoMesa.Reservada) && ComensalesEntradaDentroDeAforoMinimo();
        }
        private bool PuedeOcuparConComanda()
        {
            // Permitir desde Libre/Reservada si aforo válido, o desde OcupadaSinComanda directamente
            if (!TieneMesaSel()) return false;
            var e = MesaSeleccionada.Estado;
            if (e == EstadoMesa.Libre || e == EstadoMesa.Reservada)
                return ComensalesEntradaDentroDeAforoMinimo();
            return e == EstadoMesa.OcupadaSinComanda;
        }
        private bool PuedeLiberar()
        {
            // Permite liberar desde Reservada u Ocupada* (con o sin comanda)
            if (!TieneMesaSel()) return false;
            var e = MesaSeleccionada.Estado;
            return e == EstadoMesa.OcupadaConComanda || e == EstadoMesa.OcupadaSinComanda || e == EstadoMesa.Reservada;
        }
        private bool ComensalesEntradaDentroDeAforoMinimo()
        {
            if (!TieneMesaSel()) return false;
            // Regla: comensales > 0 y <= capacidad
            return ComensalesEntrada > 0 && ComensalesEntrada <= MesaSeleccionada.CapacidadMaxima;
        }

        // --- Acciones ---
        private void CambiarAReservada()
        {
            if (!PuedeReservar()) { Aviso("No se puede reservar. Revisa el aforo y el estado actual."); return; }
            MesaSeleccionada.Estado = EstadoMesa.Reservada;
            MesaSeleccionada.ComensalesActuales = ComensalesEntrada;
            OnPropertyChanged(nameof(MesaSeleccionada));
            ActualizarComandos();
        }
        private void CambiarAOcupadaSinComanda()
        {
            if (!PuedeOcuparSinComanda()) { Aviso("No se puede ocupar (sin comanda). Revisa el estado y el aforo."); return; }
            MesaSeleccionada.Estado = EstadoMesa.OcupadaSinComanda;
            MesaSeleccionada.ComensalesActuales = ComensalesEntrada;
            OnPropertyChanged(nameof(MesaSeleccionada));
            ActualizarComandos();
        }
        private void CambiarAOcupadaConComanda()
        {
            if (!TieneMesaSel()) { return; }

            // Si no hay comanda activa, creamos una vacía para permitir la transición (hasta Fase 5)
            if (MesaSeleccionada.ComandaActual == null)
            {
                MesaSeleccionada.ComandaActual = new Comanda
                {
                    MesaId = MesaSeleccionada.Id,
                    FechaHora = System.DateTime.Now,
                    Lineas = new System.Collections.Generic.List<LineaComanda>()
                };
                // Asegurar que queda registrada en el historial
                if (!MesaSeleccionada.ComandasHistorial.Contains(MesaSeleccionada.ComandaActual))
                    MesaSeleccionada.ComandasHistorial.Add(MesaSeleccionada.ComandaActual);
            }

            // Si venimos de Libre/Reservada, validar aforo y copiar comensales
            if (MesaSeleccionada.Estado == EstadoMesa.Libre || MesaSeleccionada.Estado == EstadoMesa.Reservada)
            {
                if (!ComensalesEntradaDentroDeAforoMinimo()) { Aviso("Especifica comensales válidos antes de ocupar con comanda."); return; }
                MesaSeleccionada.ComensalesActuales = ComensalesEntrada;
            }

            MesaSeleccionada.Estado = EstadoMesa.OcupadaConComanda;
            OnPropertyChanged(nameof(MesaSeleccionada));
            ActualizarComandos();
        }
        private void CambiarALibre()
        {
            if (!PuedeLiberar()) { Aviso("Solo se puede liberar desde 'Reservada' u 'Ocupada'."); return; }
            // Al liberar: mantener historial, limpiar comanda actual, comensales a 0
            MesaSeleccionada.ComandaActual = null; // historial no se borra
            MesaSeleccionada.ComensalesActuales = 0;
            MesaSeleccionada.Estado = EstadoMesa.Libre;
            OnPropertyChanged(nameof(MesaSeleccionada));
            ActualizarComandos();
        }

        private void NuevaSesion()
        {
            // Reinicializa la sesión de demo y vuelve a enlazar la colección de mesas
            Sesion = DemoData.CrearSesionDemo();
            Mesas = new ObservableCollection<Mesa>(Sesion.Mesas);
            OnPropertyChanged(nameof(Sesion));
            OnPropertyChanged(nameof(Mesas));
            MesaSeleccionada = null;
            ActualizarComandos();
        }

        private void Aviso(string msg)
        {
            MessageBox.Show(msg, "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void EditarComanda()
        {
            if (MesaSeleccionada == null) return;

            // Asegurar ComandaActual (si no existe, crearla y agregar al historial)
            if (MesaSeleccionada.ComandaActual == null)
            {
                MesaSeleccionada.ComandaActual = new Comanda
                {
                    MesaId = MesaSeleccionada.Id,
                    FechaHora = System.DateTime.Now,
                    Lineas = new System.Collections.Generic.List<LineaComanda>()
                };
                if (!MesaSeleccionada.ComandasHistorial.Contains(MesaSeleccionada.ComandaActual))
                    MesaSeleccionada.ComandasHistorial.Add(MesaSeleccionada.ComandaActual);
            }

            var vm = new ComandaEditorViewModel(Sesion.Carta, MesaSeleccionada.ComandaActual.Lineas);
            var dlg = new ComandaDialog(vm) { Owner = Application.Current.MainWindow };
            var ok = dlg.ShowDialog();
            if (ok == true)
            {
                // Reemplazar las líneas por el resultado del editor
                MesaSeleccionada.ComandaActual.Lineas = vm.ConstruirResultado();

                // Si la mesa estaba Libre/Reservada/OcupadaSinComanda, pasar a OcupadaConComanda
                if (MesaSeleccionada.Estado != EstadoMesa.OcupadaConComanda)
                {
                    MesaSeleccionada.Estado = EstadoMesa.OcupadaConComanda;
                    if (MesaSeleccionada.ComensalesActuales == 0 && ComensalesEntradaDentroDeAforoMinimo())
                        MesaSeleccionada.ComensalesActuales = ComensalesEntrada;
                }

                // Notificar para refrescar panel y ventana secundaria
                OnPropertyChanged(nameof(MesaSeleccionada));
                ActualizarComandos();
            }
        }
    }
}