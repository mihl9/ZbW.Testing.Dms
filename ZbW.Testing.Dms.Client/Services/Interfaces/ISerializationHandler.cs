using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZbW.Testing.Dms.Client.Services.Interfaces
{
    public interface ISerializationHandler
    {
        Task Serialize<TT>(TT obj, string path);

        Task<TT> Deserialize<TT>(Stream xml);
    }
}
