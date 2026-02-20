using System.IO;
using Contacts.Data;
using Contacts.Data.SqlServer;
using Contacts.Domain.Interfaces;
using Contacts.Logic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.MSSqlServer;
using WebApplication = Microsoft.AspNetCore.Builder.WebApplication;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);

var app = builder.Build();

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.MapOpenApi();
    app.MapScalarApiReference("/scalar");
}

ConfigureMiddleware(app);
app.Run();


void ConfigureServices(IServiceCollection services)
{
    services.AddApplicationInsightsTelemetry();

    // Configure the logger
    var fullyQualifiedLogFile = Path.Combine(builder.Environment.ContentRootPath, "logs\\logs.txt");
    ConfigureLogging(builder.Configuration, builder.Services, fullyQualifiedLogFile, "Web");
    
    services.AddRazorPages();
    services.AddControllers();
    services.AddCors();

    services.AddEndpointsApiExplorer();
    // Configure OpenAPI
    // Learn more about configuring OpenAPI at https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/overview
    // With help from https://hals.app/blog/dotnet-openapi-scalar-oauth2/
    builder.Services.AddOpenApi(options =>
    {
        options.AddDocumentTransformer<Contacts.Api.XmlDocumentTransformer>();
    });
            
    services.AddTransient<IContactDataStore, SqlServerDataStore>();
    //services.AddTransient<IContactDataStore, SqliteDataStore>();
    services.AddTransient<IContactRepository, ContactRepository>();
    services.AddTransient<IContactManager, ContactManager>();
}

void ConfigureMiddleware(IApplicationBuilder applicationBuilder)
{
            
    // TODO: Research and document for React Native client
    applicationBuilder.UseCors(x => x
        .AllowAnyMethod()
        .AllowAnyHeader()
        .WithOrigins("https://localhost:44311", "https://localhost:5001", "https://cwjg-contacts-web.azurewebsites.net"));

    applicationBuilder.UseHttpsRedirection();
    applicationBuilder.UseRouting();
        
    applicationBuilder.UseEndpoints(endpoints =>
    {
        endpoints.MapControllers(); // Map attribute-routed API controllers
        endpoints.MapRazorPages();//map razor pages
    });
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
                    configurationRoot["APPLICATIONINSIGHTS_CONNECTION_STRING"],
            configureApplicationInsightsLoggerOptions: (_) => { });
        loggingBuilder.AddApplicationInsights();
        loggingBuilder.AddSerilog(logger);
    });
}
