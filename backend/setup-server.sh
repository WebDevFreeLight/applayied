#!/bin/bash

# 🚀 Maham Backend Server Setup Script
# This script sets up the server for automated GitHub Actions deployment

set -e

echo "🚀 Setting up Maham Backend server for automated deployment..."

# Update system
echo "📦 Updating system packages..."
apt update && apt upgrade -y

# Install Docker if not installed
if ! command -v docker &> /dev/null; then
    echo "🐳 Installing Docker..."
    curl -fsSL https://get.docker.com -o get-docker.sh
    sh get-docker.sh
    usermod -aG docker $USER
    rm get-docker.sh
fi

# Install Docker Compose if not installed
if ! command -v docker-compose &> /dev/null; then
    echo "🐳 Installing Docker Compose..."
    curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
    chmod +x /usr/local/bin/docker-compose
fi

# Create project directory
PROJECT_DIR="/root/mahamBackendNET/backend"
echo "📁 Creating project directory: $PROJECT_DIR"
mkdir -p $PROJECT_DIR
cd $PROJECT_DIR

# Create docker network
echo "🌐 Creating Docker network..."
docker network create maham_network 2>/dev/null || echo "Network maham_network already exists"

# Create production docker-compose file
echo "📝 Creating production docker-compose configuration..."
cat > docker-compose.prod.yml << 'EOF'
services:
  # Production deployment - HTTP only (SSL termination handled by reverse proxy)
  zradminwebapi:
    image: zradminwebapi:latest
    container_name: maham_backend
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8080
      - DOTNET_SYSTEM_NET_DISABLEIPV6=true
    ports:
      - "8080:8080"
    networks:
      - maham_network
    extra_hosts:
      - "db.ctlaeyiulnukpoxuztla.supabase.co:34.82.126.136"
    restart: unless-stopped

  # Local PostgreSQL for development
  postgres:
    image: postgres:15-alpine
    container_name: maham_postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres123
      POSTGRES_DB: MahamDB
      POSTGRES_HOST_AUTH_METHOD: "md5"
    ports:
      - "0.0.0.0:5433:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    command: 
      - "postgres"
      - "-c"
      - "listen_addresses=*"
      - "-c"
      - "max_connections=200"
      - "-c"
      - "shared_buffers=256MB"
      - "-c"
      - "log_connections=on"
    networks:
      - maham_network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 10s
      timeout: 5s
      retries: 5
    restart: unless-stopped

volumes:
  postgres_data:

networks:
  maham_network:
    driver: bridge
EOF

# Create deployment script
echo "📝 Creating deployment script..."
cat > deploy.sh << 'EOF'
#!/bin/bash

echo "🔄 Deploying Maham Backend..."

# Stop and remove existing container
docker stop maham_backend 2>/dev/null || true
docker rm maham_backend 2>/dev/null || true

# Remove old image
docker rmi zradminwebapi:latest 2>/dev/null || true

# Build new image
echo "🔨 Building Docker image..."
docker build -t zradminwebapi:latest -f ZR.Admin.WebApi/Dockerfile .

if [ $? -ne 0 ]; then
  echo "❌ Docker build failed"
  exit 1
fi

# Run new container
echo "🚀 Starting container..."
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

if [ $? -ne 0 ]; then
  echo "❌ Container start failed"
  exit 1
fi

# Wait for container to start
echo "⏳ Waiting for container to start..."
sleep 30

# Check container status
echo "📊 Container status:"
docker ps

# Check if container is running
if ! docker ps | grep -q maham_backend; then
  echo "❌ Container is not running"
  docker logs maham_backend --tail 50
  exit 1
fi

echo "📝 Recent logs:"
docker logs maham_backend --tail 30

echo "✅ Deployment completed successfully!"
echo "🌐 Application URL: http://$(hostname -I | awk '{print $1}'):8080"
EOF

# Make deploy script executable
chmod +x deploy.sh

# Create health check script
echo "📝 Creating health check script..."
cat > health-check.sh << 'EOF'
#!/bin/bash

echo "🏥 Health check for Maham Backend..."

# Check if container is running
if docker ps | grep -q maham_backend; then
    echo "✅ Container is running"
    
    # Try multiple endpoints
    if curl -f -m 10 http://localhost:8080/api/health 2>/dev/null; then
        echo "✅ Health endpoint responding"
    elif curl -f -m 10 http://localhost:8080/health 2>/dev/null; then
        echo "✅ Health endpoint responding"
    elif curl -f -m 10 http://localhost:8080/swagger 2>/dev/null; then
        echo "✅ Swagger endpoint responding"
    elif curl -f -m 10 http://localhost:8080/ 2>/dev/null; then
        echo "✅ Root endpoint responding"
    else
        echo "❌ Application is not responding"
        docker logs maham_backend --tail 30
        exit 1
    fi
else
    echo "❌ Container is not running"
    exit 1
fi

echo "✅ Health check passed!"
EOF

# Make health check script executable
chmod +x health-check.sh

# Create logs script
echo "📝 Creating logs script..."
cat > logs.sh << 'EOF'
#!/bin/bash

echo "📝 Recent logs for Maham Backend:"
docker logs maham_backend --tail 50 -f
EOF

chmod +x logs.sh

# Create cleanup script
echo "📝 Creating cleanup script..."
cat > cleanup.sh << 'EOF'
#!/bin/bash

echo "🧹 Cleaning up Maham Backend..."

# Stop and remove containers
docker stop maham_backend 2>/dev/null || true
docker rm maham_backend 2>/dev/null || true
docker stop maham_postgres 2>/dev/null || true
docker rm maham_postgres 2>/dev/null || true

# Remove images
docker rmi zradminwebapi:latest 2>/dev/null || true

# Remove volumes
docker volume rm backend_postgres_data 2>/dev/null || true

echo "✅ Cleanup completed!"
EOF

chmod +x cleanup.sh

# Test Docker installation
echo "🧪 Testing Docker installation..."
docker --version
docker-compose --version

echo ""
echo "✅ Server setup completed successfully!"
echo ""
echo "📁 Project directory: $PROJECT_DIR"
echo "🚀 To deploy: ./deploy.sh"
echo "🏥 To check health: ./health-check.sh"
echo "📝 To view logs: ./logs.sh"
echo "🧹 To cleanup: ./cleanup.sh"
echo ""
echo "🔧 Next steps:"
echo "1. Configure GitHub repository secrets:"
echo "   - SERVER_HOST: $(hostname -I | awk '{print $1}')"
echo "   - SERVER_USER: root"
echo "   - SERVER_SSH_KEY: Your SSH private key"
echo "2. Push to main branch to trigger automatic deployment"
echo "3. Monitor deployment in GitHub Actions"
