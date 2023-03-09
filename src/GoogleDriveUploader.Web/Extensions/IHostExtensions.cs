using GoogleDriveUploader.GoogleAccess.Auth;
using System.Diagnostics;
using System.Reflection;

namespace GoogleDriveUploader.Web.Extensions
{
    public static class IHostExtensions
    {
        public static void AuthorizeGoogleDrive(this IHost host) =>
            GoogleAuthenticator.GenerateGoogleDriveCredentials(new CancellationTokenSource(TimeSpan.FromSeconds(60)).Token).Wait();
        
        public static void CreateAndStartWindowsService(this IHost host)
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory + Assembly.GetExecutingAssembly().GetName().Name + ".exe";
            var serviceName = "GoogleDriveUploader.Service";
            Process.Start("sc", String.Format("create \"{0}\" binPath=\"{1}\" start=\"auto\"", serviceName, baseDirectory));
            Process.Start("sc", serviceName);
        }
    }
}
