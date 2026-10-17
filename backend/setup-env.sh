#!/bin/bash

# Simple Environment Setup Script
# Run this on your server to prepare for deployment

echo "🔧 Setting up deployment environment..."

# Check if Docker is installed
if ! command -v docker &> /dev/null; then
    echo "❌ Docker is not installed. Please install Docker first."
    echo "📖 Installation guide: https://docs.docker.com/engine/install/"
    exit 1
fi

# Check if Docker is running
if ! docker info &> /dev/null; then
    echo "❌ Docker is not running. Please start Docker service."
    exit 1
fi

echo "✅ Docker is available and running"

# Create project directory
PROJECT_DIR="/root/mahamBackendNET/backend"
mkdir -p $PROJECT_DIR
cd $PROJECT_DIR

echo "📁 Project directory: $PROJECT_DIR"

# Create Docker network
echo "🌐 Creating Docker network..."
docker network create maham_network 2>/dev/null || echo "Network maham_network already exists"

# Create .env file for environment variables
cat > .env << 'EOF'
# Maham Backend Environment Variables
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8887
DOTNET_SYSTEM_NET_DISABLEIPV6=true

# Database Configuration (if using local PostgreSQL)
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres123
POSTGRES_DB=MahamDB
POSTGRES_HOST_AUTH_METHOD=md5
EOF

echo "📝 Created .env file"

# Create a simple docker-compose file for local development/testing
cat > docker-compose.local.yml << 'EOF'
version: '3.8'

services:
  zradminwebapi:
    build:
      context: .
      dockerfile: ZR.Admin.WebApi/Dockerfile
    container_name: maham_backend_local
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ASPNETCORE_URLS=http://+:8887
    ports:
      - "8887:8887"
    volumes:
      - ./appsettings.Development.json:/app/appsettings.json:ro
    networks:
      - maham_network

  postgres:
    image: postgres:15-alpine
    container_name: maham_postgres_local
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres123
      POSTGRES_DB: MahamDB
    ports:
      - "5433:5432"
    volumes:
      - postgres_data_local:/var/lib/postgresql/data
    networks:
      - maham_network

volumes:
  postgres_data_local:

networks:
  maham_network:
    external: true
EOF

echo "📝 Created docker-compose.local.yml for local development"

# Create a quick start script
cat > quick-start.sh << 'EOF'
#!/bin/bash

echo "🚀 Quick Start - Maham Backend"

# Check if source code exists
if [ ! -f "ZR.Admin.WebApi/Dockerfile" ]; then
    echo "❌ Source code not found. Please copy your project files to this directory first."
    echo "📁 Expected structure:"
    echo "   - ZR.Admin.WebApi/"
    echo "   - Infrastructure/"
    echo "   - ZR.Common/"
    echo "   - ZR.ServiceCore/"
    echo "   - ZR.Repository/"
    echo "   - ZR.Model/"
    echo "   - ZR.Mall/"
    echo "   - ZR.Service/"
    echo "   - ZR.Tasks/"
    exit 1
fi

echo "✅ Source code found"

# Build and run
echo "🔨 Building and starting application..."
docker-compose -f docker-compose.local.yml up --build -d

echo "⏳ Waiting for application to start..."
sleep 15

# Check status
echo "📊 Application status:"
docker-compose -f docker-compose.local.yml ps

echo "📝 Recent logs:"
docker-compose -f docker-compose.local.yml logs --tail 20

echo "✅ Quick start completed!"
echo "🌐 Application should be available at: http://localhost:8887"
echo ""
echo "📝 To view logs: docker-compose -f docker-compose.local.yml logs -f"
echo "🛑 To stop: docker-compose -f docker-compose.local.yml down"
EOF

chmod +x quick-start.sh

echo "✅ Environment setup completed!"
echo ""
echo "📁 Project directory: $PROJECT_DIR"
echo "🚀 Next steps:"
echo "1. Copy your source code to this directory"
echo "2. Run ./quick-start.sh to test locally"
echo "3. The GitHub Actions workflow will handle production deployments"
echo ""
echo "🔧 Useful commands:"
echo "   - ./quick-start.sh     # Start local development environment"
echo "   - docker ps            # View running containers"
echo "   - docker logs <container> # View container logs"
