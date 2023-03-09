using GoogleDriveUploader.AppCore.Exceptions;
using GoogleDriveUploader.AppCore.Interfaces;
using Microsoft.Extensions.Logging;
using FileInfo = GoogleDriveUploader.AppCore.Entities.FileInfo;

namespace GoogleDriveUploader.AppCore.Services
{
    public class FileUploader : IFileUploader
    {
        private readonly IFileWorker fileWorker;
        private readonly IGoogleDriveClient googleDriveClient;
        private readonly IUploadedFilesInfoRepository uploadedFilesInfoRepository;
        private readonly ILogger<FileUploader> logger;
        private readonly string mainFolderPath;
        private FileSystemWatcher? fileSystemWatcher;

        public FileUploader(string mainFolderPath, IGoogleDriveClient googleDriveClient, IFileWorker fileWorker, IUploadedFilesInfoRepository uploadedFilesInfoRepository, ILoggerFactory loggerFactory)
        {
            this.mainFolderPath = mainFolderPath ?? throw new ArgumentNullException("There is no given folder path. You have to put folder path in appsettings.json.");
            InitializeMainFileWatcher(mainFolderPath);
            this.googleDriveClient = googleDriveClient ?? throw new ArgumentNullException(nameof(googleDriveClient));
            this.fileWorker = fileWorker ?? throw new ArgumentNullException(nameof(fileWorker));
            this.uploadedFilesInfoRepository = uploadedFilesInfoRepository ?? throw new ArgumentNullException(nameof(uploadedFilesInfoRepository));
            logger = loggerFactory.CreateLogger<FileUploader>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        private void InitializeMainFileWatcher(string folderPath)
        {
            var fileWatcher = new FileSystemWatcher(folderPath);
            fileWatcher.Filter = "*.*";
            fileWatcher.IncludeSubdirectories = true;
            fileWatcher.EnableRaisingEvents = true;
            fileWatcher.Created += OnFileCreatedAsync;
            fileWatcher.Changed += OnFileChangedAsync;
            fileWatcher.Deleted += OnFileDeletedAsync;
            fileWatcher.Renamed += OnFileRenamedAsync;
            fileSystemWatcher = fileWatcher;
        }

        private async void OnFileCreatedAsync(object? sender, FileSystemEventArgs e)
        {
            try
            {
                var fileInfo = CreateFileInfoFromPath(e.FullPath);
                logger.LogInformation($"Discovered created file {fileInfo.FilePath}");
                var uploadedFileInfo = await googleDriveClient.UploadFilesAsync(new List<FileInfo>() { fileInfo });
                uploadedFilesInfoRepository.AddFilesInfo(uploadedFileInfo);
            }
            catch (GoogleException ex)
            {
                logger.LogError($"Failed to upload file {e.FullPath}. Reason:\n{ex.Message}.\n Try to fix problem and restart service.");
            }
        }

        private async void OnFileChangedAsync(object? sender, FileSystemEventArgs e)
        {
            try
            {
                var uploadedFileInfo = uploadedFilesInfoRepository.GetFileInfoByPath(e.FullPath);
                logger.LogInformation($"Discovered changed file {e.FullPath}");
                if (uploadedFileInfo == null)
                {
                    var fileInfo = CreateFileInfoFromPath(e.FullPath);
                    var uploadedFilesInfo = await googleDriveClient.UploadFilesAsync(new List<FileInfo>() { fileInfo });
                    uploadedFilesInfoRepository.AddFilesInfo(uploadedFilesInfo);
                }
                else
                    await googleDriveClient.UpdateFileAsync(uploadedFileInfo);                 
            }
            catch (GoogleException ex)
            {
                logger.LogError($"Failed to update file {e.FullPath}. Reason:\n{ex.Message}.\n Try to fix problem and restart service.");
            }
        }

        private async void OnFileDeletedAsync(object? sender, FileSystemEventArgs e)
        {
            try
            {
                var fileInfo = uploadedFilesInfoRepository.GetFileInfoByPath(e.FullPath);
                logger.LogInformation($"Discovered deleted file {e.FullPath}");
                if (fileInfo != null)
                {
                    await googleDriveClient.DeleteFileAsync(fileInfo);
                    uploadedFilesInfoRepository.DeleteFileInfo(fileInfo);
                }
            }
            catch (GoogleException ex)
            {
                logger.LogError($"Failed to delete file {e.FullPath}. Reason:\n{ex.Message}.\n Try to fix problem and restart service.");
            }
        }

        private async void OnFileRenamedAsync(object? sender, FileSystemEventArgs e)
        {
            try
            {
                var fileInfo = uploadedFilesInfoRepository.GetFileInfoByPath((e as RenamedEventArgs).OldFullPath);
                logger.LogInformation($"Discovered changed file name {e.FullPath}");
                if (fileInfo != null)
                {
                    fileInfo.FilePath = e.FullPath;
                    await googleDriveClient.RenameFileAsync(fileInfo);
                    uploadedFilesInfoRepository.UpdateFileInfo(fileInfo);
                }
            }
            catch (GoogleException ex)
            {
                logger.LogError($"Failed to delete file {e.FullPath}. Reason:\n{ex.Message}.\n Try to fix problem and restart service.");
            }
        }

        private FileInfo CreateFileInfoFromPath(string path)
        {
            var parentFolderPath = Path.GetDirectoryName(path);
            var parentFolderDriveId = uploadedFilesInfoRepository.GetFileInfoByPath(parentFolderPath)?.GoogleDriveId;
            return new FileInfo(path, parentFolderDriveId: parentFolderDriveId);
        }

        public async Task UploadAllNotUploadedFiles()
        {
            var notUploadedFilesInfo = GetNotUploadedFilesInfo();
            logger.LogInformation($"Recievied {notUploadedFilesInfo.Count} not uploaded files.");

            if (notUploadedFilesInfo.Count > 0)
            {
                try
                {
                    var uploadedFilesInfo = await googleDriveClient.UploadFilesAsync(notUploadedFilesInfo);
                    uploadedFilesInfoRepository.AddFilesInfo(uploadedFilesInfo);
                }
                catch (GoogleException ex)
                {
                    logger.LogError($"Failed to upload files {notUploadedFilesInfo.Count}. Reason:\n{ex.Message}.\n Try to fix problem and restart service.");
                }
            }
        }

        private List<FileInfo> GetNotUploadedFilesInfo()
        {
            var allFilesInfo = fileWorker.GetAllFilesInfo(mainFolderPath);
            var uploadedFilesInfo = uploadedFilesInfoRepository.GetAllFilesInfo();

            return allFilesInfo.ExceptBy(uploadedFilesInfo.Select(t => t.FilePath), t => t.FilePath).Select(fileInfo =>
            {
                fileInfo.ParentFolderDriveId = uploadedFilesInfo.FirstOrDefault(t => t.FilePath == fileInfo.ParentFolderPath)?.GoogleDriveId;
                return fileInfo;
            }).ToList();
        }
    }
}
                                                 