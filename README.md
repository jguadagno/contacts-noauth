# Sample Application

This repository is being used for several presentations. It's based off of the [contacts](https://www.github.com/jguadagno/contacts) repository.

## Software Tools

You'll need to have at least one of the following Integrated Development Environments (IDEs) installed:

* [Visual Studio](https://visualstudio.microsoft.com/) [Code](https://code.visualstudio.com/?wt.mc_id=DX_841432) ([Mac](https://code.visualstudio.com/docs/?dv=osx) / [Windows](https://code.visualstudio.com/?wt.mc_id=DX_841432#))
* [Visual Studio](https://visualstudio.microsoft.com/) Community ([Windows](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&rel=16))
* [JetBrains](https://www.jetbrains.com/) [Rider](https://www.jetbrains.com/rider/) ([Mac](https://www.jetbrains.com/rider/download/download-thanks.html/) / [Windows](https://www.jetbrains.com/rider/download/download-thanks.html))

## Databases

Currently, the application supports two databases:

* [Microsoft SQL Server](#microsoft-sql-server)
* [SQLite](#sqlite)

### Microsoft SQL Server

To use Microsoft SQL Server, you will need to install [Microsoft SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) for local development.
See the file [./db-scripts/sqlserver/readme.md](./db-scripts/sqlserver/readme.md) for steps to set up the database.

To configure the application to use Microsoft SQL Server,
update the `ConnectionStrings` `ContactsDatabaseSqlServer` setting in the *Contacts.Api* project's *appsettings.json* file(s) with the connection string to your instance of Microsoft SQL Server.

The connection string is in the format of `Server=<server>;Database=<database>;User Id=<user>;Password=<password>;`.

Example:

```json
{
  "ConnectionStrings": {
    "ContactsDatabaseSqlServer": "Server=localhost;Database=Contacts;User Id=contacts_user;Password=Password123!;"
  }
}
```

***NOTE:*** You may need to add the `encrypt=false` to the connection string if you are using a local instance of Microsoft SQL Server.

The `appsettings.json` file(s) are located in the *./src/Contacts.Api* folder.

Files:

* [appsettings.Development.json](./src/Contacts.Api/appsettings.Development.json)
* [appsettings.json](./src/Contacts.Api/appsettings.json)

In the [Program.cs](./src/Contacts.Api/Program.cs) file of the *./src/Contacts.Api* project, you will need to uncomment the following line:

```csharp
builder.Services.AddTransient<IContactDataStore, SqlServerDataStore>();
```

and comment the following line:

```csharp
builder.Services.AddTransient<IContactDataStore, SqliteDataStore>();
```

### SQLite

To use SQLite, you will need to install [SQLite](https://www.sqlite.org/index.html) for local development. 
See the file [/db-scripts/sqlite/readme.md](./db-scripts/sqlite/readme.md) for steps to set up the database.

To configure the application to use Microsoft SQL Server,
update the `ConnectionStrings` `ContactsDatabaseSqlServer` setting in the *Contacts.Api* project's *appsettings.json* file(s) with the connection string to your instance of Microsoft SQL Server.

The connection string is in the format of `Data Source=C:\databases\contacts.db;`.

Example:

```json
{
  "ConnectionStrings": {
    "ContactsDatabaseSqlite": "Data Source=<fully_qualified_path_and_filename>;"
  }
}
```

The `appsettings.json` file(s) are located in the *./src/Contacts.Api* folder.

Files:

* [appsettings.Development.json](./src/Contacts.Api/appsettings.Development.json)
* [appsettings.json](./src/Contacts.Api/appsettings.json)

In the [Program.cs](./src/Contacts.Api/Program.cs) file of the *./src/Contacts.Api* project, you will need to uncomment the following line:

```csharp
builder.Services.AddTransient<IContactDataStore, SqliteDataStore>();
```

and comment the following line:

```csharp
builder.Services.AddTransient<IContactDataStore, SqlServerDataStore>();
```
## Web Application

You may need to update the `ApiRootUri` setting in the *Contacts.Web* project's [appsettings.Development.json](./src/Contacts.WebUi/appsettings.Development.json) and/or [appsettings.json](./src/Contacts.WebUi/appsettings.json) file(s) with the URL to your instance of the *Contacts.Api* project.

If you're running locally, with no changes to the Api project startup options,
the setting should be "https://localhost:5901/" (remember the trailing slash).  
Otherwise, set the URL and port to whatever you configured.
