# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine-amd64 AS build
WORKDIR /source
COPY . .
RUN dotnet restore "./src/WebApi/WebApi.csproj" --disable-parallel
RUN dotnet publish "./src/WebApi/WebApi.csproj" -c release -o /app --no-restore

# Server Stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine-amd64 AS runtime
WORKDIR /app
COPY --from=build /app ./

# Create and use a non-root user
RUN addgroup -S appgroup && adduser -S appuser -G appgroup
USER appuser

# Define a health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:8000/health || exit 1
  
EXPOSE 8000
ENV ASPNETCORE_URLS=http://*:8000

ENTRYPOINT ["dotnet", "WebApi.dll"]