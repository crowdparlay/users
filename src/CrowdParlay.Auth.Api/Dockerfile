FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/CrowdParlay.Auth.Api/CrowdParlay.Auth.Api.csproj", "CrowdParlay.Auth.Api/"]
RUN dotnet restore "src/CrowdParlay.Auth.Api/CrowdParlay.Auth.Api.csproj"
COPY . .
WORKDIR "/src/CrowdParlay.Auth.Api"
RUN dotnet build "CrowdParlay.Auth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "CrowdParlay.Auth.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CrowdParlay.Auth.Api.dll"]
