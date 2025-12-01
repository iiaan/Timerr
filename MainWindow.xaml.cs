using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
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
        private bool _initializedSuccessfully = false;

        public MainWindow()
        {
            try
            {
                LogMessage("Iniciando aplicación...");

                InitializeComponent();
                LogMessage("Componentes WPF inicializados.");

                // Verificar permisos de administrador
                if (!HasAdminPermissions())
                {
                    ShowErrorMessage("Permisos requeridos",
                        "Esta aplicación requiere permisos de administrador para modificar el archivo hosts.\n\n" +
                        "Por favor, ejecute la aplicación como administrador (clic derecho → Ejecutar como administrador).");
                    Application.Current.Shutdown();
                    return;
                }
                LogMessage("Permisos de administrador verificados.");

                try
                {
                    _taskService = new TaskService();
                    LogMessage("TaskService creado.");
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Error de inicialización",
                        $"No se pudo crear el servicio de tareas: {ex.Message}");
                    Application.Current.Shutdown();
                    return;
                }

                // Intentar operación inicial de limpieza
                try
                {
                    _taskService.RemoveIps();
                    LogMessage("IPs limpiadas correctamente.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Advertencia: No se pudieron limpiar las IPs inicialmente: {ex.Message}");
                    MessageBox.Show($"Advertencia: No se pudieron limpiar las IPs inicialmente. " +
                                  $"La aplicación continuará pero puede que algunos bloqueos previos persistan.\n\n" +
                                  $"Error: {ex.Message}",
                                  "Advertencia de inicialización",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }

                try
                {
                    _stopwatch = new Stopwatch();
                    _timerClass = new Services.Timer();
                    TimeDisplay.Text = _timerClass.StartChronometer();

                    _timer = new System.Timers.Timer(interval: 1000);
                    _timer.Elapsed += OnTimerElapse;
                    _timer.AutoReset = true;
                    _timer.Enabled = false;
                    LogMessage("Timer y Stopwatch inicializados.");
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Error de temporizador",
                        $"No se pudo inicializar el sistema de temporización: {ex.Message}");
                    Application.Current.Shutdown();
                    return;
                }

                // Registrar evento de cierre
                AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
                LogMessage("Evento de cierre de proceso registrado.");

                // Mostrar mensaje informativo
                MessageBox.Show("Si la aplicación tuvo un cierre inesperado o un error, " +
                    "abre la aplicación nuevamente y ciérrala para restaurar el estado correcto.\n\n" +
                    "Asegúrate de tener permisos de administrador siempre que ejecutes esta aplicación.",
                    "Información importante",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                _initializedSuccessfully = true;
                LogMessage("Aplicación inicializada exitosamente.");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR CRÍTICO EN CONSTRUCTOR: {ex.Message}\n{ex.StackTrace}");
                ShowErrorMessage("Error crítico al iniciar",
                    $"No se pudo inicializar la aplicación:\n\n{ex.Message}\n\n" +
                    $"Verifique los permisos y que el archivo hosts no esté bloqueado por otro programa.");
                Application.Current.Shutdown();
            }
        }

        private void OnTimerElapse(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Application.Current != null && Application.Current.Dispatcher != null)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            TimeDisplay.Text = _stopwatch.Elapsed.ToString(@"hh\:mm\:ss");
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error al actualizar tiempo: {ex.Message}");
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error en evento de timer: {ex.Message}");
            }
        }

        private void BlockSelectedSocialMedia()
        {
            try
            {
                LogMessage("Iniciando bloqueo de redes sociales...");

                // Limpiar primero cualquier bloqueo anterior
                try
                {
                    _taskService.RemoveIps();
                    LogMessage("IPs anteriores limpiadas.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error limpiando IPs anteriores: {ex.Message}");
                    throw new Exception($"No se pudieron limpiar los bloqueos anteriores: {ex.Message}", ex);
                }

                // Lista de redes a bloquear
                var socialNetworks = new System.Collections.Generic.List<string>();

                if (chkTwitter.IsChecked == true)
                {
                    try
                    {
                        _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "x.com" });
                        socialNetworks.Add("Twitter/X");
                        LogMessage("Twitter/X bloqueado.");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error bloqueando Twitter/X: {ex.Message}");
                    }
                }

                if (chkInstagram.IsChecked == true)
                {
                    try
                    {
                        _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.instagram.com" });
                        socialNetworks.Add("Instagram");
                        LogMessage("Instagram bloqueado.");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error bloqueando Instagram: {ex.Message}");
                    }
                }

                if (chkFacebook.IsChecked == true)
                {
                    try
                    {
                        _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.facebook.com" });
                        socialNetworks.Add("Facebook");
                        LogMessage("Facebook bloqueado.");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error bloqueando Facebook: {ex.Message}");
                    }
                }

                if (chkYouTube.IsChecked == true)
                {
                    try
                    {
                        _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.youtube.com" });
                        socialNetworks.Add("YouTube");
                        LogMessage("YouTube bloqueado.");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error bloqueando YouTube: {ex.Message}");
                    }
                }

                if (chkTikTok.IsChecked == true)
                {
                    try
                    {
                        _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.tiktok.com" });
                        socialNetworks.Add("TikTok");
                        LogMessage("TikTok bloqueado.");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error bloqueando TikTok: {ex.Message}");
                    }
                }

                if (chkReddit.IsChecked == true)
                {
                    try
                    {
                        _taskService.AddSocialMedia(new Socialmedia { ip = "127.0.0.1", hostname = "www.reddit.com" });
                        socialNetworks.Add("Reddit");
                        LogMessage("Reddit bloqueado.");
                    }
                    catch (Exception ex)
                    {
                        LogMessage($"Error bloqueando Reddit: {ex.Message}");
                    }
                }

                if (socialNetworks.Count == 0)
                {
                    LogMessage("No se seleccionaron redes sociales para bloquear.");
                    throw new Exception("No se seleccionó ninguna red social para bloquear.");
                }

                LogMessage($"Bloqueo completado para: {string.Join(", ", socialNetworks)}");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR EN BlockSelectedSocialMedia: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
        }

        private void btnMyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!_initializedSuccessfully)
                {
                    ShowErrorMessage("Aplicación no inicializada",
                        "La aplicación no se inicializó correctamente. Por favor, cierre y vuelva a abrir.");
                    return;
                }

                LogMessage("Iniciando bloqueo...");

                // Realizar backup del archivo hosts
                try
                {
                    _taskService.BackupHosts();
                    LogMessage("Backup de hosts realizado.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error haciendo backup: {ex.Message}");
                    MessageBox.Show($"Advertencia: No se pudo hacer backup del archivo hosts. " +
                                  $"El bloqueo continuará pero la restauración automática podría no funcionar.\n\n" +
                                  $"Error: {ex.Message}",
                                  "Advertencia",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }

                // Bloquear redes seleccionadas
                try
                {
                    BlockSelectedSocialMedia();
                }
                catch (Exception ex)
                {
                    ShowErrorMessage("Error al bloquear",
                        $"No se pudieron bloquear las redes sociales:\n\n{ex.Message}");
                    return;
                }

                // Iniciar temporizadores
                try
                {
                    _stopwatch.Start();
                    _timer.Start();
                    LogMessage("Temporizadores iniciados.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error iniciando temporizadores: {ex.Message}");
                    ShowErrorMessage("Error de temporización",
                        $"No se pudieron iniciar los temporizadores: {ex.Message}");
                    return;
                }

                // Actualizar interfaz
                try
                {
                    StateIndicator.Background = new SolidColorBrush(Color.FromRgb(46, 204, 113)); // verde
                    StateText.Text = "Ready";
                    btnStop.Visibility = Visibility.Visible;
                    btnMyButton.Visibility = Visibility.Collapsed;
                    StatusMessage.Text = "Redes bloqueadas. Cierra el navegador para aplicar los cambios.";
                    LogMessage("Interfaz actualizada a estado de bloqueo.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error actualizando interfaz: {ex.Message}");
                }

                MessageBox.Show("Redes bloqueadas, estudio YA.\n\n" +
                              "IMPORTANTE:\n" +
                              "1. Cierra todos los navegadores para que los cambios surtan efecto.\n" +
                              "2. Si las páginas siguen cargando, vacía la caché del DNS (ipconfig /flushdns en CMD).",
                              "Bloqueo activado",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                LogMessage("Bloqueo iniciado exitosamente.");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR CRÍTICO EN btnMyButton_Click: {ex.Message}\n{ex.StackTrace}");
                ShowErrorMessage("Error inesperado",
                    $"Ocurrió un error inesperado al iniciar el bloqueo:\n\n{ex.Message}");
            }
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                LogMessage("Deteniendo bloqueo...");

                // Detener temporizadores
                try
                {
                    _stopwatch.Stop();
                    _stopwatch.Reset();
                    _timer.Stop();
                    LogMessage("Temporizadores detenidos.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error deteniendo temporizadores: {ex.Message}");
                }

                // Restaurar hosts
                try
                {
                    _taskService.RestoreHosts();
                    LogMessage("Hosts restaurados.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error restaurando hosts: {ex.Message}");
                    MessageBox.Show($"Advertencia: No se pudo restaurar completamente el archivo hosts.\n\n" +
                                  $"Puede que necesite restaurarlo manualmente desde:\n" +
                                  $@"C:\Windows\System32\drivers\etc\hosts.bak\n\n" +
                                  $"Error: {ex.Message}",
                                  "Advertencia de restauración",
                                  MessageBoxButton.OK,
                                  MessageBoxImage.Warning);
                }

                // Limpiar IPs adicionales
                try
                {
                    _taskService.RemoveIps();
                    LogMessage("IPs limpiadas.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error limpiando IPs: {ex.Message}");
                }

                // Actualizar interfaz
                try
                {
                    StateIndicator.Background = new SolidColorBrush(Color.FromRgb(241, 196, 15)); // amarillo
                    StateText.Text = "Waiting";
                    StatusMessage.Text = "Bloqueo detenido. ¡Ahora puedes usar las redes otra vez!";
                    TimeDisplay.Text = @"00:00:00";
                    btnStop.Visibility = Visibility.Collapsed;
                    btnMyButton.Visibility = Visibility.Visible;
                    LogMessage("Interfaz actualizada a estado de espera.");
                }
                catch (Exception ex)
                {
                    LogMessage($"Error actualizando interfaz: {ex.Message}");
                }

                MessageBox.Show("Sesión detenida, redes restauradas.\n\n" +
                              "IMPORTANTE:\n" +
                              "1. Es posible que necesites cerrar y reabrir el navegador.\n" +
                              "2. Si las páginas no cargan, vacía la caché del DNS (ipconfig /flushdns).",
                              "Bloqueo desactivado",
                              MessageBoxButton.OK,
                              MessageBoxImage.Information);

                LogMessage("Bloqueo detenido exitosamente.");
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR CRÍTICO EN btnStop_Click: {ex.Message}\n{ex.StackTrace}");
                ShowErrorMessage("Error al detener",
                    $"Ocurrió un error al intentar detener el bloqueo:\n\n{ex.Message}");
            }
        }

        private async void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                LogMessage("Iniciando cierre de aplicación...");

                // Cancelar el cierre temporalmente
                e.Cancel = true;

                // Crear ventana de estado
                var statusWindow = new Window
                {
                    Width = 350,
                    Height = 120,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen,
                    WindowStyle = WindowStyle.None,
                    ResizeMode = ResizeMode.NoResize,
                    Title = "Cerrando aplicación"
                };

                var stackPanel = new StackPanel
                {
                    Margin = new Thickness(20),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var textBlock = new TextBlock
                {
                    Text = "Restaurando configuración...",
                    FontSize = 14,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    TextWrapping = TextWrapping.Wrap
                };

                var progressBar = new ProgressBar
                {
                    Height = 20,
                    Width = 250,
                    IsIndeterminate = true,
                    Margin = new Thickness(0, 10, 0, 0)
                };

                stackPanel.Children.Add(textBlock);
                stackPanel.Children.Add(progressBar);
                statusWindow.Content = stackPanel;
                statusWindow.Show();

                try
                {
                    // Restaurar hosts
                    await Task.Run(() =>
                    {
                        try
                        {
                            textBlock.Dispatcher.Invoke(() => textBlock.Text = "Restaurando archivo hosts...");
                            _taskService.RestoreHosts();
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error restaurando hosts al cerrar: {ex.Message}");
                            textBlock.Dispatcher.Invoke(() =>
                                textBlock.Text = $"Error restaurando hosts: {ex.Message}");
                        }
                    });

                    await Task.Delay(500);

                    // Limpiar IPs
                    await Task.Run(() =>
                    {
                        try
                        {
                            textBlock.Dispatcher.Invoke(() => textBlock.Text = "Limpiando bloqueos...");
                            _taskService.RemoveIps();
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error limpiando IPs al cerrar: {ex.Message}");
                        }
                    });

                    textBlock.Text = "Configuración restaurada ✓";
                    progressBar.IsIndeterminate = false;
                    progressBar.Value = 100;

                    await Task.Delay(1000);
                }
                catch (Exception ex)
                {
                    LogMessage($"ERROR DURANTE CIERRE: {ex.Message}");
                    textBlock.Text = $"Error durante el cierre: {ex.Message}";
                    textBlock.Foreground = Brushes.Red;
                    await Task.Delay(2000);
                }
                finally
                {
                    statusWindow.Close();
                }

                LogMessage("Aplicación cerrada exitosamente.");

                // Guardar log final
                SaveLogToFile();

                // Ahora sí cerrar la aplicación
                Application.Current.Shutdown();
            }
            catch (Exception ex)
            {
                LogMessage($"ERROR CRÍTICO EN Window_Closing: {ex.Message}\n{ex.StackTrace}");

                // Forzar cierre si hay error
                Application.Current.Shutdown();
            }
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            try
            {
                LogMessage("Proceso terminando, restaurando configuración...");

                // Asegurarse de restaurar hosts al terminar
                if (_taskService != null)
                {
                    _taskService.RestoreHosts();
                    _taskService.RemoveIps();
                }

                LogMessage("Proceso terminado.");
                SaveLogToFile();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en OnProcessExit: {ex.Message}");
            }
        }

        #region Métodos auxiliares

        private bool HasAdminPermissions()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch
            {
                return false;
            }
        }

        private void ShowErrorMessage(string title, string message)
        {
            try
            {
                MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
                LogMessage($"ERROR: {title} - {message}");
            }
            catch
            {
                // Si falla MessageBox, al menos loguear
                LogMessage($"ERROR (sin MessageBox): {title} - {message}");
            }
        }

        private static readonly object _logLock = new object();
        private static List<string> _logMessages = new List<string>();

        private void LogMessage(string message)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string logMessage = $"[{timestamp}] {message}";

                lock (_logLock)
                {
                    _logMessages.Add(logMessage);
                }

                // También escribir en Debug para Visual Studio
                Debug.WriteLine(logMessage);
            }
            catch
            {
                // Ignorar errores de logging
            }
        }

        private void SaveLogToFile()
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                    $"Timerr_Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                lock (_logLock)
                {
                    if (_logMessages.Count > 0)
                    {
                        File.WriteAllLines(logPath, _logMessages);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error guardando log: {ex.Message}");
            }
        }

        #endregion
    }
}