# PMO Portal — .NET 8 API image.
# IMPORTANT: build with the REPO ROOT as context (so data/csv ships for first-boot seeding):
#   docker build -f backend/Dockerfile -t pmo-api .
# The Render Blueprint (render.yaml) sets dockerContext: . automatically.

# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/ ./backend/
RUN dotnet restore backend/ZR.Admin.WebApi/ZR.Admin.WebApi.csproj
RUN dotnet publish backend/ZR.Admin.WebApi/ZR.Admin.WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish ./
# Synthetic PMO seed package (CSV) imported on first boot when PmoSeed=true.
COPY data/ ./data/
ENV ASPNETCORE_ENVIRONMENT=Production \
    UseHttps=false \
    PmoSeedPath=/app/data/csv
EXPOSE 8080
# Bind the platform's $PORT (Render/Railway/Fly set it) else 8080. HTTP only — the platform does TLS.
ENTRYPOINT ["/bin/sh","-c","ASPNETCORE_URLS=http://0.0.0.0:${PORT:-8080} dotnet ZR.Admin.WebApi.dll"]
