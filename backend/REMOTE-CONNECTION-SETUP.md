# Remote Connection Setup Guide

## Overview
This guide explains how to configure your PostgreSQL database and application to accept remote connections.

## 1. PostgreSQL Configuration (Local PostgreSQL)

### Step 1: Edit postgresql.conf
Location varies by system:
- **Windows**: `C:\Program Files\PostgreSQL\15\data\postgresql.conf`
- **Linux**: `/etc/postgresql/15/main/postgresql.conf`
- **Docker**: Configured via environment variables

```conf
# Find and modify this line:
listen_addresses = '*'          # Listen on all interfaces
# Or specify specific IPs:
# listen_addresses = 'localhost,192.168.1.100,10.0.0.5'

# Ensure port is set:
port = 5432
```

### Step 2: Edit pg_hba.conf
Add rules to allow remote connections:

```conf
# TYPE  DATABASE        USER            ADDRESS                 METHOD

# Allow connections from specific IP
host    all             all             192.168.1.0/24          md5

# Allow connections from any IP (less secure)
host    all             all             0.0.0.0/0               md5

# Allow IPv6 connections
host    all             all             ::/0                    md5

# For specific database and user
host    MahamDB         postgres        192.168.1.0/24          md5
```

### Step 3: Restart PostgreSQL
```bash
# Windows
net stop postgresql-x64-15
net start postgresql-x64-15

# Linux
sudo systemctl restart postgresql

# Docker
docker-compose restart postgres
```

## 2. Docker PostgreSQL for Remote Access

### Update docker-compose.yml:

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: maham_postgres
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres123
      POSTGRES_DB: MahamDB
      # Allow connections from any host
      POSTGRES_HOST_AUTH_METHOD: "md5"
    ports:
      - "0.0.0.0:5432:5432"  # Bind to all interfaces
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./postgresql.conf:/etc/postgresql/postgresql.conf
      - ./pg_hba.conf:/etc/postgresql/pg_hba.conf
    command: 
      - "postgres"
      - "-c"
      - "listen_addresses=*"
      - "-c"
      - "max_connections=200"
    networks:
      - maham_network

  # Your backend API
  zradminwebapi:
    image: ${DOCKER_REGISTRY-}zradminwebapi
    build:
      context: .
      dockerfile: ZR.Admin.WebApi/Dockerfile
    container_name: maham_backend
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8888
    ports:
      - "0.0.0.0:8888:8888"  # Bind to all interfaces
    networks:
      - maham_network
    depends_on:
      - postgres

volumes:
  postgres_data:

networks:
  maham_network:
    driver: bridge
```

## 3. Application Configuration for Remote Access

### Update appsettings.json for remote connections:

```json
{
  "dbConfigs": [
    {
      "Comment": "Remote PostgreSQL Connection",
      "Conn": "Host=YOUR_SERVER_IP;Port=5432;Database=MahamDB;Username=postgres;Password=postgres123;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=100;",
      "DbType": 4,
      "ConfigId": "0",
      "IsAutoCloseConnection": true
    }
  ],
  "urls": "http://0.0.0.0:8888",  // Listen on all interfaces
  "corsUrls": [ "*" ]  // Allow all origins (configure properly for production)
}
```

## 4. Firewall Configuration

### Windows Firewall:
```powershell
# Open PowerShell as Administrator

# Allow PostgreSQL port
New-NetFirewallRule -DisplayName "PostgreSQL" -Direction Inbound -Protocol TCP -LocalPort 5432 -Action Allow

# Allow API port
New-NetFirewallRule -DisplayName "Maham API" -Direction Inbound -Protocol TCP -LocalPort 8888 -Action Allow

# Check rules
Get-NetFirewallRule -DisplayName "PostgreSQL"
Get-NetFirewallRule -DisplayName "Maham API"
```

### Linux (Ubuntu/Debian):
```bash
# Using ufw (Uncomplicated Firewall)
sudo ufw allow 5432/tcp
sudo ufw allow 8888/tcp
sudo ufw enable
sudo ufw status

# Using iptables
sudo iptables -A INPUT -p tcp --dport 5432 -j ACCEPT
sudo iptables -A INPUT -p tcp --dport 8888 -j ACCEPT
sudo iptables-save
```

### CentOS/RHEL:
```bash
# Using firewall-cmd
sudo firewall-cmd --permanent --add-port=5432/tcp
sudo firewall-cmd --permanent --add-port=8888/tcp
sudo firewall-cmd --reload
sudo firewall-cmd --list-all
```

## 5. Cloud Provider Specific Settings

### AWS EC2:
1. Go to EC2 Dashboard → Security Groups
2. Edit Inbound Rules:
   - Type: PostgreSQL, Port: 5432, Source: 0.0.0.0/0 (or specific IPs)
   - Type: Custom TCP, Port: 8888, Source: 0.0.0.0/0

### Azure:
1. Go to Network Security Group
2. Add Inbound Security Rules:
   - PostgreSQL: Port 5432, Priority 100
   - API: Port 8888, Priority 101

### Google Cloud:
```bash
# Create firewall rules
gcloud compute firewall-rules create allow-postgres \
  --allow tcp:5432 \
  --source-ranges 0.0.0.0/0

gcloud compute firewall-rules create allow-api \
  --allow tcp:8888 \
  --source-ranges 0.0.0.0/0
```

## 6. Supabase Remote Connection

For Supabase (already allows remote connections):

### Connection String:
```
Host=db.ctlaeyiulnukpoxuztla.supabase.co;Port=5432;Database=postgres;Username=postgres;Password=e/cYx$dW2Z6D+Cj;Ssl Mode=Require;Trust Server Certificate=true;
```

### Connection Pooling Settings:
```
Host=db.ctlaeyiulnukpoxuztla.supabase.co;Port=6543;Database=postgres;Username=postgres;Password=e/cYx$dW2Z6D+Cj;Ssl Mode=Require;Trust Server Certificate=true;Pooling=true;Minimum Pool Size=1;Maximum Pool Size=20;
```

Note: Port 6543 is for connection pooling (recommended for serverless/high-traffic apps)

## 7. Testing Remote Connections

### Test PostgreSQL Connection:
```bash
# From remote machine
psql -h YOUR_SERVER_IP -p 5432 -U postgres -d MahamDB

# Using pg_isready
pg_isready -h YOUR_SERVER_IP -p 5432

# Using telnet
telnet YOUR_SERVER_IP 5432

# Using nmap
nmap -p 5432 YOUR_SERVER_IP
```

### Test API Connection:
```bash
# Test API endpoint
curl http://YOUR_SERVER_IP:8888/api/health

# Test with wget
wget -O- http://YOUR_SERVER_IP:8888/api/health
```

## 8. Security Best Practices

### For PostgreSQL:
1. **Never use 0.0.0.0/0 in production** - Always specify IP ranges
2. **Use SSL/TLS connections**:
   ```
   Host=server;Port=5432;Database=db;Username=user;Password=pass;SSL Mode=Require;
   ```
3. **Create specific users with limited permissions**:
   ```sql
   CREATE USER app_user WITH PASSWORD 'strong_password';
   GRANT CONNECT ON DATABASE MahamDB TO app_user;
   GRANT USAGE ON SCHEMA public TO app_user;
   GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA public TO app_user;
   ```

4. **Use connection pooling** to limit concurrent connections
5. **Enable logging** for audit trails:
   ```conf
   log_connections = on
   log_disconnections = on
   log_statement = 'all'
   ```

### For Application:
1. **Configure CORS properly**:
   ```json
   "corsUrls": [ "https://yourdomain.com", "https://app.yourdomain.com" ]
   ```

2. **Use HTTPS in production**:
   ```json
   "urls": "https://+:443;http://+:80"
   ```

3. **Implement rate limiting**
4. **Use environment variables for sensitive data**:
   ```bash
   export DB_PASSWORD=your_secure_password
   ```

## 9. Troubleshooting

### Connection Refused:
- Check if PostgreSQL is running: `sudo systemctl status postgresql`
- Check if port is listening: `netstat -an | grep 5432`
- Check firewall rules: `sudo iptables -L`

### Authentication Failed:
- Verify pg_hba.conf settings
- Check username/password
- Ensure user has connect permissions

### Timeout Issues:
- Check network connectivity: `ping YOUR_SERVER_IP`
- Verify firewall/security group rules
- Check if port is open: `nc -zv YOUR_SERVER_IP 5432`

### Docker Specific:
```bash
# Check container logs
docker logs maham_postgres

# Exec into container
docker exec -it maham_postgres psql -U postgres

# Check network
docker network inspect backend_maham_network
```

## 10. Connection String Examples

### Local Network:
```
Host=192.168.1.100;Port=5432;Database=MahamDB;Username=postgres;Password=postgres123;
```

### Public IP:
```
Host=203.0.113.10;Port=5432;Database=MahamDB;Username=postgres;Password=postgres123;SSL Mode=Require;
```

### Domain Name:
```
Host=db.yourdomain.com;Port=5432;Database=MahamDB;Username=postgres;Password=postgres123;SSL Mode=Require;Trust Server Certificate=true;
```

### With Connection Pooling:
```
Host=server;Port=5432;Database=MahamDB;Username=postgres;Password=pass;Pooling=true;Minimum Pool Size=5;Maximum Pool Size=100;Connection Lifetime=300;
```

## Quick Setup Script

Create `setup-remote-access.sh`:

```bash
#!/bin/bash

# Enable PostgreSQL remote access
echo "Configuring PostgreSQL for remote access..."

# Backup original files
sudo cp /etc/postgresql/15/main/postgresql.conf /etc/postgresql/15/main/postgresql.conf.backup
sudo cp /etc/postgresql/15/main/pg_hba.conf /etc/postgresql/15/main/pg_hba.conf.backup

# Update postgresql.conf
sudo sed -i "s/#listen_addresses = 'localhost'/listen_addresses = '*'/" /etc/postgresql/15/main/postgresql.conf

# Add remote access rule to pg_hba.conf
echo "host    all             all             0.0.0.0/0               md5" | sudo tee -a /etc/postgresql/15/main/pg_hba.conf

# Configure firewall
sudo ufw allow 5432/tcp
sudo ufw allow 8888/tcp
sudo ufw --force enable

# Restart PostgreSQL
sudo systemctl restart postgresql

echo "PostgreSQL configured for remote access!"
echo "Remember to:"
echo "1. Update connection strings in your application"
echo "2. Use strong passwords"
echo "3. Restrict IP ranges in production"
```

Make it executable and run:
```bash
chmod +x setup-remote-access.sh
./setup-remote-access.sh
```