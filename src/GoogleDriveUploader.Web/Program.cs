using GoogleDriveUploader.Web.BackgroundJobs;
using GoogleDriveUploader.Web.Extensions;
using GoogleDriveUploader.Worker.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting.WindowsServices;
using Serilog;

WebApplicationOptions options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = WindowsServiceHelpers.IsWindowsService() ? AppContext.BaseDirectory : default
};

var builder = WebApplication.CreateBuilder(options);

builder.Host.UseWindowsService();
builder.Services.AddControllers();
builder.AddSerilog();
builder.Services.AddCoreServices();

if (WindowsServiceHelpers.IsWindowsService())
    builder.Services.AddHostedService<UploaderJob>();

var app = builder.Build();

if (!WindowsServiceHelpers.IsWindowsService())
{
    app.AuthorizeGoogleDrive();
    app.CreateAndStartWindowsService();
    Environment.Exit(0);
}

await app.RunAsync();
