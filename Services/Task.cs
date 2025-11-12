using System.IO;
using System.Linq;
using Timerr.Models;
namespace Timerr.Services
{
    public class TaskService
    {
        private Socialmedia _socialmedia;
        string path = @"C:\Windows\System32\drivers\etc\hosts";
        string pathBak = @"C:\Windows\System32\drivers\etc\hosts.bak";

        public void BackupHosts()
        {
            File.Copy(path, pathBak, true); // Sobrescribe si ya existe
        }

        public void RestoreHosts()
        {
            if (File.Exists(pathBak))
            {
                File.Copy(pathBak, path, true);
                File.Delete(pathBak);
            }
        }
        public void AddSocialMedia(Socialmedia sm)
        {
            string entry = $"{sm.ip} {sm.hostname}";

            var lines = File.ReadAllLines(path);


            bool exists = lines.Any(line =>
                line.Contains(sm.hostname) ||
                line.Contains($"{sm.ip} "));
            if (exists)
            {
                return;
            }
            using (StreamWriter sw = new StreamWriter(path, true))
            {
                sw.WriteLine(entry);

            }


        }


    }
}
