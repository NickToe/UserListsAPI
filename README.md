# UserLists API
> Web API application designed to fetch items of different content types from external APIs, save them to own database and send the data to UserListsMVC app on request.

## Table of Contents
* [Technologies Used](#technologies-used)
* [General Info](#general-information)
* [Features](#features)
* [Build](#build)
* [Configuration](#configuration)
* [Room for Improvement](#room-for-improvement)
* [Contact](#contact)

## Technologies Used
- ASP.NET Core API
- C# 10
- .NET 6
- EF Core 6
- PostgreSQL

## General Information
This API is not designed to be used as standalone app.</br>
It is designed to meet UserListsMVC app requirements.</br>
This API is responsible for:
1. Saving data from external APIs to own database
2. Send data to UserListsMVC on request 
3. Swagger is supported for testing purposes

## Features
- Get single item by id or title
- Get multiple items by ids or title

## Build
dotnet build --configuration Release

## Configuration
In order for this Web API to function you need to</br>
 - enroll for 'imdb-api.com' and 'api.steampowered.com' api keys.</br>
 - adjust "DefaultConnection" according to your PostgreSQL server settings.</br>
 - adjust "XApiKey" that is the static key of this API (optional).</br>

Below is what default configuration in appsettings.json looks like (Serilog configuration is omitted).
```
    "Movie": {
        "ApiUrl": "https://imdb-api.com/en/API",
        "ApiKey": "APIKEY" // Replace with your API key for imdb-api.com
    },
    "Game": {
        "ApiUrl": "https://api.steampowered.com",
        "ApiKey": "APIKEY" // Replace with your API key for api.steampowered.com
    },
    "ConnectionStrings": {
        // Replace with your connection string for PostgreSQL
        "DefaultConnection": "Host=localhost;Database=UserListsAPI.db;Username=postgres;Password=admin;"
    },
    "UserListsMVC": {
        "Host": "localhost",
        "Port": 5006 // Replace with your port number for UserListsMVC
    },
    "XApiKey": "RANDOM_APIKEY" // Replace with your key for this API
```

## Room for Improvement
- More content types (Music, Books, etc)

## Contact
Created by [@N1ckToe](https://t.me/N1ckToe) - feel free to contact me!
