FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine3.18-amd64 AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./src/WebApi/WebApi.csproj" --disable-parallel
RUN dotnet publish "./src/WebApi/WebApi.csproj" -c release -o /app --no-restore

# Server Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine3.18-amd64 AS runtime
WORKDIR /app
COPY --from=build /app ./

EXPOSE 8000
ENV ASPNETCORE_URLS=http://*:8000

ENTRYPOINT ["dotnet", "WebApi.dll"]