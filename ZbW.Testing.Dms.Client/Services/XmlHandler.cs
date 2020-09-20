using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using ZbW.Testing.Dms.Client.Services.Interfaces;

namespace ZbW.Testing.Dms.Client.Services
{
    public class XmlHandler : ISerializationHandler
    {
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
                throw new XmlException($"Couldn't Serialize File to: {path}", e);
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
                throw new XmlException($"Couldn't Serialize Xml", e);
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
                throw new XmlException("Couldn't Deserialize the XML", e);
            }

            return Task.FromResult(file);
        }
    }
}
