using GoogleDriveUploader.Worker.Configuration;
using Microsoft.AspNetCore.Builder;
using Serilog;

namespace GoogleDriveUploader.Worker.Configuration
{
    public static class ConfigureSeriLog
    {
        public static void AddSerilog(this WebApplicationBuilder builder)
        {
            var loggerConfig = new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).Enrich.FromLogContext();
            loggerConfig.WriteTo.File(AppContext.BaseDirectory + @"\Logs\GoogleDriveUploaderLogs.log");
            var logger = loggerConfig.CreateLogger();
            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog(logger);
        }
    }
}
