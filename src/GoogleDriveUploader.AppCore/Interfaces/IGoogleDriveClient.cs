using FileInfo = GoogleDriveUploader.AppCore.Entities.FileInfo;

namespace GoogleDriveUploader.AppCore.Interfaces
{
    public interface IGoogleDriveClient
    {
        public Task<IList<FileInfo>> UploadFilesAsync(IList<FileInfo> filesInfo);
        public Task UpdateFileAsync(FileInfo fileInfo);
        public Task DeleteFileAsync(FileInfo fileInfo);
        public Task RenameFileAsync(FileInfo fileInfo);
    }
}
