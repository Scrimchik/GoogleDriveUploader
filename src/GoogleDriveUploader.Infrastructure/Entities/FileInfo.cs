using LiteDB;

namespace GoogleDriveUploader.Infrastructure.Entities
{
    public class FileInfo
    {
        public FileInfo(string path, string googleDriveId, string parentFolderDriveId) 
        {
            FilePath = path;
            GoogleDriveId = googleDriveId;
            ParentFolderDriveId = parentFolderDriveId;
        }

        [BsonId]
        public string GoogleDriveId { get; set; }
        public string FilePath { get; set; }
        public string ParentFolderDriveId { get; set; }
    }
}
