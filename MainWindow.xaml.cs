using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using Timerr.Models;
using Timerr.Services;
namespace Timerr
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TaskService _taskService;
        private System.Timers.Timer _timer;
        private Services.Timer _timerClass;
        private Stopwatch _stopwatch;
        private readonly Socialmedia twitter = new Socialmedia
        {
            hostname = "pepehost",
            ip = "123.123.123.123"
        };
        public MainWindow()
        {

            InitializeComponent();
            _taskService = new TaskService();
            _taskService.BackupHosts();
            _stopwatch = new Stopwatch();
            TimeDisplay.Text = _timerClass.StartChronometer();
            _timer = new System.Timers.Timer(interval: 1000);
            // _timer.Elapsed += OnTim
            Application.Current.Exit += (s, e) => _taskService.RestoreHosts();
        }


        private void OnTimerElapse(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => TimeDisplay.Text = _stopwatch.Elapsed.ToString(format: @"hh\:mm\:ss"));
        }


        private void btnMyButton_Click(object sender, RoutedEventArgs e)
        {

            _taskService.AddSocialMedia(twitter);
            _stopwatch.Start();
            _timer.Start();
            //Clear.IsEnabled = false;
            MessageBox.Show("Redes bloqueadas, estudio YA");
        }



        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Cancelar el cierre temporalmente
            e.Cancel = true;

            // Crear ventana de estado
            var statusWindow = new Window
            {
                Width = 300,
                Height = 100,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                Content = new TextBlock
                {
                    Text = "Restaurando hosts...",
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };

            statusWindow.Show();

            // Esperar un momento para que se vea
            await Task.Delay(1000);

            // Restaurar hosts
            _taskService.RestoreHosts();

            // Cambiar mensaje
            ((TextBlock)statusWindow.Content).Text = "Hosts restaurado ✓";
            await Task.Delay(1000);

            // Cerrar ventana de estado
            statusWindow.Close();

            // Ahora sí cerrar la aplicación
            Application.Current.Shutdown();
        }

    }
}
