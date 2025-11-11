using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using RestaurantSala.Core.Models;
using RestaurantSala.Core.Utils; // para Estadisticas

namespace RestaurantSala
{
    public partial class MainWindow : Window
    {
        private VentanaSecundaria _secundaria; // instancia única (no modal)

        public MainWindow()
        {
            DataContext = new SalaViewModel();
            InitializeComponent();
            VM.PropertyChanged += VM_PropertyChanged; // <<-- escuchar cambios (MesaSeleccionada)
            Loaded += OnLoaded;
        }

        private SalaViewModel VM { get { return (SalaViewModel)DataContext; } }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DibujarSala();
            DibujarEstadisticas();
        }

        // Sincroniza Canvas cuando cambia MesaSeleccionada desde cualquier ventana
        private void VM_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SalaViewModel.MesaSeleccionada))
            {
                if (VM.MesaSeleccionada != null)
                    ResaltarSeleccion(VM.MesaSeleccionada.Id);
                    ActualizarColores();
                    DibujarEstadisticas();
            }
        }
        private void AbrirSecundaria_Click(object sender, RoutedEventArgs e)
        {
            if (_secundaria == null)
            {
                _secundaria = new VentanaSecundaria(VM); // pasar la MISMA VM
                _secundaria.Owner = this;
                _secundaria.Closed += (s, args) => _secundaria = null; // permitir reabrir
                _secundaria.Show();
            }
            else
            {
                _secundaria.Activate();
            }
        }

        private void PlanoSala_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(PlanoSala);

            foreach (UIElement child in PlanoSala.Children)
            {
                var el = child as Ellipse;
                if (el == null) continue;

                double x = Canvas.GetLeft(el);
                double y = Canvas.GetTop(el);
                var rect = new Rect(x, y, el.Width, el.Height);

                if (rect.Contains(p))
                {
                    int id = (int)el.Tag;
                    VM.MesaSeleccionada = VM.Mesas.First(m => m.Id == id); // esto disparará VM_PropertyChanged
                    break;
                }
            }
        }
        private void DibujarSala()
        {
            PlanoSala.Children.Clear();

            // Distribución fija simple en cuadrícula 3xN
            double cellW = 160, cellH = 140;
            int col = 0, fil = 0;

            foreach (var mesa in VM.Mesas)
            {
                // Figura de mesa
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

                // Etiqueta ID
                var tb = new TextBlock
                {
                    Text = "M" + mesa.Id,
                    FontWeight = FontWeights.Bold,
                    Foreground = Brushes.Black
                };
                Canvas.SetLeft(tb, x + 30);
                Canvas.SetTop(tb, y + 35);
                PlanoSala.Children.Add(tb);

                // Siguiente celda
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

                int id = (int)el.Tag;
                el.Stroke = (id == idSel) ? Brushes.OrangeRed : Brushes.DarkSlateGray;
                el.StrokeThickness = (id == idSel) ? 5 : 3;
            }
        }
        private void ActualizarColores()
        {
            foreach (UIElement child in PlanoSala.Children)
            {
                var el = child as Ellipse; if (el == null) continue;
                int id = (int)el.Tag;
                var mesa = VM.Mesas.FirstOrDefault(m => m.Id == id);
                if (mesa != null)
                    el.Fill = BrushPorEstado(mesa.Estado);
            }
        }
        private void RefrescarTodo()
        {
            ActualizarColores();
            DibujarEstadisticas();
        }

        private readonly Dictionary<string, Brush> _palette = new Dictionary<string, Brush>();
        private Brush GetBrushForKey(string key)
        {
            if (string.IsNullOrEmpty(key)) return Brushes.Gray;
            if (_palette.ContainsKey(key)) return _palette[key];
            // Genera colores deterministas por hash simple
            int h = key.GetHashCode();
            byte r = (byte)(50 + (h & 0x7F));
            byte g = (byte)(50 + ((h >> 7) & 0x7F));
            byte b = (byte)(50 + ((h >> 14) & 0x7F));
            var brush = new SolidColorBrush(Color.FromRgb(r, g, b));
            _palette[key] = brush;
            return brush;
        }

        private void DibujarEstadisticas()
        {
            if (CanvasStats == null) return; // si aún no se construyó
            CanvasStats.Children.Clear();

            if (VM.MesaSeleccionada == null)
                DibujarStatsGenerales();
            else
                DibujarStatsPorMesa(VM.MesaSeleccionada);
        }

        // --- Modo 1: Sin selección => barras por mesa (total de platos servidos) ---
        private void DibujarStatsGenerales()
        {

            var datos = VM.Mesas
                .Select(m => new { Mesa = m, Total = Estadisticas.TotalPlatosServidos(m) })
                .ToList();

            const double paddingRight = 40; // margen derecho para respiración

            double barsWidth = Math.Max(520, datos.Count * 70 + 80);
            double ancho = barsWidth + paddingRight;
            double alto = 400;
            CanvasStats.Width = ancho; CanvasStats.Height = alto;

            double max = Math.Max(1, datos.Max(d => d.Total));
            double x0 = 50, y0 = alto - 50; // origen
            double barW = 40, gap = 30;

            // Ejes
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

        // --- Modo 2: Con selección => 3 barras por categoría, segmentadas por plato ---
        private void DibujarStatsPorMesa(Mesa mesa)
        {

            // Agrupar totales por plato dentro de cada categoría
            var lineas = mesa.ComandasHistorial.SelectMany(c => c.Lineas);
            var porCat = new[] { CategoriaPlato.Primero, CategoriaPlato.Segundo, CategoriaPlato.Postre }
                .Select(cat => new
                {
                    Cat = cat,
                    Platos = lineas.Where(l => l.Plato != null && l.Plato.Categoria == cat)
                                   .GroupBy(l => l.Plato.Codigo)
                                   .Select(g => new { Codigo = g.Key, Nombre = g.First().Plato.Nombre, Total = g.Sum(x => x.Cantidad) })
                                   .OrderByDescending(x => x.Total)
                                   .ToList()
                })
                .ToList();

            const double legendWidth = 180; // reservar espacio para la leyenda
            const double paddingRight = 30;
            double alto = 440;
            double x0 = 60, y0 = alto - 60;

            // Área de barras: 3 barras anchas separadas
            double barW = 90; double gap = 90;
            double barsAreaWidth = x0 + gap + 3 * (barW + gap); // usa x0 real + grupos + margen
            double ancho = Math.Max(700, barsAreaWidth + legendWidth + paddingRight);
            CanvasStats.Width = ancho; CanvasStats.Height = alto;

            // Ejes
            CanvasStats.Children.Add(new Line { X1 = x0, Y1 = 30, X2 = x0, Y2 = y0, Stroke = Brushes.Black });
            CanvasStats.Children.Add(new Line { X1 = x0, Y1 = y0, X2 = barsAreaWidth, Y2 = y0, Stroke = Brushes.Black });

            int maxTotal = Math.Max(1, porCat.Max(c => c.Platos.Sum(p => p.Total)));

            for (int i = 0; i < porCat.Count; i++)
            {
                double xBar = x0 + gap + i * (barW + gap);

                double acc = 0.0; // usar double para evitar truncado
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

                // Etiqueta categoría
                var lbl = new TextBlock { Text = porCat[i].Cat.ToString(), FontWeight = FontWeights.Bold };
                Canvas.SetLeft(lbl, xBar - 6);
                Canvas.SetTop(lbl, y0 + 4);
                CanvasStats.Children.Add(lbl);
            }

            // Leyenda: dibujar fuera del área de barras
            DibujarLeyenda(
                porCat.SelectMany(c => c.Platos)
                      .GroupBy(p => p.Codigo)
                      .Select(g => new { g.First().Codigo, g.First().Nombre })
                      .OrderBy(x => x.Nombre)
                      .Cast<object>()
                      .ToList(),
                legendLeft: ancho - legendWidth + 10,
                legendTop: 40);
        }

        private void DibujarLeyenda(IList<object> items, double legendLeft, double legendTop)
        {
            // Crea una leyenda flotante a la derecha, fuera del área de barras
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
    }
}
