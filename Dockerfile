FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY /src .
RUN dotnet restore "CrowdParlay.Users.Api/CrowdParlay.Users.Api.csproj"
RUN dotnet publish "CrowdParlay.Users.Api/CrowdParlay.Users.Api.csproj" -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "CrowdParlay.Users.Api.dll"]
