using YellowCar.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace YellowCar
{
    public class GenericStorage
    {
        private string _filePath;

        public GenericStorage()
        {
            var webAppsHome = Environment.GetEnvironmentVariable("HOME")?.ToString();
            if (String.IsNullOrEmpty(webAppsHome))
            {
                _filePath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath) + "\\";
            }
            else
            {
                _filePath = webAppsHome + "\\site\\wwwroot\\";
            }
        }

        public async Task<int> Save(Byte[] blob)
        {
            int numberOfFiles = Directory.GetFiles(_filePath).Length;
            int id = numberOfFiles + 1;
            File.WriteAllBytes(_filePath + id, blob);
            return id;
        }

        public async Task<Byte[]> Get(int id)
        {
            byte[] imageBytes;
            if (File.Exists(_filePath + id))
            {
                imageBytes = File.ReadAllBytes(_filePath + id);
                return imageBytes;
            }
            else
            {
                return null;
            }
        }
    }
}
