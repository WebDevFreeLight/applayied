# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app

 


EXPOSE 8887 
#EXPOSE 8888





# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
 
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["ZR.Admin.WebApi/ZR.Admin.WebApi.csproj", "ZR.Admin.WebApi/"]
COPY ["ZR.CodeGenerator/ZR.CodeGenerator.csproj", "ZR.CodeGenerator/"]
COPY ["Infrastructure/ZR.Infrastructure.csproj", "Infrastructure/"]
COPY ["ZR.Common/ZR.Common.csproj", "ZR.Common/"]
COPY ["ZR.ServiceCore/ZR.ServiceCore.csproj", "ZR.ServiceCore/"]
COPY ["ZR.Repository/ZR.Repository.csproj", "ZR.Repository/"]
COPY ["ZR.Model/ZR.Model.csproj", "ZR.Model/"]
COPY ["ZR.Mall/ZR.Mall.csproj", "ZR.Mall/"]
COPY ["ZR.Service/ZR.Service.csproj", "ZR.Service/"]
COPY ["ZR.Tasks/ZR.Tasks.csproj", "ZR.Tasks/"]
COPY ["ZR.Puppeteer/ZR.Puppeteer.csproj", "ZR.Puppeteer/"]
COPY ["ZR.Pdf/ZR.Pdf.csproj", "ZR.Pdf/"]
RUN dotnet restore "./ZR.Admin.WebApi/ZR.Admin.WebApi.csproj"
COPY . .
WORKDIR "/src/ZR.Admin.WebApi"
RUN dotnet build "./ZR.Admin.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ZR.Admin.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
USER root
WORKDIR /app
COPY --from=publish /app/publish .

# Copy static image files to wwwroot/images
COPY ZR.Admin.WebApi/wwwroot/images /app/wwwroot/images

# Create upload, export, certs, and secure-uploads directories with proper permissions
RUN mkdir -p /app/wwwroot/upload /app/wwwroot/export /app/wwwroot/images /app/certs /app/secure-uploads && \
    chown -R $APP_UID:$APP_UID /app/wwwroot /app/certs /app/secure-uploads && \
    chmod -R 755 /app/wwwroot && \
    chmod -R 755 /app/certs && \
    chmod -R 755 /app/secure-uploads && \
    chmod -R 644 /app/certs/* 2>/dev/null || true







RUN apt-get update && apt-get install -y --allow-unauthenticated libgdiplus
RUN apt-get install xvfb libfontconfig wkhtmltopdf libc6-dev openssl libssl-dev wget -y
RUN apt-get install libxrender1 libfontconfig1 libxext-dev -y

RUN apt-get install -y ghostscript



# Install fontconfig library and Arabic fonts for PDF export
RUN apt-get update && apt-get install -y --no-install-recommends fontconfig fonts-noto fonts-dejavu && apt-get clean && rm -rf /var/lib/apt/lists/*

# Refresh font cache
RUN fc-cache -fv

# Verify Arabic font installation
RUN fc-list | grep -i arabic || echo "No Arabic fonts found"




# Install required dependencies for Chromium
RUN apt-get update && \
    apt-get install -y \
    ca-certificates \
    fonts-liberation \
    libappindicator3-1 \
    libasound2 \
    libatk-bridge2.0-0 \
    libatk1.0-0 \
    libc6 \
    libcairo2 \
    libcups2 \
    libdbus-1-3 \
    libexpat1 \
    libfontconfig1 \
    libgbm1 \
    libgcc1 \
    libglib2.0-0 \
    libgtk-3-0 \
    libnspr4 \
    libnss3 \
    libpango-1.0-0 \
    libpangocairo-1.0-0 \
    libstdc++6 \
    libx11-6 \
    libx11-xcb1 \
    libxcb1 \
    libxcomposite1 \
    libxcursor1 \
    libxdamage1 \
    libxext6 \
    libxfixes3 \
    libxi6 \
    libxrandr2 \
    libxrender1 \
    libxss1 \
    libxtst6 \
    lsb-release \
    wget \
    xdg-utils \
    && rm -rf /var/lib/apt/lists/*




#RUN apt-get install -y google-chrome-stable

# Copy the SSL certificate (if it exists)
# COPY aspnetcore.pfx .

# Switch back to non-root user
USER $APP_UID
ENTRYPOINT ["dotnet", "ZR.Admin.WebApi.dll"]