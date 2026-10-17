#!/bin/bash

echo "🔍 Checking Maham Backend deployment status..."

# Check if container is running
echo "📊 Container Status:"
docker ps -a | grep maham_backend

echo ""
echo "📝 Recent Container Logs:"
docker logs maham_backend --tail 50

echo ""
echo "🌐 Network Status:"
docker network ls | grep maham_network

echo ""
echo "🔌 Port Status:"
netstat -tlnp | grep 8887 || ss -tlnp | grep 8887 || echo "Port 8887 not listening"

echo ""
echo "💾 Disk Space:"
df -h

echo ""
echo "🐳 Docker System Info:"
docker system df

echo ""
echo "📋 Container Details:"
docker inspect maham_backend --format='{{.State.Status}} - {{.State.Health.Status}} - Started: {{.State.StartedAt}}'

echo ""
echo "🔧 Environment Variables:"
docker exec maham_backend env | grep -E "(ASPNETCORE|DOTNET)" || echo "Cannot access container environment"

echo ""
echo "🌍 Application Health Check:"
curl -f http://localhost:8887/health 2>/dev/null || curl -f http://localhost:8887/api/health 2>/dev/null || echo "Health endpoints not responding"

echo ""
echo "✅ Status check completed!"
