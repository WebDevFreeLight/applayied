# ---- build ----
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY VibeVoice.slnx ./
COPY VibeVoice.Domain/VibeVoice.Domain.csproj VibeVoice.Domain/
COPY VibeVoice.Application/VibeVoice.Application.csproj VibeVoice.Application/
COPY VibeVoice.Infrastructure/VibeVoice.Infrastructure.csproj VibeVoice.Infrastructure/
COPY VibeVoice.Api/VibeVoice.Api.csproj VibeVoice.Api/
RUN dotnet restore VibeVoice.Api/VibeVoice.Api.csproj
COPY . .
RUN dotnet publish VibeVoice.Api/VibeVoice.Api.csproj -c Release -o /app --no-restore

# ---- runtime ----
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
# SQLite database + uploaded files live here; mount a volume to keep them.
RUN mkdir -p /app/storage && chown -R app:app /app/storage
VOLUME /app/storage
USER app
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080
ENTRYPOINT ["dotnet", "VibeVoice.Api.dll"]
