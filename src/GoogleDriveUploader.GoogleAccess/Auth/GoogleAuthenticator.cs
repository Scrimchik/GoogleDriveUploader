using Google.Apis.Auth.OAuth2;
using Google.Apis.Util.Store;

namespace GoogleDriveUploader.GoogleAccess.Auth
{
    public static class GoogleAuthenticator
    {
        public static async Task<UserCredential> GenerateGoogleDriveCredentials(CancellationToken cancellationToken)
        {
            return await GoogleWebAuthorizationBroker.AuthorizeAsync(
                new ClientSecrets()
                {
                    ClientId = "860977300101-q1jt8b5ri1su6nu2ab0l252hcq6lvr7t.apps.googleusercontent.com",
                    ClientSecret = "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"
                }, new List<string>() { "https://www.googleapis.com/auth/drive.file" }, "user", cancellationToken, 
                new FileDataStore(@$"{AppContext.BaseDirectory}\Creds\", true));
        }
    }
}
