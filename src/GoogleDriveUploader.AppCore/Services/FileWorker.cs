using GoogleDriveUploader.AppCore.Interfaces;
using FileInfo = GoogleDriveUploader.AppCore.Entities.FileInfo;

namespace GoogleDriveUploader.AppCore.Services
{
    public class FileWorker : IFileWorker
    {
        public IList<FileInfo> GetAllFilesInfo(string folderPath)
        {
            var folderInfo = new DirectoryInfo(folderPath);
            var allFilesInfoInFolder = folderInfo.GetFileSystemInfos();
            List<FileInfo> filesInfo = new List<FileInfo>();
            filesInfo.Add(new FileInfo(folderInfo.FullName));

            foreach (var fileInfo in allFilesInfoInFolder)
            {
                if (fileInfo.Attributes.HasFlag(FileAttributes.Directory))
                {
                    filesInfo.AddRange(GetAllFilesInfo(fileInfo.FullName));
                    continue;
                }

                filesInfo.Add(new FileInfo(fileInfo.FullName));
            }

            return filesInfo;
        }
    }
}
