# Synology NAS Docker & Firewall Configuration Guide

## Table of Contents
1. [Overview](#overview)
2. [Synology Firewall Configuration](#synology-firewall-configuration)
3. [Docker Network Setup on Synology](#docker-network-setup-on-synology)
4. [PostgreSQL Database Access](#postgresql-database-access)
5. [Troubleshooting](#troubleshooting)

## Overview

This guide helps you configure your Synology NAS to properly allow Docker containers to communicate and enable remote database access while maintaining security.

## Prerequisites

- Synology NAS with DSM 7.0+ (or DSM 6.2+ for older models)
- Docker package installed from Package Center
- Administrator access to DSM
- SSH access enabled (optional, for advanced configuration)

## 1. Synology Firewall Configuration

### Step 1: Access Firewall Settings

1. Open **DSM (DiskStation Manager)**
2. Go to **Control Panel** → **Security** → **Firewall**
3. Enable the firewall if not already enabled
4. Click **Edit Rules**

### Step 2: Create Docker Network Rules

#### A. Create Profile for Docker

1. Click **Create** to add a new profile
2. Name it: `Docker-Database-Profile`
3. Select source: **All** (or specify IP ranges for security)

#### B. Add Essential Rules

Add these rules in order (priority matters):

| Priority | Ports | Protocol | Source IP | Action | Description |
|----------|-------|----------|-----------|---------|-------------|
| 1 | 22 | TCP | Specific IP/Subnet | Allow | SSH Access (optional) |
| 2 | 5432 | TCP | All/Subnet | Allow | PostgreSQL Default |
| 3 | 5433 | TCP | All/Subnet | Allow | PostgreSQL Docker |
| 4 | 8888 | TCP | All/Subnet | Allow | Backend API |
| 5 | 80,443 | TCP | All | Allow | HTTP/HTTPS |
| 6 | 3306 | TCP | Subnet | Allow | MySQL (if needed) |
| 7 | 6379 | TCP | Subnet | Allow | Redis (if needed) |
| 8 | All | All | Docker Subnet | Allow | Docker Internal |

### Step 3: Configure Docker Subnet Access

```bash
# Default Docker subnets that need access:
172.17.0.0/16  # Docker bridge network
172.18.0.0/16  # Custom bridge networks
10.0.0.0/8     # Docker swarm overlay
```

#### Add Docker Network Rules via DSM:

1. In Firewall rules, click **Create**
2. Configure as follows:

**Rule 1: Docker Bridge Network**
- Ports: Select "All"
- Protocol: All
- Source IP: Specific IP
  - From: `172.17.0.0`
  - Subnet mask: `255.255.0.0`
- Action: Allow

**Rule 2: Custom Docker Networks**
- Ports: Select "All"
- Protocol: All
- Source IP: Specific IP
  - From: `172.18.0.0`
  - Subnet mask: `255.255.0.0`
- Action: Allow

**Rule 3: Docker Overlay Networks**
- Ports: Select "All"
- Protocol: All
- Source IP: Specific IP
  - From: `10.0.0.0`
  - Subnet mask: `255.0.0.0`
- Action: Allow

### Step 4: Apply Profile

1. Select the profile: `Docker-Database-Profile`
2. Click **OK** to apply
3. The firewall will reload with new rules

## 2. Docker Network Setup on Synology

### Via Synology Docker GUI:

1. Open **Docker** package
2. Go to **Network** tab
3. Click **Add** to create network

**Create Backend Network:**
- Network name: `maham_network`
- Driver: `bridge`
- Subnet: `172.20.0.0/16`
- Gateway: `172.20.0.1`
- IP Range: `172.20.0.0/24`

### Via SSH (Advanced):

```bash
# SSH into your Synology
ssh admin@your-synology-ip

# Create custom Docker network
sudo docker network create \
  --driver bridge \
  --subnet=172.20.0.0/16 \
  --gateway=172.20.0.1 \
  --opt com.docker.network.bridge.name=br-maham \
  maham_network

# Verify network
sudo docker network ls
sudo docker network inspect maham_network
```

## 3. Docker Compose Configuration for Synology

Create a Synology-optimized `docker-compose.synology.yml`:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: maham_postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgres123}
      POSTGRES_DB: MahamDB
      # PostgreSQL tuning for Synology NAS
      POSTGRES_INITDB_ARGS: "--encoding=UTF8 --locale=C"
    ports:
      - "0.0.0.0:5433:5432"  # External access on 5433
    volumes:
      # Use Synology volume paths
      - /volume1/docker/maham/postgres/data:/var/lib/postgresql/data
      - /volume1/docker/maham/postgres/config:/etc/postgresql
      - /volume1/docker/maham/postgres/logs:/var/log/postgresql
    command:
      - "postgres"
      - "-c"
      - "listen_addresses=*"
      - "-c"
      - "max_connections=100"  # Adjust based on NAS resources
      - "-c"
      - "shared_buffers=128MB"  # Conservative for NAS
      - "-c"
      - "effective_cache_size=512MB"
      - "-c"
      - "maintenance_work_mem=64MB"
      - "-c"
      - "checkpoint_completion_target=0.9"
      - "-c"
      - "wal_buffers=16MB"
      - "-c"
      - "default_statistics_target=100"
      - "-c"
      - "random_page_cost=1.1"
      - "-c"
      - "effective_io_concurrency=200"
      - "-c"
      - "work_mem=4MB"
      - "-c"
      - "min_wal_size=1GB"
      - "-c"
      - "max_wal_size=4GB"
      - "-c"
      - "log_connections=on"
      - "-c"
      - "log_disconnections=on"
      - "-c"
      - "log_duration=on"
    networks:
      - maham_network
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 30s
      timeout: 10s
      retries: 5
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"

  zradminwebapi:
    image: ${DOCKER_REGISTRY-}zradminwebapi
    build:
      context: .
      dockerfile: ZR.Admin.WebApi/Dockerfile
    container_name: maham_backend
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8888
      - DOTNET_SYSTEM_NET_DISABLEIPV6=true
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=MahamDB;Username=postgres;Password=${POSTGRES_PASSWORD:-postgres123};Pooling=true;
    ports:
      - "0.0.0.0:8888:8888"
    depends_on:
      postgres:
        condition: service_healthy
    volumes:
      - /volume1/docker/maham/app/appsettings:/app/appsettings
      - /volume1/docker/maham/app/wwwroot:/app/wwwroot
      - /volume1/docker/maham/app/logs:/app/logs
    networks:
      - maham_network
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"

networks:
  maham_network:
    external: true  # Use the network created earlier

volumes:
  postgres_data:
    driver: local
    driver_opts:
      type: none
      o: bind
      device: /volume1/docker/maham/postgres/data
```

## 4. Synology-Specific Setup Script

Create `/volume1/docker/maham/setup-synology.sh`:

```bash
#!/bin/bash

# Synology Docker Setup Script
# Run as admin user with sudo

set -e

VOLUME_PATH="/volume1/docker/maham"
DOCKER_COMPOSE_FILE="$VOLUME_PATH/docker-compose.synology.yml"

echo "=== Synology Docker Setup for Maham Backend ==="

# Create directory structure
echo "Creating directory structure..."
sudo mkdir -p $VOLUME_PATH/{postgres/{data,config,logs},app/{appsettings,wwwroot,logs}}

# Set permissions
echo "Setting permissions..."
sudo chown -R 999:999 $VOLUME_PATH/postgres  # PostgreSQL user
sudo chown -R 1000:1000 $VOLUME_PATH/app     # App user

# Create .env file
echo "Creating environment file..."
cat > $VOLUME_PATH/.env <<EOF
# PostgreSQL Configuration
POSTGRES_PASSWORD=postgres123

# Network Configuration
DOCKER_NETWORK=maham_network

# Synology Specific
VOLUME_PATH=/volume1/docker/maham
EOF

# Create PostgreSQL config
echo "Creating PostgreSQL configuration..."
cat > $VOLUME_PATH/postgres/config/postgresql.conf <<EOF
# PostgreSQL Configuration for Synology NAS
listen_addresses = '*'
max_connections = 100
shared_buffers = 128MB
effective_cache_size = 512MB
maintenance_work_mem = 64MB
checkpoint_completion_target = 0.9
wal_buffers = 16MB
default_statistics_target = 100
random_page_cost = 1.1
effective_io_concurrency = 200
work_mem = 4MB
min_wal_size = 1GB
max_wal_size = 4GB

# Logging
logging_collector = on
log_directory = '/var/log/postgresql'
log_filename = 'postgresql-%Y-%m-%d_%H%M%S.log'
log_file_mode = 0600
log_truncate_on_rotation = off
log_rotation_age = 1d
log_rotation_size = 100MB
log_line_prefix = '%m [%p] %q%u@%d '
log_statement = 'all'
log_connections = on
log_disconnections = on
log_duration = on
EOF

# Create pg_hba.conf
echo "Creating PostgreSQL access configuration..."
cat > $VOLUME_PATH/postgres/config/pg_hba.conf <<EOF
# PostgreSQL Client Authentication Configuration
# TYPE  DATABASE        USER            ADDRESS                 METHOD

# Local connections
local   all             all                                     trust

# IPv4 local connections
host    all             all             127.0.0.1/32            trust

# IPv4 Docker networks
host    all             all             172.16.0.0/12           md5
host    all             all             10.0.0.0/8              md5

# IPv4 external connections (adjust subnet as needed)
host    all             all             192.168.0.0/16          md5
host    all             all             0.0.0.0/0               md5

# IPv6 connections
host    all             all             ::1/128                 trust
host    all             all             ::/0                    md5
EOF

# Create Docker network if not exists
echo "Creating Docker network..."
if ! sudo docker network ls | grep -q maham_network; then
    sudo docker network create \
        --driver bridge \
        --subnet=172.20.0.0/16 \
        --gateway=172.20.0.1 \
        maham_network
    echo "Network created successfully"
else
    echo "Network already exists"
fi

echo ""
echo "=== Setup Complete ==="
echo ""
echo "Next steps:"
echo "1. Copy your docker-compose.synology.yml to: $VOLUME_PATH/"
echo "2. Navigate to: $VOLUME_PATH"
echo "3. Run: sudo docker-compose -f docker-compose.synology.yml up -d"
echo ""
echo "Access points:"
echo "  PostgreSQL: your-synology-ip:5433"
echo "  API: http://your-synology-ip:8888"
echo ""
echo "To check logs:"
echo "  sudo docker-compose -f docker-compose.synology.yml logs -f"
```

## 5. Synology Firewall Advanced Configuration

### Using iptables (SSH Required)

```bash
# SSH into Synology
ssh admin@your-synology-ip

# Add custom iptables rules for Docker
# Allow Docker bridge network
sudo iptables -I INPUT -s 172.17.0.0/16 -j ACCEPT
sudo iptables -I INPUT -s 172.18.0.0/16 -j ACCEPT
sudo iptables -I INPUT -s 172.20.0.0/16 -j ACCEPT

# Allow specific ports from any source
sudo iptables -I INPUT -p tcp --dport 5433 -j ACCEPT
sudo iptables -I INPUT -p tcp --dport 8888 -j ACCEPT

# Allow established connections
sudo iptables -I INPUT -m conntrack --ctstate ESTABLISHED,RELATED -j ACCEPT

# Save rules (Synology specific)
sudo iptables-save > /usr/syno/etc/iptables_rules

# Create startup script to restore rules
cat > /usr/local/etc/rc.d/iptables-docker.sh <<'EOF'
#!/bin/bash
# Restore iptables rules on boot
iptables-restore < /usr/syno/etc/iptables_rules
EOF

chmod +x /usr/local/etc/rc.d/iptables-docker.sh
```

## 6. Synology Resource Monitoring

### Monitor Docker Resources

```bash
# Check Docker container stats
sudo docker stats

# Check PostgreSQL connections
sudo docker exec maham_postgres psql -U postgres -c "SELECT count(*) FROM pg_stat_activity;"

# Check disk usage
df -h /volume1/docker/maham

# Check memory usage
free -m

# Monitor network connections
netstat -an | grep -E "5433|8888"
```

### Create Monitoring Script

```bash
#!/bin/bash
# Save as /volume1/docker/maham/monitor.sh

echo "=== Maham Docker Monitoring ==="
echo ""
echo "Container Status:"
sudo docker ps --filter "name=maham" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo ""
echo "Resource Usage:"
sudo docker stats --no-stream --filter "name=maham"

echo ""
echo "PostgreSQL Connections:"
sudo docker exec maham_postgres psql -U postgres -c "SELECT datname, count(*) as connections FROM pg_stat_activity GROUP BY datname;"

echo ""
echo "Disk Usage:"
du -sh /volume1/docker/maham/*

echo ""
echo "Network Connections:"
netstat -an | grep -E "5433|8888" | head -10
```

## 7. Troubleshooting

### Common Issues and Solutions

#### 1. Cannot Connect to PostgreSQL

```bash
# Check if container is running
sudo docker ps | grep postgres

# Check PostgreSQL logs
sudo docker logs maham_postgres

# Test connection from Synology
sudo docker exec -it maham_postgres psql -U postgres

# Check firewall rules
sudo iptables -L -n | grep 5433
```

#### 2. Docker Network Issues

```bash
# Inspect network
sudo docker network inspect maham_network

# Check container network
sudo docker inspect maham_postgres | grep IPAddress

# Test connectivity between containers
sudo docker exec maham_backend ping postgres
```

#### 3. Permission Denied Errors

```bash
# Fix PostgreSQL permissions
sudo chown -R 999:999 /volume1/docker/maham/postgres

# Fix app permissions
sudo chown -R 1000:1000 /volume1/docker/maham/app

# Check Synology permissions
ls -la /volume1/docker/maham/
```

#### 4. Firewall Blocking Connections

```bash
# Temporarily disable firewall (for testing only!)
sudo synoiptables -F

# Check current rules
sudo iptables -L -n -v

# Add logging to debug
sudo iptables -I INPUT -j LOG --log-prefix "FIREWALL: "
sudo tail -f /var/log/messages | grep FIREWALL
```

## 8. Security Best Practices for Synology

### 1. Enable Auto Block
- Control Panel → Security → Protection
- Enable auto block for repeated failed login attempts

### 2. Configure Access Protection
- Control Panel → Security → Protection
- Enable DoS protection

### 3. Use Strong Passwords
```bash
# Generate strong password
openssl rand -base64 32
```

### 4. Regular Backups
```bash
# Backup Docker volumes
sudo tar -czf /volume1/backups/maham-backup-$(date +%Y%m%d).tar.gz \
  /volume1/docker/maham/postgres/data
```

### 5. SSL/TLS Configuration
- Use Synology's built-in reverse proxy with SSL
- Control Panel → Application Portal → Reverse Proxy

## 9. Performance Optimization

### PostgreSQL Tuning for Synology

Based on your NAS model:

#### For Entry-Level NAS (2GB-4GB RAM):
```yaml
command:
  - "postgres"
  - "-c"
  - "shared_buffers=64MB"
  - "-c"
  - "effective_cache_size=256MB"
  - "-c"
  - "max_connections=50"
```

#### For Mid-Range NAS (4GB-8GB RAM):
```yaml
command:
  - "postgres"
  - "-c"
  - "shared_buffers=128MB"
  - "-c"
  - "effective_cache_size=512MB"
  - "-c"
  - "max_connections=100"
```

#### For High-End NAS (8GB+ RAM):
```yaml
command:
  - "postgres"
  - "-c"
  - "shared_buffers=256MB"
  - "-c"
  - "effective_cache_size=1GB"
  - "-c"
  - "max_connections=200"
```

## 10. Quick Commands Reference

```bash
# Start services
cd /volume1/docker/maham
sudo docker-compose -f docker-compose.synology.yml up -d

# Stop services
sudo docker-compose -f docker-compose.synology.yml down

# View logs
sudo docker-compose -f docker-compose.synology.yml logs -f

# Restart services
sudo docker-compose -f docker-compose.synology.yml restart

# Update images
sudo docker-compose -f docker-compose.synology.yml pull
sudo docker-compose -f docker-compose.synology.yml up -d

# Backup database
sudo docker exec maham_postgres pg_dump -U postgres MahamDB > backup.sql

# Restore database
sudo docker exec -i maham_postgres psql -U postgres MahamDB < backup.sql

# Access PostgreSQL CLI
sudo docker exec -it maham_postgres psql -U postgres

# Check container health
sudo docker inspect maham_postgres --format='{{.State.Health.Status}}'
```

## Connection Strings

### From External Clients:
```
# PostgreSQL
Host=your-synology-ip;Port=5433;Database=MahamDB;Username=postgres;Password=postgres123;

# API Endpoint
http://your-synology-ip:8888
```

### From Other Docker Containers:
```
# PostgreSQL (using service name)
Host=postgres;Port=5432;Database=MahamDB;Username=postgres;Password=postgres123;

# API Endpoint (using service name)
http://zradminwebapi:8888
```

## Summary

Your Synology NAS is now configured to:
1. ✅ Allow Docker network communication
2. ✅ Enable PostgreSQL remote access on port 5433
3. ✅ Enable API access on port 8888
4. ✅ Properly isolate Docker networks
5. ✅ Maintain security with firewall rules

Remember to:
- Regularly update Docker images
- Monitor resource usage
- Backup your data
- Review firewall logs periodically
- Keep DSM and packages updated