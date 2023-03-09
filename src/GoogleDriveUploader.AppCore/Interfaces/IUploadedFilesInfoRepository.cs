using FileInfo = GoogleDriveUploader.AppCore.Entities.FileInfo;

namespace GoogleDriveUploader.AppCore.Interfaces
{
    public interface IUploadedFilesInfoRepository
    {
        public IList<FileInfo> GetAllFilesInfo();
        public void AddFilesInfo(IList<FileInfo> uploadedFilesInfo);
        public FileInfo? GetFileInfoByPath(string? path);
        public void UpdateFileInfo(FileInfo updatedFileInfo);
        public void DeleteFileInfo(FileInfo updatedFileInfo);
    }
}
