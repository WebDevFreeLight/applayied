#!/bin/bash

# 🔒 SSL Certificate Setup Script for Maham Backend
# This script creates a self-signed SSL certificate and configures HTTPS

set -e

echo "🔒 Setting up SSL certificate for Maham Backend..."

# Create certificates directory
CERT_DIR="/root/mahamBackendNET/backend/certs"
mkdir -p $CERT_DIR
cd $CERT_DIR

echo "📁 Certificate directory: $CERT_DIR"

# Get server hostname/IP
SERVER_HOST=$(hostname -I | awk '{print $1}')
echo "🌐 Server IP: $SERVER_HOST"

# Create self-signed certificate
echo "🔐 Creating self-signed SSL certificate..."
openssl req -x509 -nodes -days 365 -newkey rsa:2048 \
    -keyout aspnetcore.key \
    -out aspnetcore.crt \
    -subj "/CN=$SERVER_HOST" \
    -addext "subjectAltName=DNS:$SERVER_HOST,IP:$SERVER_HOST,IP:127.0.0.1"

# Convert to PFX format
echo "🔄 Converting certificate to PFX format..."
echo "Enter a password for the certificate (remember this!):"
read -s CERT_PASSWORD

openssl pkcs12 -export \
    -out aspnetcore.pfx \
    -inkey aspnetcore.key \
    -in aspnetcore.crt \
    -passout pass:"$CERT_PASSWORD"

# Set proper permissions
chmod 600 aspnetcore.key
chmod 600 aspnetcore.pfx
chmod 644 aspnetcore.crt

echo "✅ SSL certificate created successfully!"

# Create docker-compose with HTTPS
cd /root/mahamBackendNET/backend

echo "📝 Creating HTTPS docker-compose configuration..."
cat > docker-compose.https.yml << EOF
services:
  # HTTPS deployment with SSL certificate
  zradminwebapi:
    image: zradminwebapi:latest
    container_name: maham_backend
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=https://+:8885
      - ASPNETCORE_Kestrel__Certificates__Default__Path=/certs/aspnetcore.pfx
      - ASPNETCORE_Kestrel__Certificates__Default__Password=$CERT_PASSWORD
      - DOTNET_SYSTEM_NET_DISABLEIPV6=true
    ports:
      - "8885:8885"
    volumes:
      - ./certs:/certs:ro
    networks:
      - maham_network
    extra_hosts:
      - "db.ctlaeyiulnukpoxuztla.supabase.co:34.82.126.136"
    restart: unless-stopped

  # Local PostgreSQL
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

# Create deployment script for HTTPS
echo "📝 Creating HTTPS deployment script..."
cat > deploy-https.sh << EOF
#!/bin/bash

echo "🔒 Deploying Maham Backend with HTTPS..."

# Stop and remove existing container
docker stop maham_backend 2>/dev/null || true
docker rm maham_backend 2>/dev/null || true

# Remove old image
docker rmi zradminwebapi:latest 2>/dev/null || true

# Build new image
echo "🔨 Building Docker image..."
docker build -t zradminwebapi:latest -f ZR.Admin.WebApi/Dockerfile .

if [ \$? -ne 0 ]; then
  echo "❌ Docker build failed"
  exit 1
fi

# Run new container with HTTPS
echo "🚀 Starting container with HTTPS..."
docker run -d \\
  --name maham_backend \\
  --restart unless-stopped \\
  -p 8885:8885 \\
  -e ASPNETCORE_ENVIRONMENT=Production \\
  -e ASPNETCORE_URLS=https://+:8885 \\
  -e ASPNETCORE_Kestrel__Certificates__Default__Path=/certs/aspnetcore.pfx \\
  -e ASPNETCORE_Kestrel__Certificates__Default__Password=$CERT_PASSWORD \\
  -e DOTNET_SYSTEM_NET_DISABLEIPV6=true \\
  -v \$(pwd)/certs:/certs:ro \\
  --network maham_network \\
  --add-host db.ctlaeyiulnukpoxuztla.supabase.co:34.82.126.136 \\
  zradminwebapi:latest

if [ \$? -ne 0 ]; then
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

echo "✅ HTTPS deployment completed successfully!"
echo "🔒 Application URL: https://$SERVER_HOST:8885"
echo "📖 Swagger UI: https://$SERVER_HOST:8885/swagger"
EOF

chmod +x deploy-https.sh

# Create health check for HTTPS
echo "📝 Creating HTTPS health check script..."
cat > health-check-https.sh << EOF
#!/bin/bash

echo "🏥 Health check for Maham Backend (HTTPS)..."

# Check if container is running
if docker ps | grep -q maham_backend; then
    echo "✅ Container is running"
    
    # Try HTTPS endpoints
    if curl -f -k -m 10 https://localhost:8885/api/health 2>/dev/null; then
        echo "✅ Health endpoint responding (HTTPS)"
    elif curl -f -k -m 10 https://localhost:8885/health 2>/dev/null; then
        echo "✅ Health endpoint responding (HTTPS)"
    elif curl -f -k -m 10 https://localhost:8885/swagger 2>/dev/null; then
        echo "✅ Swagger endpoint responding (HTTPS)"
    elif curl -f -k -m 10 https://localhost:8885/ 2>/dev/null; then
        echo "✅ Root endpoint responding (HTTPS)"
    else
        echo "❌ Application is not responding to HTTPS endpoints"
        docker logs maham_backend --tail 30
        exit 1
    fi
else
    echo "❌ Container is not running"
    exit 1
fi

echo "✅ HTTPS health check passed!"
echo "🔒 Application is accessible at: https://$SERVER_HOST:8885"
EOF

chmod +x health-check-https.sh

echo ""
echo "✅ SSL setup completed successfully!"
echo ""
echo "📁 Certificate files created in: $CERT_DIR"
echo "🔑 Certificate password: $CERT_PASSWORD"
echo ""
echo "🚀 To deploy with HTTPS:"
echo "   ./deploy-https.sh"
echo ""
echo "🏥 To check HTTPS health:"
echo "   ./health-check-https.sh"
echo ""
echo "🔒 Your application will be available at:"
echo "   https://$SERVER_HOST:8885"
echo ""
echo "⚠️  Note: This is a self-signed certificate."
echo "   Browsers will show a security warning."
echo "   Click 'Advanced' and 'Proceed' to access your application."
