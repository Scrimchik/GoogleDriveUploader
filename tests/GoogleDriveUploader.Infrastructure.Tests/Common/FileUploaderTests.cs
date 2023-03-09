using Google;
using GoogleDriveUploader.DataAccess.Common;
using GoogleDriveUploader.FileWorker.Common;
using GoogleDriveUploader.GoogleAccess.Common;
using GoogleDriveUploader.Infrastructure.Common;
using GoogleDriveUploader.TestsHelper;
using GoogleDriveUploader.TestsHelper.Generators;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using FileInfo = GoogleDriveUploader.Common.Entities.FileInfo;

namespace GoogleDriveUploader.Infrastructure.Tests.Common
{
    public class FileUploaderTests
    {
        private FileUploader fileUploader;
        private Mock<IGoogleDriveClient> googleClient;
        private Mock<IFileWorker> fileWorker;
        private Mock<IUploadedFilesInfoRepository> filesRepository;
        private List<FileInfo> filesInfo;
        private const string mainFolderPath = @"..\..\..\..\TestFiles";
        private TestFileOperations testFileOperations;

        [SetUp]
        public void SetUp()
        {
            var mockRepo = new MockRepository(MockBehavior.Default);
            googleClient = mockRepo.Create<IGoogleDriveClient>();
            fileWorker = mockRepo.Create<IFileWorker>();
            filesRepository = mockRepo.Create<IUploadedFilesInfoRepository>();
            fileUploader = new FileUploader(mainFolderPath, googleClient.Object, fileWorker.Object, filesRepository.Object, new NullLoggerFactory());
            filesInfo = FileInfoGenerator.GetFilesInfoFromTestFolder();
        }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            testFileOperations = new TestFileOperations();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            testFileOperations.DeleteAllTestFiles();
        }

        [Test]
        public void Construcutor_Passes()
        {
            // Act & Assert
            Assert.DoesNotThrow(() => new FileUploader(mainFolderPath, googleClient.Object, fileWorker.Object, filesRepository.Object, new NullLoggerFactory()));
        }

        [Test]
        public void Construcutor_IfParameterIsNull_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                new FileUploader(null, googleClient.Object, fileWorker.Object, filesRepository.Object, new NullLoggerFactory()));
            Assert.Throws<ArgumentNullException>(() =>
                new FileUploader(mainFolderPath, null, fileWorker.Object, filesRepository.Object, new NullLoggerFactory()));
            Assert.Throws<ArgumentNullException>(() =>
                new FileUploader(mainFolderPath, googleClient.Object, null, filesRepository.Object, new NullLoggerFactory()));
            Assert.Throws<ArgumentNullException>(() =>
                new FileUploader(mainFolderPath, googleClient.Object, fileWorker.Object, null, new NullLoggerFactory()));
        }

        [Test]
        public async Task UploadAllNotUploadedFiles_ShouldReceiveNotUploadFilesInfo()
        {
            // Arrange
            SetupGettingNotUploadedFilesWithFewNotUploadedFiles();

            // Act
            await fileUploader.UploadAllNotUploadedFiles();

            // Assert
            fileWorker.Verify(worker => worker.GetAllFilesInfo(mainFolderPath), Times.Once);
            filesRepository.Verify(repo => repo.GetAllFilesInfo(), Times.Once());
        }

        [Test]
        public async Task UploadAllNotUploadedFiles_ThereIsNoNotUploadedFiles_ShouldNotCallServicesToPublishFiles()
        {
            // Arrange
            fileWorker.Setup(worker => worker.GetAllFilesInfo(mainFolderPath)).Returns(filesInfo);
            filesRepository.Setup(repo => repo.GetAllFilesInfo()).Returns(filesInfo);

            // Act
            await fileUploader.UploadAllNotUploadedFiles();

            // Assert
            googleClient.Verify(client => client.UploadFilesAsync(It.IsAny<List<FileInfo>>()), Times.Never);
            filesRepository.Verify(repo => repo.AddFilesInfo(It.IsAny<List<FileInfo>>()), Times.Never);
        }

        [Test]
        public async Task UploadAllNotUploadedFiles_ShouldPublishAndSaveUploadedFilesInfo()
        {
            // Arrange
            var notUploadedFilesInfo = SetupGettingNotUploadedFilesWithFewNotUploadedFiles();
            googleClient.Setup(client => client.UploadFilesAsync(notUploadedFilesInfo)).ReturnsAsync(notUploadedFilesInfo);

            // Act
            await fileUploader.UploadAllNotUploadedFiles();

            // Assert
            googleClient.Verify(client => client.UploadFilesAsync(notUploadedFilesInfo), Times.Once);
            filesRepository.Verify(repo => repo.AddFilesInfo(notUploadedFilesInfo), Times.Once);
        }

        [Test]
        public async Task UploadAllNotUploadedFiles_GoogleDriveClientThrowException()
        {
            // Arrange
            var notUploadedFilesInfo = SetupGettingNotUploadedFilesWithFewNotUploadedFiles();
            googleClient.Setup(client => client.UploadFilesAsync(It.IsAny<List<FileInfo>>())).ThrowsAsync(new GoogleApiException("forbidden"));

            // Act
            await fileUploader.UploadAllNotUploadedFiles();

            // Assert
            googleClient.Verify(client => client.UploadFilesAsync(notUploadedFilesInfo), Times.Once);
            filesRepository.Verify(repo => repo.AddFilesInfo(notUploadedFilesInfo), Times.Never);
        }

        [Test]
        public void OnFileCreatedAsync_ShouldUploadCreatedFile()
        {
            // Act
            var createdFileInfo = testFileOperations.CreateTestFileAsync().Result;

            // Assert
            filesRepository.Verify(repo => repo.GetFileInfoByPath(createdFileInfo.ParentFolderPath), Times.Once);
            googleClient.Verify(client => client.UploadFilesAsync(It.IsAny<List<FileInfo>>()), Times.Once);
            filesRepository.Verify(repo => repo.AddFilesInfo(It.IsAny<List<FileInfo>>()), Times.Once);
        }

        [Test]
        public void OnFileCreatedAsync_GoogleClientThrowsException()
        {
            // Arrange
            googleClient.Setup(client => client.UploadFilesAsync(It.IsAny<List<FileInfo>>())).ThrowsAsync(new GoogleApiException("forbidden"));

            // Act
            var createdFileInfo = testFileOperations.CreateTestFileAsync().Result;

            // Assert
            filesRepository.Verify(repo => repo.AddFilesInfo(It.IsAny<List<FileInfo>>()), Times.Never);
        }

        [Test]
        public void OnFileChangedAsync_ShouldCallFileInfoFromRepo()
        {
            // Act
            var changedFileInfo = testFileOperations.ChangeTestFile().Result;

            // Assert
            filesRepository.Verify(repo => repo.GetFileInfoByPath(changedFileInfo.FilePath), Times.Once);
        }

        [Test]
        public void OnFileChangedAsync_IfThereIsNoFileInfoInRepository_ShouldCreateAndPublishFileInfo()
        {
            // Act
            var changedFileInfo = testFileOperations.ChangeTestFile().Result;

            // Assert
            filesRepository.Verify(repo => repo.GetFileInfoByPath(changedFileInfo.ParentFolderPath), Times.Once);
            googleClient.Verify(client => client.UploadFilesAsync(It.IsAny<List<FileInfo>>()), Times.Once);
            filesRepository.Verify(repo => repo.AddFilesInfo(It.IsAny<List<FileInfo>>()), Times.Once);
        }

        [Test]
        public void OnFileChangedAsync_IfThereIsUploadedFileInfoInRepository_ShouldPublishIt()
        {
            // Act
            var changedFileInfo = testFileOperations.ChangeTestFile().Result;

            // Assert
            googleClient.Verify(client => client.UploadFilesAsync(It.IsAny<List<FileInfo>>()), Times.Once);
        }

        private List<FileInfo> SetupGettingNotUploadedFilesWithFewNotUploadedFiles()
        {
            List<FileInfo> uploadedFilesInfo = FileInfoGenerator.GetFilesInfoFromTestFolder();
            uploadedFilesInfo.RemoveRange(0, uploadedFilesInfo.Count - 2);
            List<FileInfo> notUploadedFilesInfo = filesInfo.ExceptBy(uploadedFilesInfo.Select(file => file.FilePath), file => file.FilePath).ToList();
            filesRepository.Setup(repo => repo.GetAllFilesInfo()).Returns(uploadedFilesInfo);

            fileWorker.Setup(worker => worker.GetAllFilesInfo(mainFolderPath)).Returns(filesInfo);
            filesRepository.Setup(repo => repo.GetAllFilesInfo()).Returns(uploadedFilesInfo);
            return notUploadedFilesInfo;
        }
    }
}
