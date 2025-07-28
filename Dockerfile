# Build the React webapp
FROM node:20 AS web-build
WORKDIR /app/webapp
COPY webapp/package*.json ./
RUN npm ci
COPY webapp/ .
RUN npm run build

# Build the .NET API including the web assets
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY DocQualityChecker/DocQualityChecker.csproj DocQualityChecker/
COPY DocQualityChecker.Api/DocQualityChecker.Api.csproj DocQualityChecker.Api/
COPY DocQualityChecker.Tests/DocQualityChecker.Tests.csproj DocQualityChecker.Tests/
RUN dotnet restore DocQualityChecker.Api/DocQualityChecker.Api.csproj
COPY . .
COPY --from=web-build /app/webapp/dist ./DocQualityChecker.Api/wwwroot
RUN dotnet publish DocQualityChecker.Api/DocQualityChecker.Api.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DocQualityChecker.Api.dll"]
