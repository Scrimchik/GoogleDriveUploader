using GoogleDriveUploader.AppCore.Interfaces;

namespace GoogleDriveUploader.Web.BackgroundJobs
{
    public class UploaderJob : BackgroundService
    {
        private readonly IFileUploader fileUploader;

        public UploaderJob(IFileUploader fileUploader)
        {
            this.fileUploader = fileUploader;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await fileUploader.UploadAllNotUploadedFiles();
        }
    }
}
