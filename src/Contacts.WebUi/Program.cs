using System;
using System.IO;
using System.Linq;
using Azure.Storage.Blobs.Models;
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
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.MSSqlServer;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

ConfigureServices(builder.Configuration, builder.Services, builder.Environment);

var app = builder.Build();
ConfigureMiddleware(app, builder.Environment);

app.MapDefaultEndpoints();

app.Run();

void ConfigureServices(IConfiguration configuration, IServiceCollection services, IWebHostEnvironment environment)
{
    var settings = new Settings();
    configuration.Bind("Settings", settings);

    if (environment.IsDevelopment())
    {
        var endpoint = settings.ContactBlobStorageAccount.Split(';').First(x => x.StartsWith("BlobEndpoint"));
        settings.ContactImageUrl =  endpoint.Split('=')[1].TrimEnd('/') + "/";
        settings.ContactBlobStorageAccount = settings.ContactBlobStorageAccount.Substring(0, settings.ContactBlobStorageAccount.IndexOf(";;", StringComparison.Ordinal));
    }
    
    services.AddSingleton(settings);

    // Configure the logger
    var fullyQualifiedLogFile = Path.Combine(builder.Environment.ContentRootPath, "logs\\logs.txt");
    ConfigureLogging(builder.Configuration, builder.Services, fullyQualifiedLogFile, "Web");
    
    services.AddHttpClient();
    services.TryAddScoped<IContactService, ContactService>();
    services.TryAddScoped<IImageStore>(_ =>
    {
        if (environment.IsDevelopment())
        {
            var blob = new Blobs(settings.ContactBlobStorageAccount, settings.ContactImageContainerName);
            blob.BlobContainerClient.SetAccessPolicy(PublicAccessType.Blob);
            return new ImageStore(blob);
        }

        return new ImageStore(
            new Blobs(settings.ContactBlobStorageAccountName, null, settings.ContactImageContainerName));

    });
    
    services.TryAddScoped<IThumbnailImageStore>(_ =>
    {
        if (environment.IsDevelopment())
        {
            var blob = new Blobs(settings.ContactBlobStorageAccount, settings.ContactThumbnailImageContainerName);
            blob.BlobContainerClient.SetAccessPolicy(PublicAccessType.Blob);
            return new ThumbnailImageStore(blob);
        }

        return new ThumbnailImageStore(
            new Blobs(settings.ContactBlobStorageAccountName, null, settings.ContactThumbnailImageContainerName));

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

void ConfigureMiddleware(WebApplication application, IWebHostEnvironment env)
{
    if (env.IsDevelopment())
    {
        application.UseDeveloperExceptionPage();
    }
    else
    {
        application.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        application.UseHsts();
    }

    application.UseHttpsRedirection();
    application.UseStaticFiles();

    application.UseRouting();

    application.MapDefaultControllerRoute();
    application.MapRazorPages();
}

void ConfigureLogging(IConfigurationRoot configurationRoot, IServiceCollection services, string logPath, string applicationName)
{
    var logger = new LoggerConfiguration()
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithEnvironmentName()
        .Enrich.WithAssemblyName()
        .Enrich.WithAssemblyVersion(true)
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("Application", applicationName)
        .Destructure.ToMaximumDepth(4)
        .Destructure.ToMaximumStringLength(100)
        .Destructure.ToMaximumCollectionCount(10)
        .WriteTo.Console()
        .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
        .WriteTo.MSSqlServer(
            connectionString: configurationRoot.GetConnectionString("ContactsDatabaseSqlServer"),
            sinkOptions: new MSSqlServerSinkOptions
            {
                TableName = "Logs",
                AutoCreateSqlTable = false, 
                AutoCreateSqlDatabase = false
            })
        .WriteTo.OpenTelemetry()
        .CreateLogger();
    services.AddLogging(loggingBuilder =>
    {
        loggingBuilder.AddApplicationInsights(configureTelemetryConfiguration: (config) =>
                config.ConnectionString =
                    configurationRoot["ApplicationInsights:ConnectionString"],
            configureApplicationInsightsLoggerOptions: (_) => { });loggingBuilder.AddApplicationInsights();
        loggingBuilder.AddSerilog(logger);
    });
}