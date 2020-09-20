using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZbW.Testing.Dms.Client.Services.Interfaces
{
    public interface IFileHandler
    {
        Task<string[]> SearchFiles(string searchPattern);

        Task<Stream> LoadFirstFile(string searchPattern);

        Task SaveFile(Stream stream, string path);

        Task<Process> OpenFile(string path);
    }
}
