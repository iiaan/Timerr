using System;
using System.Collections.Generic;
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

                File.Delete(pathBak);
            }

        }

        public void RemoveIps()
        {

            var lines = File.ReadAllLines(path).ToList();

            var socialHosts = new List<string>
    {
        "x.com",
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
            lines.RemoveAll(line =>
        socialHosts.Any(h => line.Contains(h))
    );

            File.WriteAllLines(path, lines);
        }
        public void AddSocialMedia(Socialmedia sm)
        {
            string entry = $"{sm.ip} {sm.hostname}".Trim();
            var lines = File.ReadAllLines(path)
                            .Select(l => l.Trim())
                            .ToList();

            // Comparación exacta, no contains
            bool exists = lines.Any(line =>
                line.Equals(entry, StringComparison.OrdinalIgnoreCase)
            );

            if (exists)
                return;

            using (StreamWriter sw = new StreamWriter(path, true))
                sw.WriteLine(entry);

        }


    }
}
