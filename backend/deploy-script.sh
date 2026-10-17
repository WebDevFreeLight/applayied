#!/bin/bash

# Maham Backend Deployment Script
# Run this script on the server to set up the initial environment

set -e

echo "🚀 Setting up Maham Backend deployment environment..."

# Create project directory
PROJECT_DIR="/root/mahamBackendNET/backend"
mkdir -p $PROJECT_DIR

# Navigate to project directory
cd $PROJECT_DIR


# Create docker network if it doesn't exist
echo "📦 Creating Docker network..."
docker network create maham_network 2>/dev/null || echo "Network maham_network already exists"

# Create docker-compose override for production
cat > docker-compose.prod.yml << 'EOF'
services:
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

# Create a simple deployment script
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

# Wait for container to be healthy
echo "⏳ Waiting for container to start..."
sleep 10

# Check container status
echo "📊 Container status:"
docker ps
echo "📝 Recent logs:"
docker logs maham_backend --tail 20

echo "✅ Deployment completed!"
echo "🌐 Application should be available at: http://$(hostname -I | awk '{print $1}'):8080"
EOF

# Make deploy script executable
chmod +x deploy.sh

# Create a health check script
cat > health-check.sh << 'EOF'
#!/bin/bash

echo "🏥 Health check for Maham Backend..."

# Check if container is running
if docker ps | grep -q maham_backend; then
    echo "✅ Container is running"
    
    # Check if application is responding
    if curl -f http://localhost:8080/health 2>/dev/null || curl -f http://localhost:8080/api/health 2>/dev/null; then
        echo "✅ Application is responding"
    else
        echo "❌ Application is not responding"
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

# Create a logs script
cat > logs.sh << 'EOF'
#!/bin/bash

echo "📝 Recent logs for Maham Backend:"
docker logs maham_backend --tail 50 -f
EOF

chmod +x logs.sh

echo "✅ Environment setup completed!"
echo ""
echo "📁 Project directory: $PROJECT_DIR"
echo "🚀 To deploy: ./deploy.sh"
echo "🏥 To check health: ./health-check.sh"
echo "📝 To view logs: ./logs.sh"
echo ""
echo "🔧 Next steps:"
echo "1. Copy your source code to this directory"
echo "2. Run ./deploy.sh to deploy"
echo "3. The GitHub Actions workflow will automatically deploy on main branch pushes"
