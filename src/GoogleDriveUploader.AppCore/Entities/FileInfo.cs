using GoogleDriveUploader.Common.Enums;

namespace GoogleDriveUploader.AppCore.Entities
{
    public class FileInfo
    {
        public FileInfo(string filePath, string? googleDriveId = null, string? parentFolderDriveId = null)
        {
            FilePath = filePath;
            GoogleDriveId = googleDriveId;
            ParentFolderDriveId = parentFolderDriveId;
        }

        public string? GoogleDriveId { get; set; }
        public string FilePath { get; set; }
        public string FileName => Path.GetFileName(FilePath);
        public string? ParentFolderDriveId { get; set; }
        public string? ParentFolderPath => Path.GetDirectoryName(FilePath);
        public FileType FileType => new System.IO.FileInfo(FilePath).Attributes == FileAttributes.Directory
            ? FileType.Folder
            : FileType.File;
    }
}
