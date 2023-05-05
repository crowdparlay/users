﻿FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
EXPOSE 8080

COPY /src .
RUN dotnet restore "CrowdParlay.Users.Api/CrowdParlay.Users.Api.csproj"
RUN dotnet publish "CrowdParlay.Users.Api/CrowdParlay.Users.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "CrowdParlay.Users.Api.dll"]