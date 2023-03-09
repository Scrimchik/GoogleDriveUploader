using Google;
using GoogleDriveUploader.Common.Enums;
using GoogleDriveUploader.TestsHelper.Generators;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using File = Google.Apis.Drive.v3.Data.File;
using FileInfo = GoogleDriveUploader.Common.Entities.FileInfo;

namespace GoogleDriveUploader.GoogleAccess.Tests.Common
{
    public class GoogleDriveClientTests
    {
        private Mock<IDriveService> driveService;
        private Mock<ILogger<GoogleDriveClient>> logger;
        private GoogleDriveClient driveClient;
        private List<FileInfo> filesInfo;

        [SetUp]
        public void SetUp()
        {
            var mockRepo = new MockRepository(MockBehavior.Default);
            driveService = mockRepo.Create<IDriveService>();    
            logger = mockRepo.Create<ILogger<GoogleDriveClient>>();
            driveClient = new GoogleDriveClient(logger.Object, driveService.Object);

            filesInfo = FileInfoGenerator.GetFilesInfoFromTestFolder();
        }

        [Test]
        public void Construcutor_Passes()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => new GoogleDriveClient(logger.Object, driveService.Object));
        }

        [Test]
        public void Construcutor_IfParameterIsNull_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new GoogleDriveClient(null, driveService.Object));
            Assert.Throws<ArgumentNullException>(() =>
                new GoogleDriveClient(logger.Object, null));
        }

        [Test]
        public async Task UploadFilesAsync_ShouldRequestAndSetGenerateIds()
        {
            // Arrange
            var generatedIds = SetUpGenerateIds();

            // Act
            var result = await driveClient.UploadFilesAsync(filesInfo);

            // Assert
            driveService.Verify(service => service.GenerateIdsAsync(filesInfo.Count), Times.Once);
            Assert.That(result.Select(t => t.GoogleDriveId), Is.EquivalentTo(generatedIds));
        }

        [Test]
        public async Task UploadFilesAsync_ShouldUploadFilesAndFolders()
        {
            // Arrange
            SetUpGenerateIds();
            var googleFiles = filesInfo.Select(fileInfo =>
            {
                string parentId = fileInfo.ParentFolderDriveId ?? filesInfo.FirstOrDefault(t => t.FilePath == fileInfo.ParentFolderPath)?.GoogleDriveId;
                new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FilePath, out string? contentType);
                string mimeContentType = fileInfo.FileType == FileType.Folder ? "application/vnd.google-apps.folder" : contentType ?? "application/octet-stream";
                return new File()
                {
                    Id = fileInfo.GoogleDriveId,
                    Name = fileInfo.FileName,
                    MimeType = mimeContentType,
                    Parents = parentId != null ? new List<string>() { parentId } : null
                };
            });
          
            // Act
            await driveClient.UploadFilesAsync(filesInfo);

            // Assert
            foreach(var googleFile in googleFiles)
            {
                if (googleFile.MimeType == "application/vnd.google-apps.folder")
                    driveService.Verify(service => service.UploadFolderAsync(It.Is<File>(file => 
                    file.Id == googleFile.Id && file.Name == googleFile.Name && file.MimeType == googleFile.MimeType)), Times.Once);
                else
                    driveService.Verify(service => service.UploadFileAsync(It.Is<File>(file =>
                    file.Id == googleFile.Id && file.Name == googleFile.Name && file.MimeType == googleFile.MimeType), It.IsAny<Stream>()), Times.Once);
            } 
        }

        [Test]
        public void UploadFilesAsync_UploadFileThrowGoogleApiException()
        {
            // Arrange
            SetUpGenerateIds();
            driveService.Setup(service => service.UploadFolderAsync(It.IsAny<File>())).Throws(new GoogleApiException("unathorized", "Unathorized"));
            driveService.Setup(service => service.UploadFileAsync(It.IsAny<File>(), It.IsAny<Stream>())).Throws(new GoogleApiException("unathorized", "Unathorized"));

            // Act
            Assert.ThrowsAsync<GoogleApiException>(() => driveClient.UploadFilesAsync(filesInfo));

            // Assert
            driveService.Verify(service => service.UploadFolderAsync(It.IsAny<File>()), Times.AtMostOnce);
            driveService.Verify(service => service.UploadFileAsync(It.IsAny<File>(), It.IsAny<Stream>()), Times.AtMostOnce);
        }

        [Test]
        public async Task UpdateFileAsync_ShouldCallDriveService()
        {
            // Arrange
            var fileInfo = filesInfo.First(t => t.FileType != FileType.Folder);
            new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FilePath, out string? contentType);
            string mimeContentType = fileInfo.FileType == FileType.Folder ? "application/vnd.google-apps.folder" : contentType ?? "application/octet-stream";
            var googleFile = new File()
            {
                Name = fileInfo.FileName,
                MimeType = mimeContentType
            };

            // Act
            await driveClient.UpdateFileAsync(fileInfo);

            // Assert
            driveService.Verify(service => service.UpdateFileAsync(It.Is<File>(t => t.Name == googleFile.Name), fileInfo.GoogleDriveId, It.IsAny<Stream>()), Times.Once);
        }

        [Test]
        public async Task UpdateFileAsync_ReceivedFolderInfo_ShouldNotCallDriveService()
        {
            // Arrange
            var fileInfo = filesInfo.First(t => t.FileType == FileType.Folder);
            new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FilePath, out string? contentType);
            string mimeContentType = fileInfo.FileType == FileType.Folder ? "application/vnd.google-apps.folder" : contentType ?? "application/octet-stream";
            var googleFile = new File()
            {
                Name = fileInfo.FileName,
                MimeType = mimeContentType
            };

            // Act
            await driveClient.UpdateFileAsync(fileInfo);

            // Assert
            driveService.Verify(service => service.UpdateFileAsync(It.IsAny<File>(), It.IsAny<string>(), It.IsAny<Stream>()), Times.Never);
        }

        [Test]
        public void UpdateFileAsync_UpdateFileThrowGoogleApiException()
        {
            // Arrange
            driveService.Setup(service => service.UpdateFileAsync(It.IsAny<File>(), It.IsAny<string>(), It.IsAny<Stream>())).Throws(new GoogleApiException("unathorized", "Unathorized"));

            // Act & Assert
            Assert.ThrowsAsync<GoogleApiException>(() => driveClient.UpdateFileAsync(filesInfo.First(t => t.FileType != FileType.Folder)));
        }

        [Test]
        public async Task DeleteFileAsync_ShouldCallDriveService()
        {
            // Act
            await driveClient.DeleteFileAsync(filesInfo.First());

            // Assert
            driveService.Verify(service => service.DeleteFileAsync(filesInfo.First().GoogleDriveId));
        }

        [Test]
        public async Task RenameFileAsync_ShouldCallDriveService()
        {
            // Arrange
            var fileInfo = filesInfo.First();
            new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FilePath, out string? contentType);
            string mimeContentType = fileInfo.FileType == FileType.Folder ? "application/vnd.google-apps.folder" : contentType ?? "application/octet-stream";
            var googleFile = new File()
            {
                Name = fileInfo.FileName,
                MimeType = mimeContentType
            };

            // Act
            await driveClient.RenameFileAsync(fileInfo);

            // Assert
            driveService.Verify(service => service.RenameFileAsync(It.Is<File>(t => t.Name == googleFile.Name), fileInfo.GoogleDriveId));
        }

        private List<string> SetUpGenerateIds()
        {
            List<string> generatedIds = new List<string>();
            for (int i = 0; i < filesInfo.Count; i++)
            {
                string generatedId = Guid.NewGuid().ToString();
                generatedIds.Add(generatedId);
                filesInfo[i].GoogleDriveId = generatedId;
            }

            driveService
                .Setup(service => service.GenerateIdsAsync(filesInfo.Count))
                .ReturnsAsync(generatedIds);

            return generatedIds;
        }
    }
}
