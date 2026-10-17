# 🚀 Maham Backend Deployment Guide

## ✅ **HTTPS Certificate Issue - RESOLVED**

The deployment was failing because the `aspnetcore.pfx` SSL certificate file was not available during the CI/CD build process.

## 🔧 **Solution Implemented**

### **1. Development Environment (Local)**
- **File**: `docker-compose.yml`
- **Protocol**: HTTPS with SSL certificate
- **Port**: 8080
- **Certificate**: `aspnetcore.pfx` (embedded in Docker image)

### **2. Production Environment (Server)**
- **File**: `docker-compose.prod.yml`
- **Protocol**: HTTP (SSL termination handled by reverse proxy)
- **Port**: 8080
- **Certificate**: None required

## 🚀 **Deployment Steps**

### **Option 1: Use Production Docker Compose (Recommended)**
```bash
cd /root/mahamBackendNET/backend
docker compose -f docker-compose.prod.yml up --build -d
```

### **Option 2: Use Deployment Script**
```bash
cd /root/mahamBackendNET/backend
./deploy.sh
```

### **Option 3: Manual Docker Commands**
```bash
# Build image
docker build -t zradminwebapi:latest -f ZR.Admin.WebApi/Dockerfile .

# Run container
docker run -d \
  --name maham_backend \
  --restart unless-stopped \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ASPNETCORE_URLS=http://+:8080 \
  -e DOTNET_SYSTEM_NET_DISABLEIPV6=true \
  --network maham_network \
  --add-host db.ctlaeyiulnukpoxuztla.supabase.co:34.82.126.136 \
  zradminwebapi:latest
```

## 🌐 **Access URLs**

- **Local Development**: `https://localhost:8080` (with HTTPS)
- **Production Server**: `http://YOUR_SERVER_IP:8080` (HTTP only)

## 🔒 **SSL/HTTPS for Production**

For production HTTPS, use a reverse proxy (nginx, Traefik, or cloud load balancer) to handle SSL termination and forward requests to the HTTP container.

## 📋 **Health Check**

```bash
# Check container status
docker ps

# Check logs
docker logs maham_backend --tail 50

# Test application
curl http://localhost:8080/health
```

## ✅ **Expected Result**

- ✅ No more certificate errors
- ✅ Application starts successfully
- ✅ Accessible on port 8080
- ✅ Production-ready deployment
