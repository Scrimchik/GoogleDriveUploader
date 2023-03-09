using Google.Apis.Services;
using GoogleDriveUploader.GoogleAccess.Auth;
using Microsoft.Extensions.Logging;
using GoogleDriveService = Google.Apis.Drive.v3.DriveService;
using GoogleFile = Google.Apis.Drive.v3.Data.File;

namespace GoogleDriveUploader.GoogleAccess
{
    public class DriveService
    {
        private GoogleDriveService driveService;

        public DriveService(ILogger<DriveService> logger)
        {
            var credsTask = GoogleAuthenticator.GenerateGoogleDriveCredentials(new CancellationTokenSource(TimeSpan.FromSeconds(30)).Token);
            if (credsTask.IsFaulted)
            {
                logger.LogCritical("Authorize Error. You have to open application as a Console to authorize in Google drive.");
                throw new ArgumentNullException("Creds");
            }

            var creds = credsTask.Result;
            driveService = new GoogleDriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = creds,
                ApplicationName = "GoogleDriveUploader"
            });
        }

        public async Task<IList<string>> GenerateIdsAsync(int count)
        {
            var generateIdsRequest = driveService.Files.GenerateIds();
            generateIdsRequest.Count = count;
            var generatedIds = await generateIdsRequest.ExecuteAsync();
            return generatedIds.Ids;
        }

        public async Task UploadFolderAsync(GoogleFile folder)
        {
            await driveService.Files.Create(folder).ExecuteAsync();
        }

        public async Task UploadFileAsync(GoogleFile file, Stream fileStream)
        {
            await driveService.Files.Create(file, fileStream, file.MimeType).UploadAsync();
        }

        public async Task UpdateFileAsync(GoogleFile file, string fileId, Stream fileStream)
        {
            await driveService.Files.Update(file, fileId, fileStream, file.MimeType).UploadAsync();
        }

        public async Task RenameFileAsync(GoogleFile file, string fileId)
        {
            await driveService.Files.Update(file, fileId).ExecuteAsync();
        }

        public async Task DeleteFileAsync(string fileId)
        {
            await driveService.Files.Delete(fileId).ExecuteAsync();
        }
    }
}
