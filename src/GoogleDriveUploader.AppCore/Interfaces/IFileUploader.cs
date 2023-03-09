namespace GoogleDriveUploader.AppCore.Interfaces
{
    public interface IFileUploader
    {
        public Task UploadAllNotUploadedFiles();
    }
}
