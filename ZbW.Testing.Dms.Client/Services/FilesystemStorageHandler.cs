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
    internal class FilesystemStorageHandler : BaseStorageHandler
    {
        private const string FileSuffix = "_Content";
        

        private readonly string _repoPath;

        public FilesystemStorageHandler(IMetadataService metadataService) : base(metadataService)
        {
            _repoPath = ConfigurationManager.AppSettings[Settings.RepositoryPath] + @"\";
            PathValidity(_repoPath);
        }

        public void PathValidity(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public override async Task<Document> LoadDocument(Guid id)
        {
            var metadata = await _metadataService.LoadMetadata(id);
            var document = new Document()
            {
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

            var path = _repoPath + document.Metadata.Valuta.Year + @"\" + document.Id + FileSuffix + document.Metadata.FileEnding;
            using (var fileStream = File.Create(path))
            {
                await document.File.CopyToAsync(fileStream);
                await fileStream.FlushAsync();
            }

        }

        public override async Task<List<Document>> SearchDocument(string term, string type)
        {
            const string searchPattern = "*" + FileSuffix + ".*";

            var documents = new List<Document>();
            var files = Directory.GetFiles(_repoPath, searchPattern, SearchOption.AllDirectories);

            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                var parts = filename?.Split('_');
                if (!Guid.TryParse(parts?[0], out var id))
                {
                    continue;
                }

                var metadata = await _metadataService.LoadMetadata(id);
                if (metadata.Typ.Equals(type) &&
                    (term == "*" || term == null || (metadata.FileName.Contains(term) || metadata.Keywords.Contains(term))))
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
            var path = _repoPath + document.Metadata.Valuta.Year + @"\" + document.Id + FileSuffix + document.Metadata.FileEnding;
            Process.Start(path);

            return Task.CompletedTask;
        }
    }
}
