using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZbW.Testing.Dms.Client.Model;

namespace ZbW.Testing.Dms.Client.Services.Interfaces
{
    public interface IStorageService
    {
        Task<Document> LoadDocument(Guid id);

        Task SaveDocument(Document document);

        Task<List<Document>> SearchDocument(string term, string type);

        Task OpenDocumentExternal(Document document);
    }
}
