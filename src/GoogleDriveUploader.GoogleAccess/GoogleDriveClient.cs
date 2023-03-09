using Google;
using GoogleDriveUploader.AppCore.Entities.Enums;
using GoogleDriveUploader.AppCore.Exceptions;
using GoogleDriveUploader.AppCore.Interfaces;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Logging;
using FileInfo = GoogleDriveUploader.AppCore.Entities.FileInfo;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace GoogleDriveUploader.GoogleAccess
{
    public class GoogleDriveClient : IGoogleDriveClient
    {
        private readonly DriveService driveService;
        private readonly ILogger<GoogleDriveClient> logger;

        public GoogleDriveClient(ILoggerFactory loggerFactory)
        {
            driveService = new DriveService(loggerFactory.CreateLogger<DriveService>());
            logger = loggerFactory.CreateLogger<GoogleDriveClient>() ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<IList<FileInfo>> UploadFilesAsync(IList<FileInfo> filesInfo)
        {
            await GenerateIds(filesInfo);

            foreach (var fileInfo in filesInfo)
            {
                fileInfo.ParentFolderDriveId ??= filesInfo.FirstOrDefault(t => t.FilePath == fileInfo.ParentFolderPath)?.GoogleDriveId;
                new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FilePath, out string? contentType);
                string mimeContentType = fileInfo.FileType == FileType.Folder ? "application/vnd.google-apps.folder" : contentType ?? "application/octet-stream";
                var googleFile = new GoogleFile()
                {
                    Id = fileInfo.GoogleDriveId,
                    DriveId = fileInfo.GoogleDriveId,
                    Name = fileInfo.FileName,
                    MimeType = mimeContentType,
                    Parents = fileInfo.ParentFolderDriveId != null ? new List<string>() { fileInfo.ParentFolderDriveId } : null
                };

                try
                {
                    if (fileInfo.FileType == FileType.Folder)
                        await driveService.UploadFolderAsync(googleFile);
                    else
                        using (var fileStream = new StreamReader(fileInfo.FilePath))
                            await driveService.UploadFileAsync(googleFile, fileStream.BaseStream);
                }
                catch (GoogleApiException ex)
                {
                    throw new GoogleException(ex.Message, ex.InnerException);
                }
                logger.LogInformation($"Successfuly uploaded file {fileInfo.FilePath}");
            }

            return filesInfo;
        }

        public async Task UpdateFileAsync(FileInfo fileInfo)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FilePath, out string? contentType);
            string mimeContentType = fileInfo.FileType == FileType.Folder ? "application/vnd.google-apps.folder" : contentType ?? "application/octet-stream";
            var googleFile = new GoogleFile()
            {
                Name = fileInfo.FileName,
                MimeType = mimeContentType
            };

            if (fileInfo.FileType == FileType.File)// We don't need to update folders in google drive.
            {
                try
                {
                    using (var fileStream = new StreamReader(fileInfo.FilePath))
                        await driveService.UpdateFileAsync(googleFile, fileInfo.GoogleDriveId, fileStream.BaseStream);
                }
                catch (GoogleApiException ex)
                {
                    throw new GoogleException(ex.Message, ex.InnerException);
                }
                logger.LogInformation($"Successfuly updated file {fileInfo.FilePath}");
            }
        }

        public async Task DeleteFileAsync(FileInfo fileInfo)
        {
            try
            {
                await driveService.DeleteFileAsync(fileInfo.GoogleDriveId);
            }
            catch (GoogleApiException ex)
            {
                throw new GoogleException(ex.Message, ex.InnerException);
            }
            logger.LogInformation($"Successfuly deleted file {fileInfo.FilePath}");
        }

        public async Task RenameFileAsync(FileInfo fileInfo)
        {
            new FileExtensionContentTypeProvider().TryGetContentType(fileInfo.FilePath, out string? contentType);
            string mimeContentType = fileInfo.FileType == FileType.Folder ? "application/vnd.google-apps.folder" : contentType ?? "application/octet-stream";
            var googleFile = new GoogleFile()
            {
                Name = fileInfo.FileName,
                MimeType = mimeContentType
            };

            try
            {
                await driveService.RenameFileAsync(googleFile, fileInfo.GoogleDriveId);
            }
            catch (GoogleApiException ex)
            {
                throw new GoogleException(ex.Message, ex.InnerException);
            }
            logger.LogInformation($"Successfuly renamed file {fileInfo.FilePath}");
        }

        private async Task GenerateIds(IList<FileInfo> filesInfo)
        {
            try
            {
                for (int i = 0; i < filesInfo.Count(); i++)
                    filesInfo[i].GoogleDriveId = generatedIds[i];
            }
            catch (GoogleApiException ex)
            {
                throw new GoogleException(ex.Message, ex.InnerException);
            }
        }
    }
}
