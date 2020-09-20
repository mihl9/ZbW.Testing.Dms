using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using FluentAssertions;
using Moq;
using Xunit;
using ZbW.Testing.Dms.Client.Model;
using ZbW.Testing.Dms.Client.Repositories;
using ZbW.Testing.Dms.Client.Services;
using ZbW.Testing.Dms.Client.Services.Interfaces;


namespace Zbw.Testing.Dms.Client.Test
{
    public class MetadataServiceTest
    {
        private const string FileSuffix = "_Metadata.xml";

        private readonly MetadataItem _metadataMock;
        private readonly AppSettings _appSettings;
        
        public MetadataServiceTest()
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

            _appSettings = new AppSettings()
            {
                RepositoryPath = @"C:\temp"
            };
        }

        [Fact]
        public async void LoadMetadataValid()
        {
            var id = Guid.NewGuid();
            Stream stream = new MemoryStream();
            var fileService = new Mock<IFileHandler>();
            fileService.Setup(service => service.LoadFirstFile(It.IsAny<string>())).Returns(Task.FromResult(stream));
            var serializationService = new Mock<ISerializationHandler>();
            serializationService.Setup(service => service.Deserialize<MetadataItem>(stream)).Returns(Task.FromResult(_metadataMock));
            var metadataService = new MetadataService(fileService.Object, serializationService.Object, _appSettings);

            var result = await metadataService.LoadMetadata(id);

            result.Should().Be(_metadataMock);
            fileService.Verify(x => x.LoadFirstFile(id + FileSuffix));
        }

        [Fact]
        public void LoadMetadataInvalid()
        {
            var id = Guid.NewGuid();
            Stream stream = new MemoryStream();
            var fileService = new Mock<IFileHandler>();
            fileService.Setup(service => service.LoadFirstFile(It.IsAny<string>())).Throws<FileNotFoundException>();
            var serializationService = new Mock<ISerializationHandler>();
            serializationService.Setup(service => service.Deserialize<MetadataItem>(stream)).Returns(Task.FromResult(_metadataMock));
            var metadataService = new MetadataService(fileService.Object, serializationService.Object, _appSettings);

            Func<Task<MetadataItem>> t = async () =>
                await metadataService.LoadMetadata(id);

            t.Should().Throw<FileNotFoundException>();
        }

        [Fact]
        public void LoadMetadataMissingId()
        {
            var id = Guid.Empty;
            Stream stream = new MemoryStream();
            var fileService = new Mock<IFileHandler>();
            fileService.Setup(service => service.LoadFirstFile(It.IsAny<string>())).Returns(Task.FromResult(stream));
            var serializationService = new Mock<ISerializationHandler>();
            serializationService.Setup(service => service.Deserialize<MetadataItem>(stream)).Returns(Task.FromResult(_metadataMock));
            var metadataService = new MetadataService(fileService.Object, serializationService.Object, _appSettings);

            Func<Task<MetadataItem>> t = async () =>
                await metadataService.LoadMetadata(id);

            t.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async void SaveMetadataValid()
        {
            var id = Guid.NewGuid();
            var expectedPath = _appSettings.RepositoryPath + @"\" + _metadataMock.Valuta.Year + @"\" + id + FileSuffix;
            var fileService = new Mock<IFileHandler>();
            var serializationService = new Mock<ISerializationHandler>();
            serializationService.Setup(service => service.Serialize(_metadataMock, It.IsAny<string>()));
            var metadataService = new MetadataService(fileService.Object, serializationService.Object, _appSettings);

            await metadataService.SaveMetadata(_metadataMock, id);

            serializationService.Verify(x => x.Serialize(_metadataMock, expectedPath));
        }

        [Fact]
        public async void SaveMetadataException()
        {
            var id = Guid.NewGuid();
            var expectedPath = _appSettings.RepositoryPath + @"\" + _metadataMock.Valuta.Year + @"\" + id + FileSuffix;
            var fileService = new Mock<IFileHandler>();
            var serializationService = new Mock<ISerializationHandler>();
            serializationService.Setup(service => service.Serialize(_metadataMock, It.IsAny<string>())).Throws<XmlException>();
            var metadataService = new MetadataService(fileService.Object, serializationService.Object, _appSettings);

            Func<Task> t = async () =>
                await metadataService.SaveMetadata(_metadataMock, id);

            t.Should().Throw<XmlException>();
        }

        [Fact]
        public void SaveMetadataMissingId()
        {
            var id = Guid.Empty;
            var expectedPath = _appSettings.RepositoryPath + @"\" + _metadataMock.Valuta.Year + @"\" + id + FileSuffix;
            var fileService = new Mock<IFileHandler>();
            var serializationService = new Mock<ISerializationHandler>();
            var metadataService = new MetadataService(fileService.Object, serializationService.Object, _appSettings);

            Func<Task> t = async () =>
                await metadataService.SaveMetadata(_metadataMock, id);

            t.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void SaveMetadataMissingMetadata()
        {
            var id = Guid.NewGuid();
            var expectedPath = _appSettings.RepositoryPath + @"\" + _metadataMock.Valuta.Year + @"\" + id + FileSuffix;
            var fileService = new Mock<IFileHandler>();
            var serializationService = new Mock<ISerializationHandler>();
            var metadataService = new MetadataService(fileService.Object, serializationService.Object, _appSettings);

            Func<Task> t = async () =>
                await metadataService.SaveMetadata(null, id);

            t.Should().Throw<ArgumentNullException>();
        }
    }
}
