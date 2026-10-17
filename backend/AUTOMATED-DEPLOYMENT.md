# 🚀 100% Automated Deployment Guide

## ✅ **Complete Automation Setup**

This guide ensures your deployment works 100% automatically every time you push to GitHub.

## 🔧 **Step 1: Server Setup (One-time)**

### **1.1 Run Server Setup Script**
```bash
# SSH to your server
ssh root@72.60.193.93

# Download and run the setup script
curl -fsSL https://raw.githubusercontent.com/mahamtechnologies/mahamBackendNET/main/backend/setup-server.sh | bash
```

### **1.2 Verify Setup**
```bash
cd /root/mahamBackendNET/backend
ls -la
# Should show: deploy.sh, health-check.sh, logs.sh, cleanup.sh, docker-compose.prod.yml
```

## 🔑 **Step 2: GitHub Repository Secrets**

### **2.1 Add Repository Secrets**
Go to your GitHub repository → Settings → Secrets and variables → Actions

Add these secrets:

| Secret Name | Value | Description |
|-------------|-------|-------------|
| `SERVER_HOST` | `72.60.193.93` | Your server IP address |
| `SERVER_USER` | `root` | SSH username |
| `SERVER_SSH_KEY` | Your private SSH key | SSH private key for server access |

### **2.2 Generate SSH Key (if needed)**
```bash
# On your local machine
ssh-keygen -t rsa -b 4096 -C "github-actions@maham.com"
# Copy the private key content to SERVER_SSH_KEY secret
# Add the public key to your server's ~/.ssh/authorized_keys
```

## 🚀 **Step 3: Automated Deployment**

### **3.1 Push to Main Branch**
```bash
git add .
git commit -m "🚀 Automated deployment ready"
git push origin main
```

### **3.2 Monitor Deployment**
- Go to GitHub → Actions tab
- Watch the "🚀 Auto Deploy to Production" workflow
- Check the deployment summary

## 📋 **What Happens Automatically**

### **✅ Build Process**
1. ✅ Checkout code from main branch
2. ✅ Setup .NET 8.0 environment
3. ✅ Build application in Release mode
4. ✅ Create deployment package

### **✅ Deployment Process**
1. ✅ Connect to server via SSH
2. ✅ Stop and remove old containers
3. ✅ Build new Docker image
4. ✅ Start new container with HTTP configuration
5. ✅ Wait for container to be healthy
6. ✅ Verify container is running

### **✅ Health Check Process**
1. ✅ Check if container is running
2. ✅ Test multiple endpoints:
   - `/api/health`
   - `/health`
   - `/swagger`
   - `/` (root)
3. ✅ Generate deployment summary
4. ✅ Report success/failure

## 🌐 **Access Your Application**

### **Production URL**
```
http://72.60.193.93:8080
```

### **Available Endpoints**
- **API**: `http://72.60.193.93:8080/api/`
- **Swagger**: `http://72.60.193.93:8080/swagger`
- **Health**: `http://72.60.193.93:8080/health`

## 🔍 **Monitoring & Troubleshooting**

### **Check Deployment Status**
```bash
# SSH to server
ssh root@72.60.193.93
cd /root/mahamBackendNET/backend

# Check container status
docker ps

# Check logs
./logs.sh

# Health check
./health-check.sh
```

### **Manual Deployment (if needed)**
```bash
cd /root/mahamBackendNET/backend
./deploy.sh
```

### **Cleanup (if needed)**
```bash
cd /root/mahamBackendNET/backend
./cleanup.sh
```

## 🎯 **Success Indicators**

### **✅ GitHub Actions Success**
- All steps show green checkmarks
- Deployment summary shows success
- Health check passes

### **✅ Server Success**
- Container `maham_backend` is running
- Application responds on port 8080
- No error logs

### **✅ Application Success**
- Swagger UI accessible
- API endpoints responding
- Health checks passing

## 🚨 **Troubleshooting**

### **If Deployment Fails**
1. Check GitHub Actions logs
2. SSH to server and run `./logs.sh`
3. Check container status: `docker ps`
4. Verify network: `docker network ls`

### **Common Issues**
- **Container name conflict**: Run `./cleanup.sh` then redeploy
- **Port already in use**: Check what's using port 8080
- **Build failures**: Check Dockerfile and dependencies
- **Network issues**: Verify `maham_network` exists

## 🔄 **Rollback (if needed)**
```bash
# SSH to server
ssh root@72.60.193.93
cd /root/mahamBackendNET/backend

# Stop current deployment
docker stop maham_backend
docker rm maham_backend

# Revert to previous commit in GitHub
# Push the reverted commit to trigger new deployment
```

## 📊 **Deployment Summary**

Every successful deployment will show:
- ✅ **Status**: Successfully deployed to production
- 🌐 **URL**: http://72.60.193.93:8080
- 📅 **Deployed at**: Current timestamp
- 🔗 **Commit**: GitHub commit SHA
- 📋 **Health Check Results**: All checks passed

## 🎉 **You're All Set!**

Now every time you push to the `main` branch:
1. ✅ GitHub Actions automatically triggers
2. ✅ Code is built and tested
3. ✅ Application is deployed to production
4. ✅ Health checks verify everything works
5. ✅ You get a deployment summary

**No manual intervention required!** 🚀
