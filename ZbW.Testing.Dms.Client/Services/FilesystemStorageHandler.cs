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
    public class FilesystemStorageHandler : BaseStorageHandler
    {
        private const string FileSuffix = "_Content";

        private readonly IFileHandler _fileHandler;
        private readonly AppSettings _appSettings;
        
        public FilesystemStorageHandler(IMetadataService metadataService, AppSettings appSettings, IFileHandler fileHandler) : base(metadataService)
        {
            _appSettings = appSettings;
            _fileHandler = fileHandler;

            FileHandler.PathValidity(_appSettings.RepositoryPath);
        }

        public override async Task<Document> LoadDocument(Guid id)
        {
            var metadata = await _metadataService.LoadMetadata(id);
            var document = new Document()
            {
                Id = id,
                Metadata = metadata
            };

            return document;
        }

        public override async Task SaveDocument(Document document)
        {
            if(document.Id == Guid.Empty)
                document.Id = Guid.NewGuid();

            if(document.File == null)
                throw new FileNotFoundException("No File to be saved was found!");

            await _metadataService.SaveMetadata(document.Metadata, document.Id);

            await _fileHandler.SaveFile(document.File,
                document.Metadata.Valuta.Year + @"\" + document.Id + FileSuffix + document.Metadata.FileEnding);
        }

        public override async Task<List<Document>> SearchDocument(string term, string type)
        {
            const string searchPattern = "*" + FileSuffix + ".*";

            var documents = new List<Document>();
            var files = await _fileHandler.SearchFiles(searchPattern);

            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var parts = filename?.Split('_');
                if (!Guid.TryParse(parts?[0], out var id))
                {
                    continue;
                }

                var metadata = await _metadataService.LoadMetadata(id);
                if ((string.IsNullOrEmpty(type) || metadata.Typ.Equals(type) )&&
                    (term == "*" || string.IsNullOrEmpty(term) || (metadata.FileName.Contains(term) || metadata.Keywords.Contains(term))))
                {
                    documents.Add(new Document()
                    {
                        Id = id,
                        Metadata = metadata
                    });
                }
            }

            return documents;
        }

        public override Task OpenDocumentExternal(Document document)
        {
            var path = document.Metadata.Valuta.Year + @"\" + document.Id + FileSuffix + document.Metadata.FileEnding;

            _fileHandler.OpenFile(path);

            return Task.CompletedTask;
        }
    }
}
