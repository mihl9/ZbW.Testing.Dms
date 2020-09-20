using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;
using ZbW.Testing.Dms.Client.Model;
using ZbW.Testing.Dms.Client.Services;

namespace Zbw.Testing.Dms.Client.Test
{
    public class FileHandlerTest
    {
        private readonly AppSettings _appSettings;

        public FileHandlerTest()
        {
            _appSettings = new AppSettings()
            {
                RepositoryPath = @"C:\temp"
            };
        }

        [Fact]
        public async void OpenFileValid()
        {
            var path = Guid.NewGuid() + ".txt";
            var fileHandler = new FileHandler(_appSettings);
            File.WriteAllText(_appSettings.RepositoryPath + @"\" + path, "test");

            var process = await fileHandler.OpenFile(path);

            process.Responding.Should().BeTrue();

            process.Kill();
            File.Delete(path);

        }

        [Fact]
        public void OpenFileNoHandler()
        {
            var path = Guid.NewGuid() + ".tmp";
            var fileHandler = new FileHandler(_appSettings);
            File.WriteAllText(_appSettings.RepositoryPath + @"\" + path, "test");

            Func<Task<Process>> t = async () =>
                await fileHandler.OpenFile(path);


            t.Should().Throw<Win32Exception>();

            File.Delete(path);
        }

        [Fact]
        public async void LoadFirstFileValid()
        {
            var path = Guid.NewGuid() + ".tmp";
            var fileHandler = new FileHandler(_appSettings);
            File.WriteAllText(_appSettings.RepositoryPath + @"\" + path, "test");

            var stream = await fileHandler.LoadFirstFile(path);

            stream.Length.Should().BeGreaterThan(0);
            
            stream.Dispose();

            File.Delete(path);
        }

        [Fact]
        public void LoadFirstFileNotFound()
        {
            var path = Guid.NewGuid() + ".tmp";
            var fileHandler = new FileHandler(_appSettings);

            Func<Task<Stream>> t = async () => 
                await fileHandler.LoadFirstFile(path);

            t.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public async void SaveFileValid()
        {
            var path = Guid.NewGuid() + ".tmp";
            var fileHandler = new FileHandler(_appSettings);

            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                await writer.WriteAsync("test");
                await writer.FlushAsync();

                await fileHandler.SaveFile(stream, path);
                writer.Dispose();
            }

            File.Exists(_appSettings.RepositoryPath + @"\" + path).Should().BeTrue();

            File.Delete(_appSettings.RepositoryPath + @"\" + path);
        }
    }
}
