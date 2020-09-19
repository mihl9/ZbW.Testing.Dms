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
    internal class MetadataService : IMetadataService
    {
        private const string FileSuffix = "_Metadata";
        private const string FileEnding = ".xml";

        private readonly string _repoPath;

        public MetadataService()
        {
            _repoPath = ConfigurationManager.AppSettings[Settings.RepositoryPath] + @"\";
        }

        public async Task<MetadataItem> LoadMetadata(Guid id)
        {
            var searchString = id + FileSuffix + FileEnding;
            var files = Directory.GetFiles(_repoPath, searchString, SearchOption.AllDirectories);
            if(files.Length <= 0)
                throw new FileNotFoundException($"File with the ID {id} could not be found.");
            var stream = File.Open(files.First(), FileMode.Open);
            var result = await Deserialize<MetadataItem>(stream);

            return result;
        }

        public async Task SaveMetadata(MetadataItem metadata, Guid id)
        {
            var savePath = _repoPath + metadata.Valuta.Year + @"\";
            PathValidity(savePath);

            savePath += id + FileSuffix + FileEnding;

            await Serialize(metadata, savePath);
        }

        public void PathValidity(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        public async Task Serialize<TT>(TT obj, string path)
        {
            try
            {
                //serialize the object and get the xml string
                var xml = await Serialize<TT>(obj);
                //write it to file
                File.WriteAllText(path, xml);
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't Serialize File to: {path}", e);
            }
        }

        protected async Task<string> Serialize<TT>(TT obj)
        {
            try
            {
                //prepare the serializer
                var serializer = new XmlSerializer(typeof(TT));
                using (var stream = new MemoryStream())
                {
                    //serialize the object into the Memory stream
                    serializer.Serialize(stream, obj);
                    stream.Position = 0;
                    //get the xml from the Stream
                    using (var reader = new StreamReader(stream))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
                
            }
            catch (Exception e)
            {
                throw new Exception($"Couldn't Serialize Xml", e);
            }
        }

        public Task<TT> Deserialize<TT>(Stream xml)
        {
            TT file;
            try
            {
                using (var reader = new StreamReader(xml))
                {
                    //parse the xml
                    file = (TT)new XmlSerializer(typeof(TT)).Deserialize(reader);
                }
                
            }
            catch (Exception e)
            {
                throw new Exception("Couldn't Deserialize the XML", e);
            }

            return Task.FromResult(file);
        }

	}
}
