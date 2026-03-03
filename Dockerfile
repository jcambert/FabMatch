# ── Stage 1: Build ─────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files first for layer caching
COPY FabMatch.sln .
COPY src/FabMatch.Domain/FabMatch.Domain.csproj          src/FabMatch.Domain/
COPY src/FabMatch.Application/FabMatch.Application.csproj src/FabMatch.Application/
COPY src/FabMatch.Infrastructure/FabMatch.Infrastructure.csproj src/FabMatch.Infrastructure/
COPY src/FabMatch.Web/FabMatch.Web.csproj                 src/FabMatch.Web/
COPY tests/FabMatch.Tests/FabMatch.Tests.csproj           tests/FabMatch.Tests/

# Restore NuGet packages
RUN dotnet restore

# Copy full source
COPY . .

# Build the web project in Release configuration
RUN dotnet build src/FabMatch.Web/FabMatch.Web.csproj \
    -c Release \
    --no-restore \
    -o /build/out

# ── Stage 2: Publish ────────────────────────────────────────────────────────────
FROM build AS publish
RUN dotnet publish src/FabMatch.Web/FabMatch.Web.csproj \
    -c Release \
    --no-restore \
    -o /app/publish \
    /p:UseAppHost=false

# ── Stage 3: Runtime ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install libgdiplus for PDF rendering (PDFtoImage / Pdfium)
RUN apt-get update && apt-get install -y \
    libgdiplus \
    libfontconfig1 \
    && rm -rf /var/lib/apt/lists/*

# Create directories for uploads and logs
RUN mkdir -p /app/uploads /app/logs && chmod 777 /app/uploads /app/logs

# Copy published output
COPY --from=publish /app/publish .

# Expose HTTP port (HTTPS handled by reverse proxy in production)
EXPOSE 8080

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "FabMatch.Web.dll"]
