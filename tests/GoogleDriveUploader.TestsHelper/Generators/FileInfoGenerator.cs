using FileInfo = GoogleDriveUploader.AppCore.Entities.FileInfo;

namespace GoogleDriveUploader.TestsHelper.Generators
{
    public static class FileInfoGenerator
    {
        public static List<FileInfo> GetFilesInfoFromTestFolder( )
        {
            var testFolderInfo = new DirectoryInfo(@"..\..\..\..\TestFiles");
            var filesInfo = testFolderInfo.GetFileSystemInfos();
            var result = new List<FileInfo>();
            result.Add(new FileInfo(testFolderInfo.FullName));

            foreach (var file in filesInfo)
            {
                result.Add(new FileInfo(file.FullName));
            }

            return result;
        }
    }
}
