using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
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

        public MainWindow()
        {

            InitializeComponent();

            _taskService = new TaskService();
            _taskService.RemoveIps();

            _stopwatch = new Stopwatch();
            _timerClass = new Services.Timer();
            TimeDisplay.Text = _timerClass.StartChronometer();
            _timer = new System.Timers.Timer(interval: 1000);
            _timer.Elapsed += OnTimerElapse;

            AppDomain.CurrentDomain.ProcessExit += (s, e) => _taskService.RestoreHosts();
            MessageBox.Show("Si la aplicación tuvo un cierre inesperado o un error, " +
    "abre la aplicación nuevamente y ciérrala para restaurar el estado correcto.",
                "Advertencia",
              MessageBoxButton.OK,
             MessageBoxImage.Warning);
        }


        private void OnTimerElapse(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() => TimeDisplay.Text = _stopwatch.Elapsed.ToString(format: @"hh\:mm\:ss"));
        }

        private void BlockSelectedSocialMedia()
        {
            // Limpiar primero cualquier bloqueo anterior
            _taskService.RemoveIps();

            if (chkTwitter.IsChecked == true)
                _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "x.com" });

            if (chkInstagram.IsChecked == true)
                _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.instagram.com" });

            if (chkFacebook.IsChecked == true)
                _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.facebook.com" });

            if (chkYouTube.IsChecked == true)
                _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.youtube.com" });

            if (chkTikTok.IsChecked == true)
                _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.tiktok.com" });

            if (chkReddit.IsChecked == true)
                _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.reddit.com" });
        }
        private void btnMyButton_Click(object sender, RoutedEventArgs e)
        {
            _taskService.BackupHosts();
            BlockSelectedSocialMedia();
            _stopwatch.Start();
            StateIndicator.Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)); // verde

            StateText.Text = "Ready";
            btnStop.Visibility = Visibility.Visible;
            btnMyButton.Visibility = Visibility.Collapsed;
            _timer.Start();
            //Clear.IsEnabled = false;
            StatusMessage.Text = "Redes bloqueadas. Cierra el navegador para aplicar los cambios.";

            MessageBox.Show("Redes bloqueadas, estudio YA. Recuerda cerrar el navegador para que los bloqueos funcionen.");
        }
        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            StateIndicator.Background = new SolidColorBrush(Color.FromRgb(241, 196, 15)); // amarillo
            StateText.Text = "Waiting";
            _stopwatch.Stop();
            _stopwatch.Reset();
            _timer.Stop();

            // Reiniciar display
            TimeDisplay.Text = @"00:00:00";
            btnStop.Visibility = Visibility.Collapsed;
            btnMyButton.Visibility = Visibility.Visible;

            _taskService.RestoreHosts();

            // Limpiar cualquier IP bloqueada por redes sociales
            _taskService.RemoveIps();
            MessageBox.Show("Sesión detenida, redes restauradas.");
            // Detén tu timer aquí si tienes
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
            _taskService.RemoveIps();
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
