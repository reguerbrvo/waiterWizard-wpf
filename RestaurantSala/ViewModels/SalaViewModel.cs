using System;
using System.Collections.ObjectModel;
using System.Linq;
using RestaurantSala.Core.Data;
using RestaurantSala.Core.Models;

namespace RestaurantSala
{
    public class SalaViewModel : ObservableObject
    {
        private bool _comandosInicializados;
        public Func<bool> ConfirmarReinicioSesion { get; set; } = () => true;
        public Action<string> MostrarAviso { get; set; } = _ => { };
        private Sesion _sesion;
        public Sesion Sesion
        {
            get { return _sesion; }
            set
            {
                if (ReferenceEquals(_sesion, value)) return;
                _sesion = value;

                if (Mesas != null)
                {
                    Mesas.Clear();
                    foreach (var m in _sesion.Mesas) Mesas.Add(m);
                    OnPropertyChanged(nameof(Mesas));
                }
                else
                {
                    Mesas = new ObservableCollection<Mesa>(_sesion.Mesas);
                    OnPropertyChanged(nameof(Mesas));
                }

                OnPropertyChanged(nameof(Sesion));
                MesaSeleccionada = null;
                ActualizarComandos();
            }
        }
        public ObservableCollection<Mesa> Mesas { get; private set; }

        public RelayCommand CmdEditarComanda { get; private set; }

        private Mesa _mesaSeleccionada;
        public Mesa MesaSeleccionada
        {
            get { return _mesaSeleccionada; }
            set { if (Set(ref _mesaSeleccionada, value)) { ActualizarComandos(); } }
        }

        private int _comensalesEntrada;
        public int ComensalesEntrada
        {
            get { return _comensalesEntrada; }
            set
            {
                if (Set(ref _comensalesEntrada, value))
                {
                    ActualizarComandos();
                }
            }
        }
        public int PlatosEnComandaSeleccionada =>
    (MesaSeleccionada != null && MesaSeleccionada.ComandaActual != null)
    ? MesaSeleccionada.ComandaActual.TotalPlatos()
    : 0;

        public RelayCommand CmdReservar { get; private set; }
        public RelayCommand CmdOcuparSinComanda { get; private set; }
        public RelayCommand CmdLiberar { get; private set; }
        public RelayCommand CmdNuevaSesion { get; private set; }
        public event EventHandler SesionReiniciada;

        public SalaViewModel()
        {
            Sesion = DemoData.CrearSesionDemo();

            Mesas = new ObservableCollection<Mesa>(Sesion.Mesas);

            CmdReservar = new RelayCommand(_ => CambiarAReservada(), _ => PuedeReservar());
            CmdOcuparSinComanda = new RelayCommand(_ => CambiarAOcupadaSinComanda(), _ => PuedeOcuparSinComanda());
            CmdLiberar = new RelayCommand(_ => CambiarALibre(), _ => PuedeLiberar());
            CmdNuevaSesion = new RelayCommand(_ => NuevaSesion(), _ => true);
            CmdEditarComanda = new RelayCommand(
                _ => EditarComanda(),
                _ => MesaSeleccionada != null &&
                     (MesaSeleccionada.Estado == EstadoMesa.OcupadaSinComanda ||
                      MesaSeleccionada.Estado == EstadoMesa.OcupadaConComanda)
            );
            _comandosInicializados = true;

        }

        private void ActualizarComandos()
        {
            if (!_comandosInicializados) return;
            CmdReservar?.RaiseCanExecuteChanged();
            CmdOcuparSinComanda?.RaiseCanExecuteChanged();
            CmdLiberar?.RaiseCanExecuteChanged();
            CmdEditarComanda?.RaiseCanExecuteChanged();
            CmdNuevaSesion?.RaiseCanExecuteChanged();
        }

        private bool TieneMesaSel() { return MesaSeleccionada != null; }

        private bool PuedeReservar()
        {
            return TieneMesaSel() && MesaSeleccionada.Estado == EstadoMesa.Libre && ComensalesEntradaDentroDeAforoMinimo();
        }
        private bool PuedeOcuparSinComanda()
        {
            if (!TieneMesaSel()) return false;
            var e = MesaSeleccionada.Estado;
            return (e == EstadoMesa.Libre || e == EstadoMesa.Reservada) && ComensalesEntradaDentroDeAforoMinimo();
        }
        private bool PuedeLiberar()
        {
            if (!TieneMesaSel()) return false;
            var e = MesaSeleccionada.Estado;
            return e == EstadoMesa.OcupadaConComanda || e == EstadoMesa.OcupadaSinComanda || e == EstadoMesa.Reservada;
        }
        private bool ComensalesEntradaDentroDeAforoMinimo()
        {
            if (!TieneMesaSel()) return false;
            return ComensalesEntrada > 0 && ComensalesEntrada <= MesaSeleccionada.CapacidadMaxima;
        }

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
        private void CambiarALibre()
        {
            if (!PuedeLiberar()) { Aviso("Solo se puede liberar desde 'Reservada' u 'Ocupada'."); return; }
            MesaSeleccionada.ComandaActual = null;
            MesaSeleccionada.ComensalesActuales = 0;
            MesaSeleccionada.Estado = EstadoMesa.Libre;
            OnPropertyChanged(nameof(MesaSeleccionada));
            ActualizarComandos();
        }

        public void NuevaSesion()
        {

            if (!ConfirmarReinicioSesion())
                return;

            MesaSeleccionada = null;
            ComensalesEntrada = 0;

            foreach (var m in Sesion.Mesas)
            {
                m.Estado = EstadoMesa.Libre;
                m.ComensalesActuales = 0;
                m.ComandaActual = null;
                m.ComandasHistorial.Clear();
            }

            OnPropertyChanged(nameof(Mesas));
            OnPropertyChanged(nameof(MesaSeleccionada));
            ActualizarComandos();

            SesionReiniciada?.Invoke(this, EventArgs.Empty);
        }

        private void Aviso(string msg)
        {
            MostrarAviso?.Invoke(msg);
        }
        private void EditarComanda()
        {
            if (MesaSeleccionada == null) return;

            var lineasIniciales = (MesaSeleccionada.ComandaActual != null)
                ? MesaSeleccionada.ComandaActual.Lineas
                : new System.Collections.Generic.List<LineaComanda>();

            var vm = new ComandaEditorViewModel(Sesion.Carta, lineasIniciales);
            var dlg = new ComandaDialog(vm) { Owner = System.Windows.Application.Current.MainWindow };
            var ok = dlg.ShowDialog();
            if (ok == true)
            {
                var resultado = vm.ConstruirResultado();

                if (resultado == null || resultado.Count == 0)
                {
                    MesaSeleccionada.ComandaActual = null;
                    if (MesaSeleccionada.Estado == EstadoMesa.OcupadaConComanda)
                        MesaSeleccionada.Estado = EstadoMesa.OcupadaSinComanda;

                    OnPropertyChanged(nameof(MesaSeleccionada));
                    ActualizarComandos();
                    return;
                }

                if (MesaSeleccionada.ComandaActual == null)
                {
                    MesaSeleccionada.ComandaActual = new Comanda
                    {
                        MesaId = MesaSeleccionada.Id,
                        FechaHora = System.DateTime.Now,
                        Lineas = new System.Collections.Generic.List<LineaComanda>()
                    };
                    MesaSeleccionada.ComandasHistorial.Add(MesaSeleccionada.ComandaActual);
                }

                MesaSeleccionada.ComandaActual.Lineas = resultado;

                if (MesaSeleccionada.Estado != EstadoMesa.OcupadaConComanda)
                    MesaSeleccionada.Estado = EstadoMesa.OcupadaConComanda;

                if (MesaSeleccionada.ComensalesActuales == 0 && ComensalesEntrada > 0 && ComensalesEntradaDentroDeAforoMinimo())
                    MesaSeleccionada.ComensalesActuales = ComensalesEntrada;

                OnPropertyChanged(nameof(MesaSeleccionada));
                ActualizarComandos();
            }
        }
    }
}