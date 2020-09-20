using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZbW.Testing.Dms.Client.Model;
using ZbW.Testing.Dms.Client.Services.Interfaces;

namespace ZbW.Testing.Dms.Client.Services
{
    public abstract class BaseStorageHandler : IStorageService
    {
        protected readonly IMetadataService _metadataService;

        protected BaseStorageHandler(IMetadataService metadataService)
        {
            _metadataService = metadataService;
        }


        public abstract Task<Document> LoadDocument(Guid id);
        public abstract Task SaveDocument(Document document);
        public abstract Task<List<Document>> SearchDocument(string term, string type);
        public abstract Task OpenDocumentExternal(Document document);
    }
}
