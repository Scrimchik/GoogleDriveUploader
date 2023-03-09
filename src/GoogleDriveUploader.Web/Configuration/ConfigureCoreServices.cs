using GoogleDriveUploader.AppCore.Interfaces;
using GoogleDriveUploader.AppCore.Services;
using GoogleDriveUploader.DataAccess.Common;
using GoogleDriveUploader.GoogleAccess;

namespace GoogleDriveUploader.Worker.Configuration
{
    public static class ConfigureCoreServices
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddSingleton<IUploadedFilesInfoRepository, UploadedFilesInfoRepository>();
            services.AddSingleton<IFileWorker, FileWorker>();
            services.AddSingleton<IGoogleDriveClient, GoogleDriveClient>();
            services.AddSingleton<IFileUploader, FileUploader>();
        }
    }
}
