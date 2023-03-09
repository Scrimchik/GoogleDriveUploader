using GoogleDriveUploader.TestsHelper.Generators;
using NUnit.Framework;
using FileInfo = GoogleDriveUploader.Common.Entities.FileInfo;
using FileWorkerService = GoogleDriveUploader.FileWorker.FileWorker;

namespace GoogleDriveUploader.FileWorker.Tests.Common
{
    public class FileWorkerTests
    {
        private FileWorkerService fileWorker;
        private const string mainFolderPath = @"..\..\..\..\TestFiles";

        [SetUp]
        public void SetUp()
        {
            fileWorker = new FileWorkerService();
        }

        [Test]
        public void GetAllFilesInfo_ShouldReturnFilesInfoWithAllNecessaryInfo()
        {
            // Act
            var result = fileWorker.GetAllFilesInfo(mainFolderPath);

            // Assert
            Assert.That(result.Any(t => t.FileName != null && t.FilePath != null && t.ParentFolderPath != null && t.FileType != null), Is.EqualTo(true));
        }

        [Test]
        public void GetAllFilesInfo_ShouldReturnFilesInfo()
        {
            // Arrange
            List<FileInfo> exceptedFilesInfo = FileInfoGenerator.GetFilesInfoFromTestFolder();

            // Act
            var result = fileWorker.GetAllFilesInfo(mainFolderPath);

            // Assert
            Assert.That(result.Select(t => new { t.FileName, t.FilePath, t.ParentFolderPath, t.ParentFolderDriveId, t.GoogleDriveId }),
                Is.EquivalentTo(exceptedFilesInfo.Select(t => new { t.FileName, t.FilePath, t.ParentFolderPath, t.ParentFolderDriveId, t.GoogleDriveId })));
        }
    }
}
