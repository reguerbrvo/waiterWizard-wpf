using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Win32;
using RestaurantSala.Core.Data.Persistence;
using RestaurantSala.Core.Models;
using RestaurantSala.Core.Utils;

namespace RestaurantSala
{
    public partial class MainWindow : Window
    {
        private VentanaSecundaria _secundaria;
        private readonly Dictionary<string, Brush> _palette = new Dictionary<string, Brush>();
        private readonly Brush BR_PRIM = (Brush)new BrushConverter().ConvertFromString("#4E79A7");
        private readonly Brush BR_SEG = (Brush)new BrushConverter().ConvertFromString("#59A14F");
        private readonly Brush BR_POST = (Brush)new BrushConverter().ConvertFromString("#F28E2B");
        private readonly Brush BR_ANILLO_BG = Brushes.White;
        private readonly Brush BR_ANILLO_STROKE = Brushes.Gray;

        private string UltimaSesionPath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "RestaurantSala",
            "ultima_sesion.json");

        private SalaViewModel ViewModel => (SalaViewModel)DataContext;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new SalaViewModel();
            ConfigureViewModel();
            Loaded += OnLoaded;
        }

        private void ConfigureViewModel()
        {
            ViewModel.PropertyChanged += OnViewModelPropertyChanged;
            ViewModel.ConfirmarReinicioSesion = ConfirmarReinicioSesion;
            ViewModel.MostrarAviso = MostrarAviso;
        }

        private bool ConfirmarReinicioSesion()
        {
            if (!Properties.Settings.Default.PreguntarAntesReiniciar)
            {
                return true;
            }

            var res = MessageBox.Show(
                "Se reiniciará la sesión: todas las mesas quedarán libres y se eliminarán las comandas.\n\n¿Quieres continuar?",
                "Nueva sesión",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return res == MessageBoxResult.Yes;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (!RestaurarSesionAnterior())
            {
                DibujarSala();
                DibujarEstadisticas();
            }
        }

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            RedibujarSalaManteniendoSeleccion();
            DibujarEstadisticas();
        }

        private bool RestaurarSesionAnterior()
        {
            try
            {
                if (File.Exists(UltimaSesionPath))
                {
                    CargarSesionDesde(UltimaSesionPath);
                    return true;
                }
            }
            catch
            {
            }

            return false;
        }

        private void CargarSesionDesde(string ruta)
        {
            var sesion = JsonSesionStore.Cargar(ruta);
            ViewModel.Sesion = sesion;
            ViewModel.MesaSeleccionada = null;
            DibujarSala();
            DibujarEstadisticas();
        }

        private void MostrarAviso(string mensaje)
        {
            MessageBox.Show(mensaje, "Aviso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OnAbrirSecundariaClick(object sender, RoutedEventArgs e)
        {
            if (_secundaria == null)
            {
                _secundaria = new VentanaSecundaria(ViewModel);
                _secundaria.Owner = this;
                _secundaria.Closed += (s, args) => _secundaria = null;
                _secundaria.Show();
            }
            else
            {
                _secundaria.Activate();
            }
        }

        private void OnPlanoSalaMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pt = e.GetPosition(PlanoSala);
            var hit = PlanoSala.InputHitTest(pt) as DependencyObject;

            int? id = null;
            for (DependencyObject d = hit; d != null; d = VisualTreeHelper.GetParent(d))
            {
                var el = d as Ellipse;
                if (el != null && el.Tag is int)
                {
                    id = (int)el.Tag;
                    break;
                }
            }

            if (!id.HasValue) return;

            var mesa = ViewModel.Sesion.Mesas.FirstOrDefault(m => m.Id == id.Value);
            if (mesa != null) ViewModel.MesaSeleccionada = mesa;
        }

        private void DibujarSala()
        {
            PlanoSala.Children.Clear();

            double cellW = 160, cellH = 140;
            int col = 0, fil = 0;

            foreach (var mesa in ViewModel.Mesas)
            {
                var el = new Ellipse
                {
                    Width = 90,
                    Height = 90,
                    StrokeThickness = 3,
                    Stroke = Brushes.DarkSlateGray,
                    Fill = BrushPorEstado(mesa.Estado),
                    Tag = mesa.Id
                };

                double x = 40 + col * cellW;
                double y = 40 + fil * cellH;
                Canvas.SetLeft(el, x);
                Canvas.SetTop(el, y);
                PlanoSala.Children.Add(el);

                var tb = new TextBlock
                {
                    Text = "M" + mesa.Id,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(tb, x + 30);
                Canvas.SetTop(tb, y + 35);
                PlanoSala.Children.Add(tb);
                DibujarMiniAnilloMesa(mesa, x, y);

                col++;
                if (col == 3) { col = 0; fil++; }
            }
        }

        private Brush BrushPorEstado(EstadoMesa estado)
        {
            switch (estado)
            {
                case EstadoMesa.Libre: return Brushes.White;
                case EstadoMesa.Reservada: return Brushes.LightGoldenrodYellow;
                case EstadoMesa.OcupadaSinComanda: return Brushes.LightSkyBlue;
                case EstadoMesa.OcupadaConComanda: return Brushes.LightCoral;
                default: return Brushes.White;
            }
        }


        private void ResaltarSeleccion(int idSel)
        {
            foreach (UIElement child in PlanoSala.Children)
            {
                var el = child as Ellipse;
                if (el == null) continue;

                if (!(el.Tag is int)) continue;

                int id = (int)el.Tag;
                el.Stroke = (id == idSel) ? Brushes.OrangeRed : Brushes.DarkSlateGray;
                el.StrokeThickness = (id == idSel) ? 5 : 3;
            }
        }

        private Brush GetBrushForKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return Brushes.Gray;
            if (_palette.ContainsKey(key)) return _palette[key];
            int h = key.GetHashCode();
            byte r = (byte)(50 + (h & 0x7F));
            byte g = (byte)(50 + ((h >> 7) & 0x7F));
            byte b = (byte)(50 + ((h >> 14) & 0x7F));
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            _palette[key] = brush;
            return brush;
        }
        private void RedibujarSalaManteniendoSeleccion()
        {
            int? idSel = ViewModel.MesaSeleccionada != null ? (int?)ViewModel.MesaSeleccionada.Id : null;
            DibujarSala();
            if (idSel.HasValue) ResaltarSeleccion(idSel.Value);
        }

        private void DibujarEstadisticas()
        {
            if (CanvasStats == null) return;
            CanvasStats.Children.Clear();

            if (ViewModel.MesaSeleccionada == null)
                DibujarStatsGenerales();
            else
                DibujarStatsPorMesa(ViewModel.MesaSeleccionada);
        }

        private void DibujarStatsGenerales()
        {

            var datos = ViewModel.Mesas
                .Select(m => new { Mesa = m, Total = Estadisticas.TotalPlatosServidos(m) })
                .ToList();

            const double paddingRight = 40;

            double barsWidth = Math.Max(520, datos.Count * 70 + 80);
            double ancho = barsWidth + paddingRight;
            double alto = 400;
            CanvasStats.Width = ancho; CanvasStats.Height = alto;

            double max = Math.Max(1, datos.Max(d => d.Total));
            double x0 = 50, y0 = alto - 50;
            double barW = 40, gap = 30;

            CanvasStats.Children.Add(new Line { X1 = x0, Y1 = 20, X2 = x0, Y2 = y0, Stroke = Brushes.Black });
            CanvasStats.Children.Add(new Line { X1 = x0, Y1 = y0, X2 = ancho - paddingRight, Y2 = y0, Stroke = Brushes.Black });

            for (int i = 0; i < datos.Count; i++)
            {
                double x = x0 + gap + i * (barW + gap);
                double h = (datos[i].Total / max) * (y0 - 30);
                var rect = new Rectangle
                {
                    Width = barW,
                    Height = h,
                    Fill = Brushes.SteelBlue,
                    Stroke = Brushes.DimGray,
                    StrokeThickness = 1,
                    ToolTip = $"Mesa {datos[i].Mesa.Id}: {datos[i].Total} platos"
                };
                Canvas.SetLeft(rect, x);
                Canvas.SetTop(rect, y0 - h);
                CanvasStats.Children.Add(rect);

                var lbl = new TextBlock { Text = "M" + datos[i].Mesa.Id, FontSize = 12 };
                Canvas.SetLeft(lbl, x);
                Canvas.SetTop(lbl, y0 + 4);
                CanvasStats.Children.Add(lbl);
            }
        }

        private void DibujarStatsPorMesa(Mesa mesa)
        {

            var lineas = (mesa.ComandaActual?.Lineas) ?? Enumerable.Empty<LineaComanda>();

            if (!lineas.Any())
            {
                CanvasStats.Width = 640;
                CanvasStats.Height = 360;
                CanvasStats.Children.Clear();

                var msg = new TextBlock { Text = "Sin comanda activa", FontStyle = FontStyles.Italic, Opacity = 0.7 };
                Canvas.SetLeft(msg, 20);
                Canvas.SetTop(msg, 20);
                CanvasStats.Children.Add(msg);
                return;
            }

            var porCat = new[] { CategoriaPlato.Primero, CategoriaPlato.Segundo, CategoriaPlato.Postre }
                .Select(cat => new {
                    Cat = cat,
                    Platos = lineas.Where(l => l.Plato != null && l.Plato.Categoria == cat)
                                   .GroupBy(l => l.Plato.Codigo)
                                   .Select(g => new { Codigo = g.Key, Nombre = g.First().Plato.Nombre, Total = g.Sum(x => x.Cantidad) })
                                   .OrderByDescending(x => x.Total)
                                   .ToList()
                })
                .ToList();

            const double legendWidth = 180;
            const double paddingRight = 30;

            double barW = 90, gap = 90;
            double x0 = 60, canvasH = 440, y0 = canvasH - 60;
            double barsAreaWidth = x0 + gap + 3 * (barW + gap);
            double canvasW = Math.Max(700, barsAreaWidth + legendWidth + paddingRight);

            CanvasStats.Width = canvasW;
            CanvasStats.Height = canvasH;

            CanvasStats.Children.Add(new Line { X1 = x0, Y1 = 30, X2 = x0, Y2 = y0, Stroke = Brushes.Black });
            CanvasStats.Children.Add(new Line { X1 = x0, Y1 = y0, X2 = barsAreaWidth, Y2 = y0, Stroke = Brushes.Black });

            int maxTotal = Math.Max(1, porCat.Max(c => c.Platos.Sum(p => p.Total)));

            for (int i = 0; i < porCat.Count; i++)
            {
                double xBar = x0 + gap + i * (barW + gap);
                double acc = 0.0;
                foreach (var p in porCat[i].Platos)
                {
                    double hSeg = (p.Total / (double)maxTotal) * (y0 - 30);
                    var rect = new Rectangle
                    {
                        Width = barW,
                        Height = hSeg,
                        Fill = GetBrushForKey(p.Codigo),
                        Stroke = Brushes.DimGray,
                        StrokeThickness = 1,
                        ToolTip = $"{p.Nombre}: {p.Total}"
                    };
                    Canvas.SetLeft(rect, xBar);
                    Canvas.SetTop(rect, y0 - acc - hSeg);
                    CanvasStats.Children.Add(rect);
                    acc += hSeg;
                }

                var lbl = new TextBlock { Text = porCat[i].Cat.ToString(), FontWeight = FontWeights.Bold };
                Canvas.SetLeft(lbl, xBar - 6);
                Canvas.SetTop(lbl, y0 + 4);
                CanvasStats.Children.Add(lbl);
            }

            DibujarLeyenda(
                porCat.SelectMany(c => c.Platos)
                      .GroupBy(p => p.Codigo)
                      .Select(g => new { g.First().Codigo, g.First().Nombre })
                      .OrderBy(x => x.Nombre)
                      .Cast<object>()
                      .ToList(),
                legendLeft: canvasW - legendWidth + 10,
                legendTop: 40
            );
        }

        private void DibujarLeyenda(IList<object> items, double legendLeft, double legendTop)
        {
            double x = legendLeft;
            double y = legendTop;

            var titulo = new TextBlock { Text = "Leyenda", FontWeight = FontWeights.Bold };
            Canvas.SetLeft(titulo, x);
            Canvas.SetTop(titulo, y - 24);
            CanvasStats.Children.Add(titulo);

            foreach (dynamic it in items)
            {
                var swatch = new Rectangle { Width = 14, Height = 14, Fill = GetBrushForKey((string)it.Codigo), Stroke = Brushes.Gray, StrokeThickness = 0.5 };
                Canvas.SetLeft(swatch, x);
                Canvas.SetTop(swatch, y);
                CanvasStats.Children.Add(swatch);

                var lbl = new TextBlock { Text = it.Nombre, FontSize = 12 };
                Canvas.SetLeft(lbl, x + 20);
                Canvas.SetTop(lbl, y - 2);
                CanvasStats.Children.Add(lbl);

                y += 20;
            }
        }
        

        private void DibujarMiniAnilloMesa(Mesa mesa, double xMesa, double yMesa)
        {
            var (tPrim, tSeg, tPost) = Estadisticas.TotalesPorCategoriaActual(mesa);
            int total = Math.Max(0, tPrim + tSeg + tPost);

            double cx = xMesa + 45;
            double cy = yMesa + 110;

            double rOuter = 18;
            double rInner = 10;

            var donutBg = new Ellipse { Width = rOuter * 2, Height = rOuter * 2, Fill = BR_ANILLO_BG, Stroke = BR_ANILLO_STROKE, StrokeThickness = 0.5, IsHitTestVisible = false };
            Canvas.SetLeft(donutBg, cx - rOuter); Canvas.SetTop(donutBg, cy - rOuter);
            PlanoSala.Children.Add(donutBg);

            if (total == 0)
            {
                var hole = new Ellipse { Width = rInner * 2, Height = rInner * 2, Fill = Brushes.White, IsHitTestVisible = false };
                Canvas.SetLeft(hole, cx - rInner); Canvas.SetTop(hole, cy - rInner);
                PlanoSala.Children.Add(hole);
                return;
            }

            int nonZero = (tPrim > 0 ? 1 : 0) + (tSeg > 0 ? 1 : 0) + (tPost > 0 ? 1 : 0);
            if (nonZero == 1)
            {
                Brush unico = tPrim > 0 ? BR_PRIM : (tSeg > 0 ? BR_SEG : BR_POST);
                donutBg.Fill = unico;
                var holeA = new Ellipse { Width = rInner * 2, Height = rInner * 2, Fill = Brushes.White, IsHitTestVisible = false };
                Canvas.SetLeft(holeA, cx - rInner); Canvas.SetTop(holeA, cy - rInner);
                PlanoSala.Children.Add(holeA);
                return;
            }

            double angPrim = 360.0 * tPrim / total;
            double angSeg = 360.0 * tSeg / total;
            double angPost = 360.0 * tPost / total;

            double start = -90;
            if (tPrim > 0) { DrawDonutSegment(cx, cy, rInner, rOuter, start, angPrim, BR_PRIM, $"Primeros: {tPrim}"); start += angPrim; }
            if (tSeg > 0) { DrawDonutSegment(cx, cy, rInner, rOuter, start, angSeg, BR_SEG, $"Segundos: {tSeg}"); start += angSeg; }
            if (tPost > 0) { DrawDonutSegment(cx, cy, rInner, rOuter, start, angPost, BR_POST, $"Postres: {tPost}"); }

            var hole2 = new Ellipse { Width = rInner * 2, Height = rInner * 2, Fill = Brushes.White, IsHitTestVisible = false };
            Canvas.SetLeft(hole2, cx - rInner); Canvas.SetTop(hole2, cy - rInner);
            PlanoSala.Children.Add(hole2);
        }

        private void DrawDonutSegment(double cx, double cy, double rInner, double rOuter,
                                      double startAngleDeg, double sweepAngleDeg,
                                      Brush fill, string tooltip)
        {
            if (sweepAngleDeg <= 0.1) return;

            double toRad = Math.PI / 180.0;
            double a0 = startAngleDeg * toRad;
            double a1 = (startAngleDeg + sweepAngleDeg) * toRad;

            var p0 = new System.Windows.Point(cx + rOuter * Math.Cos(a0), cy + rOuter * Math.Sin(a0));
            var p1 = new System.Windows.Point(cx + rOuter * Math.Cos(a1), cy + rOuter * Math.Sin(a1));
            var q1 = new System.Windows.Point(cx + rInner * Math.Cos(a1), cy + rInner * Math.Sin(a1));
            var q0 = new System.Windows.Point(cx + rInner * Math.Cos(a0), cy + rInner * Math.Sin(a0));

            bool largeArc = sweepAngleDeg > 180.0;

            var fig = new PathFigure { StartPoint = p0, IsClosed = true };
            fig.Segments.Add(new ArcSegment { Point = p1, Size = new Size(rOuter, rOuter), IsLargeArc = largeArc, SweepDirection = SweepDirection.Clockwise });
            fig.Segments.Add(new LineSegment { Point = q1 });
            fig.Segments.Add(new ArcSegment { Point = q0, Size = new Size(rInner, rInner), IsLargeArc = largeArc, SweepDirection = SweepDirection.Counterclockwise });

            var geo = new PathGeometry();
            geo.Figures.Add(fig);

            var path = new Path { Data = geo, Fill = fill, Stroke = BR_ANILLO_STROKE, StrokeThickness = 0.4, ToolTip = tooltip };
            PlanoSala.Children.Add(path);
        }

        private void DibujarLeyendaPlano()
        {
            double x = 8, y = 8;

            var frame = new Rectangle { Width = 170, Height = 78, RadiusX = 6, RadiusY = 6, Fill = Brushes.White, Stroke = Brushes.Gray, StrokeThickness = 0.8, Opacity = 0.9, IsHitTestVisible = false };
            Canvas.SetLeft(frame, x); Canvas.SetTop(frame, y);
            PlanoSala.Children.Add(frame);

            var title = new TextBlock { Text = "Leyenda", FontWeight = FontWeights.Bold, IsHitTestVisible = false };
            Canvas.SetLeft(title, x + 8); Canvas.SetTop(title, y + 6);
            PlanoSala.Children.Add(title);

            DibujarItemLeyenda(x + 10, y + 28, BR_PRIM, "Primeros");
            DibujarItemLeyenda(x + 10, y + 46, BR_SEG, "Segundos");
            DibujarItemLeyenda(x + 10, y + 64, BR_POST, "Postres");
        }

        private void DibujarItemLeyenda(double x, double y, Brush brush, string texto)
        {
            var sw = new Rectangle { Width = 14, Height = 14, Fill = brush, Stroke = Brushes.Gray, StrokeThickness = 0.5, IsHitTestVisible = false };
            Canvas.SetLeft(sw, x); Canvas.SetTop(sw, y);
            PlanoSala.Children.Add(sw);

            var lbl = new TextBlock { Text = texto, FontSize = 12, IsHitTestVisible = false };
            Canvas.SetLeft(lbl, x + 20); Canvas.SetTop(lbl, y - 2);
            PlanoSala.Children.Add(lbl);
        }
        private void OnGuardarSesionClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog { Filter = "Sesión JSON|*.json", FileName = "sesion.json" };
            if (dlg.ShowDialog() == true)
            {
                JsonSesionStore.Guardar(ViewModel.Sesion, dlg.FileName);
            }
        }

        private void OnCargarSesionClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Sesión JSON|*.json" };
            if (dlg.ShowDialog() == true)
            {
                CargarSesionDesde(dlg.FileName);
            }
        }

        private void OnSalirClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public bool PreguntarAntesReiniciar
        {
            get => Properties.Settings.Default.PreguntarAntesReiniciar;
            set { Properties.Settings.Default.PreguntarAntesReiniciar = value; Properties.Settings.Default.Save(); }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(UltimaSesionPath));
                JsonSesionStore.Guardar(ViewModel.Sesion, UltimaSesionPath);
            }
            catch { }

            base.OnClosing(e);
        }
        private void OnMenuNoPreguntarLoaded(object sender, RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            mi.IsChecked = !Properties.Settings.Default.PreguntarAntesReiniciar;
        }

        private void OnMenuNoPreguntarClick(object sender, RoutedEventArgs e)
        {
            var mi = (MenuItem)sender;
            Properties.Settings.Default.PreguntarAntesReiniciar = !mi.IsChecked;
            Properties.Settings.Default.Save();
        }
    }
}
