using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Xunit;
using ZbW.Testing.Dms.Client.Model;
using ZbW.Testing.Dms.Client.Services;

namespace Zbw.Testing.Dms.Client.Test
{
    public class XmlHandlerTest
    {
        private const string Xml =
            @"<?xml version=""1.0""?><MetadataItem xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema""><FileName>test</FileName><Valuta>{0}</Valuta><FileEnding>.test</FileEnding><Typ>test</Typ><Keywords>test</Keywords><Username>test</Username><CreationTime>{0}</CreationTime></MetadataItem>";
        private readonly MetadataItem _metadataMock;

        public XmlHandlerTest()
        {
            _metadataMock = new MetadataItem()
            {
                FileEnding = ".test",
                CreationTime = DateTime.Now,
                FileName = "test",
                Keywords = "test",
                Typ = "test",
                Username = "test",
                Valuta = DateTime.Now
            };
        }
        [Fact]
        public async void SerializeValid()
        {
            var xmlHandler = new XmlHandler();
            var path = Path.GetTempFileName();

            await xmlHandler.Serialize(_metadataMock, path);

            File.Exists(path).Should().BeTrue();

            File.Delete(path);
        }

        [Fact]
        public async void SerializeInvalid()
        {
            var xmlHandler = new XmlHandler();
            var path = "1:";

            Func<Task> t = async () =>
                await xmlHandler.Serialize(_metadataMock, path);

            t.Should().Throw<XmlException>();
        }

        [Fact]
        public async void DeserializeValid()
        {
            var xmlHandler = new XmlHandler();

            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                var xml = string.Format(Xml, _metadataMock.CreationTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff"));
                await writer.WriteAsync(xml);
                await writer.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);
                var metadata = await xmlHandler.Deserialize<MetadataItem>(stream);

                metadata.Should().BeEquivalentTo(_metadataMock);
            }
        }

        [Fact]
        public async void DeserializeInvalid()
        {
            var xmlHandler = new XmlHandler();

            using (var stream = new MemoryStream())
            {
                var writer = new StreamWriter(stream);
                var xml = string.Format(Xml, _metadataMock.CreationTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fffffff"));
                await writer.WriteAsync(string.Empty);
                await writer.FlushAsync();

                stream.Seek(0, SeekOrigin.Begin);
                Func<Task<MetadataItem>> t = async () =>
                    await xmlHandler.Deserialize<MetadataItem>(stream);
                t.Should().Throw<XmlException>();
            }
        }
    }
}
