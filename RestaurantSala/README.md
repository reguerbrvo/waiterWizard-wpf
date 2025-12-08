# RestaurantSala

Aplicación WPF de ejemplo para la gestión básica de mesas y comandas.

## Convenciones adoptadas tras la limpieza
- Patrón MVVM: vistas (`*.xaml`) enlazadas a `SalaViewModel` y `ComandaEditorViewModel` sin lógica de UI en el código-behind salvo la necesaria para inicializar.
- Código sin comentarios incrustados; la intención se refleja en nombres de métodos y propiedades.
- Sin dependencias externas nuevas; solo se usan las bibliotecas estándar incluidas en el proyecto.
- Archivos generados mantienen los atributos automáticos pero sin bloques de comentarios.

## Estructura principal
- `ViewModels/`: lógica de negocio y comandos de la aplicación.
- `MainWindow.xaml` y `VentanaSecundaria.xaml`: vistas principales conectadas al mismo `SalaViewModel`.
- `RestaurantSala.Core`: modelos de dominio, datos de demostración y utilidades de estadística.

## Notas de mantenimiento
- Las sesiones se cargan/guardan automáticamente en `%AppData%/RestaurantSala/ultima_sesion.json`.
- Las pruebas manuales pueden iniciarse ejecutando la solución `RestaurantSala.sln` en Visual Studio con .NET Framework 4.7.2.
