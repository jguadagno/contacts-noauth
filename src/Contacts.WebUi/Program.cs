using System;
using Contacts.Domain.Interfaces;
using Contacts.ImageManager;
using Contacts.WebUi.Models;
using Contacts.WebUi.Services;
using JosephGuadagno.AzureHelpers.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using WebApplication = Microsoft.AspNetCore.Builder.WebApplication;

var logger = LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    ConfigureServices(builder.Configuration, builder.Services, builder.Environment);

    var app = builder.Build();
    ConfigureMiddleware(app, builder.Environment);
    app.Run();
}
catch (Exception exception)
{
    // NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    LogManager.Shutdown();
}

void ConfigureServices(IConfiguration configuration, IServiceCollection services, IWebHostEnvironment environment)
{
    var settings = new Settings();
    configuration.Bind("Settings", settings);
    services.AddSingleton(settings);

    services.AddHttpClient();
    services.TryAddScoped<IContactService, ContactService>();
    services.TryAddScoped<IImageStore>(_ =>
    {
        var blobs = environment.IsDevelopment()
            ? new Blobs(settings.ContactBlobStorageAccount, settings.ContactImageContainerName)
            : new Blobs(settings.ContactBlobStorageAccountName, null, settings.ContactImageContainerName);
        return new ImageStore(blobs);
    });
    
    services.TryAddScoped<IThumbnailImageStore>(_ =>
    {
        var blobs = environment.IsDevelopment()
            ? new Blobs(settings.ContactBlobStorageAccount, settings.ContactThumbnailImageContainerName)
            : new Blobs(settings.ContactBlobStorageAccountName, null, settings.ContactThumbnailImageContainerName);
        return new ThumbnailImageStore(blobs);
    });

    services.TryAddScoped<IImageManager, ImageManager>();
    
    // Register Thumbnail Create Queue
    services.TryAddScoped(_ => environment.IsDevelopment()
        ? new Queue(settings.ThumbnailQueueStorageAccount, settings.ThumbnailQueueName)
        : new Queue(settings.ThumbnailQueueStorageAccountName, null, settings.ThumbnailQueueName));
    
    services.AddApplicationInsightsTelemetry();
    services.AddControllersWithViews();
    services.AddRazorPages();
}

void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();

    app.MapDefaultControllerRoute();
    app.MapRazorPages();
}
