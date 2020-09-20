using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Xunit;
using ZbW.Testing.Dms.Client.Model;
using ZbW.Testing.Dms.Client.Services;
using ZbW.Testing.Dms.Client.Services.Interfaces;

namespace Zbw.Testing.Dms.Client.Test
{
    public class FileSystemStorageTest
    {
        private const string FileSuffix = "_Content";

        private readonly Document _documentMock;
        private readonly MetadataItem _metadataMock;
        private readonly AppSettings _appSettings;

        public FileSystemStorageTest()
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

            _documentMock = new Document()
            {
               Id = Guid.NewGuid(),
               Metadata = _metadataMock,
               File = new MemoryStream()
            };

            _appSettings = new AppSettings()
            {
                RepositoryPath = @"C:\temp"
            };
        }

        [Fact]
        public async void LoadDocumentValid()
        {
            var doc = new Document()
            {
                Id = Guid.NewGuid(),
                Metadata = _metadataMock,
                File = null
            };
            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(x => x.LoadMetadata(It.IsAny<Guid>())).Returns(Task.FromResult(_metadataMock));
            var fileHandlerService = new Mock<IFileHandler>();
            var storageHandler = new FilesystemStorageHandler(metadataService.Object, _appSettings, fileHandlerService.Object);

            var result = await storageHandler.LoadDocument(doc.Id);

            result.Should().BeEquivalentTo(doc);
            metadataService.Verify(x => x.LoadMetadata(doc.Id));
        }

        [Fact]
        public void LoadDocumentMissingId()
        {
            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(x => x.LoadMetadata(It.IsAny<Guid>())).Throws<ArgumentNullException>();
            var fileHandlerService = new Mock<IFileHandler>();
            var storageHandler = new FilesystemStorageHandler(metadataService.Object, _appSettings, fileHandlerService.Object);

            Func<Task> t = async () =>
                await storageHandler.LoadDocument(Guid.Empty);

            t.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public async void SaveDocumentValid()
        {
            var expectedPath = _metadataMock.Valuta.Year + @"\" + _documentMock.Id + FileSuffix + _documentMock.Metadata.FileEnding;
            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(x => x.SaveMetadata(It.IsAny<MetadataItem>(), It.IsAny<Guid>())).Returns(Task.FromResult(_documentMock.Metadata));
            var fileHandlerService = new Mock<IFileHandler>();
            fileHandlerService.Setup(x => x.SaveFile(_documentMock.File, It.IsAny<string>()));
            var storageHandler = new FilesystemStorageHandler(metadataService.Object, _appSettings, fileHandlerService.Object);

            await storageHandler.SaveDocument(_documentMock);

            metadataService.Verify(x => x.SaveMetadata(_documentMock.Metadata, _documentMock.Id));
            fileHandlerService.Verify(x => x.SaveFile(_documentMock.File, expectedPath));
        }

        [Fact]
        public async void SaveDocumentMissingId()
        {
            var doc = new Document()
            {
                Id = Guid.Empty,
                Metadata = _metadataMock,
                File = new MemoryStream()
            };
            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(x => x.SaveMetadata(It.IsAny<MetadataItem>(), It.IsAny<Guid>())).Returns(Task.FromResult(_metadataMock));
            var fileHandlerService = new Mock<IFileHandler>();
            fileHandlerService.Setup(x => x.SaveFile(doc.File, It.IsAny<string>()));
            var storageHandler = new FilesystemStorageHandler(metadataService.Object, _appSettings, fileHandlerService.Object);

            await storageHandler.SaveDocument(doc);

            doc.Id.Should().NotBeEmpty();
        }

        [Fact]
        public void SaveDocumentNoFile()
        {
            var doc = new Document()
            {
                Id = Guid.NewGuid(),
                Metadata = _metadataMock,
                File = null
            };
            var metadataService = new Mock<IMetadataService>();
            var fileHandlerService = new Mock<IFileHandler>();
            var storageHandler = new FilesystemStorageHandler(metadataService.Object, _appSettings, fileHandlerService.Object);

            Func<Task> t = async () =>
                await storageHandler.SaveDocument(doc);

            t.Should().Throw<FileNotFoundException>();
        }

        [Theory]
        [InlineData("test1", "test", 1)]
        [InlineData("test", "asdf", 1)]
        [InlineData("", "test", 2)]
        [InlineData("", "", 3)]
        [InlineData("", "1", 0)]
        public async void SearchDocumentValid(string term, string type, int count)
        {
            var doc1 = new Document()
            {
                Id = Guid.NewGuid(),
                Metadata = new MetadataItem()
                {
                    FileEnding = ".test",
                    CreationTime = DateTime.Now,
                    FileName = "test1",
                    Keywords = "test1",
                    Typ = "test",
                    Username = "test1",
                    Valuta = DateTime.Now
                }

            };
            var doc2 = new Document()
            {
                Id = Guid.NewGuid(),
                Metadata = new MetadataItem()
                {
                    FileEnding = ".test",
                    CreationTime = DateTime.Now,
                    FileName = "test2",
                    Keywords = "test2",
                    Typ = "test",
                    Username = "test2",
                    Valuta = DateTime.Now
                }
            };
            var doc3 = new Document()
            {
                Id = Guid.NewGuid(),
                Metadata = new MetadataItem()
                {
                    FileEnding = ".test",
                    CreationTime = DateTime.Now,
                    FileName = "test2",
                    Keywords = "test2",
                    Typ = "asdf",
                    Username = "test2",
                    Valuta = DateTime.Now
                }
            };
            var files = new string[]
            {
                _appSettings.RepositoryPath + @"\" + doc1.Id + FileSuffix + doc1.Metadata.FileEnding,
                _appSettings.RepositoryPath + @"\" + doc2.Id + FileSuffix + doc2.Metadata.FileEnding,
                _appSettings.RepositoryPath + @"\" + doc3.Id + FileSuffix + doc3.Metadata.FileEnding
            };

            var fileHandlerService = new Mock<IFileHandler>();
            fileHandlerService.Setup(x => x.SearchFiles(It.IsAny<string>())).ReturnsAsync(files);
            var metadataService = new Mock<IMetadataService>();
            metadataService.Setup(x => x.LoadMetadata(doc1.Id)).ReturnsAsync(doc1.Metadata);
            metadataService.Setup(x => x.LoadMetadata(doc2.Id)).ReturnsAsync(doc2.Metadata);
            metadataService.Setup(x => x.LoadMetadata(doc3.Id)).ReturnsAsync(doc3.Metadata);

            var storageHandler = new FilesystemStorageHandler(metadataService.Object, _appSettings, fileHandlerService.Object);

            var result = await storageHandler.SearchDocument(term, type);

            result.Should().HaveCount(count);
        }

        [Fact]
        public async void OpenFileExternal()
        {
            var expectedPath = _metadataMock.Valuta.Year + @"\" + _documentMock.Id + FileSuffix + _documentMock.Metadata.FileEnding;
            var metadataService = new Mock<IMetadataService>();
            var fileHandlerService = new Mock<IFileHandler>();
            fileHandlerService.Setup(x => x.OpenFile(It.IsAny<string>()));
            var storageHandler = new FilesystemStorageHandler(metadataService.Object, _appSettings, fileHandlerService.Object);

            await storageHandler.OpenDocumentExternal(_documentMock);

            fileHandlerService.Verify(x => x.OpenFile(expectedPath));
        }

    }
}
