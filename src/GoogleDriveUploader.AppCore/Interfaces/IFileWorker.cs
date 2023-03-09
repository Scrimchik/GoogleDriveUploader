using FileInfo = GoogleDriveUploader.AppCore.Entities.FileInfo;

namespace GoogleDriveUploader.AppCore.Interfaces
{
    public interface IFileWorker
    {
        public IList<FileInfo> GetAllFilesInfo(string folderPath);
    }
}
