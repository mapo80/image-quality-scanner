# Build the Razor web app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY DocQualityChecker/DocQualityChecker.csproj DocQualityChecker/
COPY DocQualityChecker.Web/DocQualityChecker.Web.csproj DocQualityChecker.Web/
RUN dotnet restore DocQualityChecker.Web/DocQualityChecker.Web.csproj
COPY . .
RUN dotnet publish DocQualityChecker.Web/DocQualityChecker.Web.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "DocQualityChecker.Web.dll"]
