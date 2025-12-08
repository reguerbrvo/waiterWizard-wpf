using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using RestaurantSala.Core.Data;
using RestaurantSala.Core.Models;

namespace RestaurantSala
{
    public class SalaViewModel : ObservableObject
    {
        private bool _comandosInicializados;
        public Func<bool> ConfirmarReinicioSesion; 
        private Sesion _sesion;
        public Sesion Sesion
        {
            get { return _sesion; }
            set
            {
                if (ReferenceEquals(_sesion, value)) return;
                _sesion = value;

                // Refresca la colección existente de Mesas
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
        public int PlatosEnComandaSeleccionada =>
    (MesaSeleccionada != null && MesaSeleccionada.ComandaActual != null)
    ? MesaSeleccionada.ComandaActual.TotalPlatos()
    : 0;


        // Comandos de estado
        public RelayCommand CmdReservar { get; private set; }
        public RelayCommand CmdOcuparSinComanda { get; private set; }
        public RelayCommand CmdOcuparConComanda { get; private set; }
        public RelayCommand CmdLiberar { get; private set; }
        public RelayCommand CmdNuevaSesion { get; private set; }
        public event EventHandler SesionReiniciada;

        public SalaViewModel()
        {
            Sesion = DemoData.CrearSesionDemo();

            Mesas = new ObservableCollection<Mesa>(Sesion.Mesas);

            CmdReservar = new RelayCommand(_ => CambiarAReservada(), _ => PuedeReservar());
            CmdOcuparSinComanda = new RelayCommand(_ => CambiarAOcupadaSinComanda(), _ => PuedeOcuparSinComanda());
            CmdOcuparConComanda = new RelayCommand(_ => CambiarAOcupadaConComanda(), _ => PuedeOcuparConComanda());
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
            CmdOcuparConComanda?.RaiseCanExecuteChanged();
            CmdLiberar?.RaiseCanExecuteChanged();
            CmdEditarComanda?.RaiseCanExecuteChanged();
            CmdNuevaSesion?.RaiseCanExecuteChanged();
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

        public void NuevaSesion()
        {

            if (ConfirmarReinicioSesion != null && !ConfirmarReinicioSesion())
                return;

            // 1) Limpiar selección y entrada de comensales
            MesaSeleccionada = null;
            ComensalesEntrada = 0;


            // 2) Reiniciar el estado de TODAS las mesas (ENUNCIADO 4)
            foreach (var m in Sesion.Mesas)
            {
                m.Estado = EstadoMesa.Libre;
                m.ComensalesActuales = 0;
                m.ComandaActual = null;
                m.ComandasHistorial.Clear(); // NUEVA SESIÓN => historial vacío (se guarda por sesión)
            }


            // 3) Notificar cambios globales (para grids/secundaria/estadísticas)
            OnPropertyChanged(nameof(Mesas)); // si expones "Mesas" desde el VM
            OnPropertyChanged(nameof(MesaSeleccionada));
            ActualizarComandos();


            // 4) Aviso opcional (evento) para que vistas hagan refresco visual extra si lo necesitan
            SesionReiniciada?.Invoke(this, EventArgs.Empty);
        }

        private void Aviso(string msg)
        {
            MessageBox.Show(msg, "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void EditarComanda()
        {
            if (MesaSeleccionada == null) return;

            // 1) NO crear ComandaActual por adelantado. Solo preparar líneas iniciales.
            var lineasIniciales = (MesaSeleccionada.ComandaActual != null)
                ? MesaSeleccionada.ComandaActual.Lineas
                : new System.Collections.Generic.List<LineaComanda>();

            var vm = new ComandaEditorViewModel(Sesion.Carta, lineasIniciales);
            var dlg = new ComandaDialog(vm) { Owner = System.Windows.Application.Current.MainWindow };
            var ok = dlg.ShowDialog();
            if (ok == true)
            {
                // 2) Recoger resultado
                var resultado = vm.ConstruirResultado();

                // 3) Si NO hay líneas ⇒ NO hay comanda. Mantener OcupadaSinComanda (o el estado previo) y ComandaActual=null
                if (resultado == null || resultado.Count == 0)
                {
                    // Si existía un cascarón, lo anulamos
                    MesaSeleccionada.ComandaActual = null;
                    if (MesaSeleccionada.Estado == EstadoMesa.OcupadaConComanda)
                        MesaSeleccionada.Estado = EstadoMesa.OcupadaSinComanda; // mantiene ocupación, sin comanda

                    OnPropertyChanged(nameof(MesaSeleccionada));
                    ActualizarComandos();
                    return;
                }

                // 4) Con líneas ⇒ asegurar ComandaActual y estado
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

                // Reemplazar líneas
                MesaSeleccionada.ComandaActual.Lineas = resultado;

                // Promocionar a OcupadaConComanda solo si procede
                if (MesaSeleccionada.Estado != EstadoMesa.OcupadaConComanda)
                    MesaSeleccionada.Estado = EstadoMesa.OcupadaConComanda;

                // 5) NO pisar comensales: solo establecer si había 0 y el usuario introdujo un valor > 0
                if (MesaSeleccionada.ComensalesActuales == 0 && ComensalesEntrada > 0 && ComensalesEntradaDentroDeAforoMinimo())
                    MesaSeleccionada.ComensalesActuales = ComensalesEntrada;

                // 6) Notificar para refrescar panel/estadísticas/mini-gráfico
                OnPropertyChanged(nameof(MesaSeleccionada));
                ActualizarComandos();
            }
        }
    }
}