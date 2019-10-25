# Bondora API webhook sample

This is a sample webhook listener made in ASP.NET Core 2.1.
It listens to POST requests on `/api/hook` and saves them to a configured directory.

## Installation

Install [.NET Core 2.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/2.1).
Change into the webhooksite subdir.
Run `dotnet` in the source directory.

```
cd webhooksite
dotnet build
```

It should 

## Configuration

Main configuration options are `Data:DataDir` and `Signature:Keys:<keyId>`.
The values can be added to `appsettings.json` or be set using the environment or command line.

For example, using the command line options:

```
dotnet run -- --Data:DataDir C:\temp\json --Signature:Keys:test-key-01 c2VjcmV0OnRlc3Qta2V5LTAx --server.urls "http://localhost:7500"
```

See [ASP.NET Core documentation](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-2.1) for details.
