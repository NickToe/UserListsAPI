# UserLists API
> Web API application designed to fetch items of different content types from external APIs, save them to own database and send the data to UserListsMVC app on request.


## Table of Contents
* [Technologies Used](#technologies-used)
* [General Info](#general-information)
* [Features](#features)
* [Build](#build)
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
1. Saving data found in external APIs to own database
2. Send data to UserListsMVC on request 


## Features
- Get single item by id or title
- Get multiple item by matching title


## Build
dotnet build --configuration Release


## Room for Improvement
- More content types (Music, Books, etc)


## Contact
Created by [@N1ckToe](https://web.telegram.org/k/#@N1ckToe) - feel free to contact me!
