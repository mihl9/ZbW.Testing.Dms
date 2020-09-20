using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.Serialization;
using ZbW.Testing.Dms.Client.Model;
using ZbW.Testing.Dms.Client.Repositories;
using ZbW.Testing.Dms.Client.Services.Interfaces;

namespace ZbW.Testing.Dms.Client.Services
{
    public class MetadataService : IMetadataService
    {
        private const string FileSuffix = "_Metadata";
        private const string FileEnding = ".xml";

        private readonly AppSettings _appSettings;
        private readonly IFileHandler _fileHandler;
        private readonly ISerializationHandler _serializationHandler;

        public MetadataService(IFileHandler fileHandler, ISerializationHandler serializationHandler, AppSettings appSettings)
        {
            _fileHandler = fileHandler;
            _serializationHandler = serializationHandler;
            _appSettings = appSettings;
        }

        public async Task<MetadataItem> LoadMetadata(Guid id)
        {
            if(id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));
            var searchString = id + FileSuffix + FileEnding;
            var stream = await _fileHandler.LoadFirstFile(searchString);
            var result = await _serializationHandler.Deserialize<MetadataItem>(stream);

            return result;
        }

        public async Task SaveMetadata(MetadataItem metadata, Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentNullException(nameof(id));
            if (metadata == null)
                throw new ArgumentNullException(nameof(metadata));

            var savePath = _appSettings.RepositoryPath + @"\" + metadata.Valuta.Year + @"\";
            FileHandler.PathValidity(savePath);

            savePath += id + FileSuffix + FileEnding;

            await _serializationHandler.Serialize(metadata, savePath);
        }

    }
}
