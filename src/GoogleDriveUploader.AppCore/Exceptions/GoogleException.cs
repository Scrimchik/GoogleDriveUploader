namespace GoogleDriveUploader.AppCore.Exceptions
{
    public class GoogleException : Exception
    {
        public GoogleException(string message, Exception exception) : base(message, exception)
        { }
    }
}
