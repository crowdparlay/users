FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /app
EXPOSE 8080

# Copy project files
COPY /src .
# Restore as distinct layers
RUN dotnet restore "CrowdParlay.Users.Api/CrowdParlay.Users.Api.csproj"
# Build and publish a release
RUN dotnet publish "CrowdParlay.Users.Api/CrowdParlay.Users.Api.csproj" -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0
WORKDIR /app
COPY --from=build /app/out .
ENTRYPOINT ["dotnet", "CrowdParlay.Users.Api.dll"]