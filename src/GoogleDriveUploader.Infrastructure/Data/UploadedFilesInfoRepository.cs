using GoogleDriveUploader.AppCore.Interfaces;
using LiteDB;
using FileInfo = GoogleDriveUploader.AppCore.Entities.FileInfo;
using FileInfoFromDb = GoogleDriveUploader.Infrastructure.Entities.FileInfo;

namespace GoogleDriveUploader.DataAccess.Common
{
    public sealed class UploadedFilesInfoRepository : IUploadedFilesInfoRepository
    {
        private readonly string conString = AppContext.BaseDirectory + @"\Cache\Cache.db";

        public UploadedFilesInfoRepository()
        {         
            Directory.CreateDirectory(AppContext.BaseDirectory + @"\Cache\");
        }

        public IList<FileInfo> GetAllFilesInfo() 
        {
            using (var database = new LiteDatabase(conString))
            {
                var filesInfo = database.GetCollection<FileInfoFromDb>().FindAll();
                return filesInfo.Select(fileInfo => new FileInfo(fileInfo.FilePath, fileInfo.GoogleDriveId, fileInfo.ParentFolderDriveId)).ToList();
            }
        }

        public FileInfo? GetFileInfoByPath(string? path)
        {
            using (var database = new LiteDatabase(conString))
            {
                var fileInfo = database.GetCollection<FileInfoFromDb>().FindOne(Query.EQ("FilePath", path));
                return fileInfo == null ? null : new FileInfo(fileInfo.FilePath, fileInfo.GoogleDriveId, fileInfo.ParentFolderDriveId);
            }
        }

        public void AddFilesInfo(IList<FileInfo> uploadedFilesInfo)
        {
            using (var database = new LiteDatabase(conString))
            {
                foreach (var fileInfo in uploadedFilesInfo)
                    database.GetCollection<FileInfoFromDb>().Insert(new FileInfoFromDb(fileInfo.FilePath, fileInfo.GoogleDriveId, fileInfo.ParentFolderDriveId));
            }
        }

        public void UpdateFileInfo(FileInfo updatedFileInfo)
        {
            using (var database = new LiteDatabase(conString))
            {           
                var fileInfo = database.GetCollection<FileInfoFromDb>().FindById(updatedFileInfo.GoogleDriveId);
                fileInfo.GoogleDriveId = updatedFileInfo.GoogleDriveId;
                fileInfo.ParentFolderDriveId = updatedFileInfo.ParentFolderDriveId;
                fileInfo.FilePath = updatedFileInfo.FilePath;
                database.GetCollection<FileInfoFromDb>().Update(fileInfo);
            }
        }

        public void DeleteFileInfo(FileInfo updatedFileInfo)
        {
            using (var database = new LiteDatabase(conString))
            {
                database.GetCollection<FileInfoFromDb>().DeleteMany(file => file.FilePath == updatedFileInfo.FilePath);
            }
        }
    }
}
