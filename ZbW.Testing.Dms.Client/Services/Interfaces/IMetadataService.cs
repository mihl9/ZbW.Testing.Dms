using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZbW.Testing.Dms.Client.Model;

namespace ZbW.Testing.Dms.Client.Services.Interfaces
{
    internal interface IMetadataService
    {

        Task<MetadataItem> LoadMetadata(Guid id);

        Task SaveMetadata(MetadataItem metadata, Guid id);
    }
}
