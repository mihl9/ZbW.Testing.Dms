using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZbW.Testing.Dms.Client.Model;
using ZbW.Testing.Dms.Client.Repositories;
using ZbW.Testing.Dms.Client.Services.Interfaces;

namespace ZbW.Testing.Dms.Client.Services
{
    public class FileHandler : IFileHandler
    {
        private readonly AppSettings _appSettings;

        public FileHandler(AppSettings appSettings)
        {
            _appSettings = appSettings;
        }

        public Task<string[]> SearchFiles(string searchPattern)
        {
            var files = Directory.GetFiles(_appSettings.RepositoryPath, searchPattern, SearchOption.AllDirectories);
            if (files.Length <= 0)
                throw new FileNotFoundException($"File with the Name {searchPattern} could not be found.");

            return Task.FromResult(files);
        }

        public async Task<Stream> LoadFirstFile(string searchPattern)
        {
            var files = await SearchFiles(searchPattern);

            var stream = File.Open(files.First(), FileMode.Open);

            return stream;
        }

        public async Task SaveFile(Stream stream, string path)
        {
            var sPath = _appSettings.RepositoryPath + @"\" + path;
            using (var fileStream = File.Create(sPath))
            {
                await stream.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }
        }

        public Task<Process> OpenFile(string path)
        {
            return Task.FromResult(Process.Start(_appSettings.RepositoryPath + @"\" + path)); 
        }

        public static void PathValidity(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
    }
}
