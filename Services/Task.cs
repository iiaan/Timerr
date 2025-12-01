using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Timerr.Models;

namespace Timerr.Services
{
    public class TaskService
    {
        private readonly string _hostsPath = @"C:\Windows\System32\drivers\etc\hosts";
        private readonly string _backupPath = @"C:\Windows\System32\drivers\etc\hosts.bak";
        private readonly string _tempBackupPath;

        public TaskService()
        {
            try
            {
                // Crear una ruta de backup alternativa en Temp
                _tempBackupPath = Path.Combine(Path.GetTempPath(), "Timerr_hosts_backup.bak");

                // Verificar que el archivo hosts exista
                if (!File.Exists(_hostsPath))
                {
                    throw new FileNotFoundException($"No se encontró el archivo hosts en: {_hostsPath}");
                }

                // Verificar permisos de escritura
                if (!CanWriteToHostsFile())
                {
                    throw new UnauthorizedAccessException("No tiene permisos para modificar el archivo hosts. Ejecute como administrador.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al inicializar TaskService: {ex.Message}", ex);
            }
        }

        public void BackupHosts()
        {
            try
            {
                LogServiceMessage("Iniciando backup de hosts...");

                if (!File.Exists(_hostsPath))
                {
                    throw new FileNotFoundException($"Archivo hosts no encontrado: {_hostsPath}");
                }

                // Intentar backup en la ubicación original
                try
                {
                    File.Copy(_hostsPath, _backupPath, true);
                    LogServiceMessage($"Backup creado en: {_backupPath}");
                }
                catch (Exception ex)
                {
                    LogServiceMessage($"No se pudo crear backup en ubicación original: {ex.Message}");

                    // Intentar backup en ubicación temporal
                    File.Copy(_hostsPath, _tempBackupPath, true);
                    LogServiceMessage($"Backup alternativo creado en: {_tempBackupPath}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al crear backup del archivo hosts: {ex.Message}", ex);
            }
        }

        public void RestoreHosts()
        {
            try
            {
                LogServiceMessage("Iniciando restauración de hosts...");

                // Primero intentar restaurar desde ubicación original
                if (File.Exists(_backupPath))
                {
                    try
                    {
                        File.Copy(_backupPath, _hostsPath, true);
                        File.Delete(_backupPath);
                        LogServiceMessage($"Hosts restaurado desde: {_backupPath}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogServiceMessage($"Error restaurando desde backup original: {ex.Message}");
                    }
                }

                // Intentar restaurar desde backup temporal
                if (File.Exists(_tempBackupPath))
                {
                    try
                    {
                        File.Copy(_tempBackupPath, _hostsPath, true);
                        File.Delete(_tempBackupPath);
                        LogServiceMessage($"Hosts restaurado desde backup temporal: {_tempBackupPath}");
                        return;
                    }
                    catch (Exception ex)
                    {
                        LogServiceMessage($"Error restaurando desde backup temporal: {ex.Message}");
                    }
                }

                LogServiceMessage("No se encontraron backups para restaurar.");

                // Si no hay backups, limpiar manualmente
                RemoveIps();
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al restaurar el archivo hosts: {ex.Message}", ex);
            }
        }

        public void RemoveIps()
        {
            try
            {
                LogServiceMessage("Limpiando IPs de redes sociales...");

                if (!File.Exists(_hostsPath))
                {
                    throw new FileNotFoundException($"Archivo hosts no encontrado: {_hostsPath}");
                }

                // Leer todas las líneas
                string[] allLines;
                try
                {
                    allLines = File.ReadAllLines(_hostsPath);
                }
                catch (IOException ex)
                {
                    throw new Exception($"No se pudo leer el archivo hosts (puede estar en uso): {ex.Message}", ex);
                }

                var lines = allLines.ToList();
                int originalCount = lines.Count;

                // Lista de hosts de redes sociales a eliminar
                var socialHosts = new List<string>
                {
                    "x.com",
                    "x.com",
                    "www.x.com",
                    "instagram.com",
                    "www.instagram.com",
                    "facebook.com",
                    "www.facebook.com",
                    "youtube.com",
                    "www.youtube.com",
                    "tiktok.com",
                    "www.tiktok.com",
                    "reddit.com",
                    "www.reddit.com"
                };

                // Filtrar líneas
                lines.RemoveAll(line =>
                {
                    string trimmedLine = line.Trim();

                    // Conservar líneas vacías o comentarios
                    if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                        return false;

                    // Eliminar líneas que contengan hosts de redes sociales
                    return socialHosts.Any(host =>
                        trimmedLine.IndexOf(host, StringComparison.OrdinalIgnoreCase) >= 0);
                });

                int removedCount = originalCount - lines.Count;

                // Solo escribir si hubo cambios
                if (removedCount > 0)
                {
                    try
                    {
                        File.WriteAllLines(_hostsPath, lines);
                        LogServiceMessage($"Se eliminaron {removedCount} entradas de redes sociales.");
                    }
                    catch (UnauthorizedAccessException)
                    {
                        throw new UnauthorizedAccessException("Permiso denegado al escribir en el archivo hosts. Ejecute como administrador.");
                    }
                    catch (IOException ex)
                    {
                        throw new Exception($"No se pudo escribir en el archivo hosts (puede estar en uso o bloqueado): {ex.Message}", ex);
                    }
                }
                else
                {
                    LogServiceMessage("No se encontraron entradas de redes sociales para eliminar.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al limpiar IPs del archivo hosts: {ex.Message}", ex);
            }
        }

        public void AddSocialMedia(Socialmedia sm)
        {
            try
            {
                if (sm == null)
                    throw new ArgumentNullException(nameof(sm), "El objeto Socialmedia no puede ser nulo.");

                if (string.IsNullOrWhiteSpace(sm.hostname))
                    throw new ArgumentException("El hostname no puede estar vacío.", nameof(sm.hostname));

                string entry = $"{sm.ip?.Trim() ?? "127.0.0.1"} {sm.hostname.Trim()}".Trim();
                LogServiceMessage($"Agregando bloqueo para: {entry}");

                if (!File.Exists(_hostsPath))
                {
                    throw new FileNotFoundException($"Archivo hosts no encontrado: {_hostsPath}");
                }

                // Leer líneas existentes
                string[] existingLines;
                try
                {
                    existingLines = File.ReadAllLines(_hostsPath);
                }
                catch (IOException ex)
                {
                    throw new Exception($"No se pudo leer el archivo hosts: {ex.Message}", ex);
                }

                // Verificar si ya existe
                bool exists = existingLines.Any(line =>
                    line.Trim().Equals(entry, StringComparison.OrdinalIgnoreCase));

                if (exists)
                {
                    LogServiceMessage($"La entrada ya existe: {entry}");
                    return;
                }

                // Agregar la nueva entrada
                try
                {
                    using (StreamWriter sw = new StreamWriter(_hostsPath, true))
                    {
                        sw.WriteLine(entry);
                    }

                    LogServiceMessage($"Entrada agregada exitosamente: {entry}");
                }
                catch (UnauthorizedAccessException)
                {
                    throw new UnauthorizedAccessException("Permiso denegado al escribir en el archivo hosts.");
                }
                catch (IOException ex)
                {
                    throw new Exception($"No se pudo escribir en el archivo hosts: {ex.Message}", ex);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al agregar red social al archivo hosts: {ex.Message}", ex);
            }
        }

        #region Métodos auxiliares privados

        private bool CanWriteToHostsFile()
        {
            try
            {
                // Intentar abrir el archivo en modo escritura
                using (FileStream fs = File.Open(_hostsPath, FileMode.Open, FileAccess.ReadWrite))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        private void LogServiceMessage(string message)
        {
            string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
            Debug.WriteLine($"[TaskService {timestamp}] {message}");
        }

        #endregion
    }
}