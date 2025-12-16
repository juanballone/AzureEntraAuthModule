# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files and restore
COPY AzureEntraAuth/AzureEntraAuth.csproj AzureEntraAuth/
COPY TestWebApp/TestWebApp.csproj TestWebApp/
RUN dotnet restore TestWebApp/TestWebApp.csproj

# Copy everything and publish
COPY . .
RUN dotnet publish TestWebApp/TestWebApp.csproj -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Listen on port 8080 inside the container
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TestWebApp.dll"]