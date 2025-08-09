var builder = DistributedApplication.CreateBuilder(args);

// Comments out the following 2 lines if you are using Sqlite
var sql = builder.AddSqlServer("SqlServer")   
    .WithLifetime(ContainerLifetime.Persistent);

// Uncomment out is using Sqlite
// var sql = builder.AddSqlite("Sqlite");

var path = builder.AppHostDirectory;
var sqlText = string.Concat(
    File.ReadAllText(Path.Combine(path, @"..\..\db-scripts\sqlserver\create-database-and-user.sql")), 
    " ",
    File.ReadAllText(Path.Combine(path, @"..\..\db-scripts\sqlserver\create-tables.sql")),
    " ",    
    File.ReadAllText(Path.Combine(path, @"..\..\db-scripts\sqlserver\create-data.sql")));

var db = sql.AddDatabase("Contacts")
    .WithCreationScript(sqlText);

var blobStorage = builder.AddAzureStorage("AzureStorage")
    .RunAsEmulator(azurite =>
    {
        azurite.WithLifetime(ContainerLifetime.Persistent);
    });

var imageContainer = blobStorage.AddBlobContainer("contact-images");
var thumbnailContainer = blobStorage.AddBlobContainer("contact-images-thumbnails");


var api = builder.AddProject<Projects.Contacts_Api>("contacts-api")
    .WaitFor(db)
    .WithEnvironment("ConnectionStrings__ContactsDatabaseSqlServer", db);

builder.AddProject<Projects.Contacts_WebUi>("contacts-web-ui")
    .WithReference(api)
    .WaitFor(api)
    .WaitFor(blobStorage)
    .WithEnvironment("Settings__ContactBlobStorageAccount", imageContainer)
    .WithEnvironment("Settings__ContactThumbnailBlobStorageAccountName", thumbnailContainer);
   
builder.Build().Run();

