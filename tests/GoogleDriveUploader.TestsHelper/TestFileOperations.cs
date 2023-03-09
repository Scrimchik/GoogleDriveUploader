using System.Text;
using FileInfo = GoogleDriveUploader.Common.Entities.FileInfo;

namespace GoogleDriveUploader.TestsHelper
{
    public class TestFileOperations
    {
        private readonly string testFilePath = @"..\..\..\..\TestFiles\generated test file.txt";
        private string secondTestFilePath = @"..\..\..\..\TestFiles\second generated test file.txt";

        public TestFileOperations()
        {
            using (var fs = new FileStream(testFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 0))
                fs.Write(new byte[0], 0, 0);
        }

        public async Task<FileInfo> CreateTestFileAsync()
        {
            using (var fs = new FileStream(secondTestFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 0))
                await fs.WriteAsync(new byte[0], 0, 0);

            return new FileInfo(secondTestFilePath);
        }

        public async Task<FileInfo> ChangeTestFile()
        {
            string text = "text";
            var stringBytes = Encoding.UTF8.GetBytes(text);
            using (var fs = new FileStream(testFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, 0))
                await fs.WriteAsync(stringBytes, 0, stringBytes.Length);

            return new FileInfo(testFilePath);
        }

        public void RenameTestFile()
        {
            var renamedTestFilePath = @"..\..\..\..\TestFiles\renamed second generated test file.txt";
            File.Move(secondTestFilePath, renamedTestFilePath);
            secondTestFilePath = renamedTestFilePath;
        }

        public FileInfo DeleteTestFile()
        {
            File.Delete(secondTestFilePath);
            return new FileInfo(secondTestFilePath);
        }

        public void DeleteAllTestFiles()
        {
            File.Delete(testFilePath);
            File.Delete(secondTestFilePath);
        }
    }
}