# Docker PostgreSQL Connection Fix

## Problem
The error `Failed to connect to [2600:1f1c:f9:4d14:272a:8f80:3771:26f4]:5432` indicates that Docker is trying to connect to PostgreSQL using IPv6, which is failing.

## Solutions

### Solution 1: Use Local PostgreSQL in Docker (Recommended for Development)

1. **Run with docker-compose:**
```bash
# Start both PostgreSQL and the backend
docker-compose up -d

# Or just start PostgreSQL
docker-compose up -d postgres

# Then run your app connecting to local PostgreSQL
docker-compose up zradminwebapi
```

2. **Connection String for Local PostgreSQL:**
```
Host=postgres;Port=5432;Database=MahamDB;Username=postgres;Password=postgres123;
```

### Solution 2: Connect to Supabase with IPv4 Fix

1. **Force IPv4 Connection:**
   - The docker-compose.yml includes environment variable to disable IPv6
   - Adds explicit host mapping to force IPv4 address

2. **Run with IPv4 fix:**
```bash
docker-compose up -d zradminwebapi
```

### Solution 3: Build and Run Manually with IPv4

```bash
# Build the image
docker build -f ZR.Admin.WebApi/Dockerfile -t maham-backend .

# Run with IPv4 only
docker run -d \
  --name maham-backend \
  -p 8888:8888 \
  -e DOTNET_SYSTEM_NET_DISABLEIPV6=true \
  --add-host db.ctlaeyiulnukpoxuztla.supabase.co:34.82.126.136 \
  -v $(pwd)/appsettings.Docker.json:/app/appsettings.json:ro \
  maham-backend
```

### Solution 4: Use Environment Variables

Override connection string via environment variable:

```bash
docker run -d \
  --name maham-backend \
  -p 8888:8888 \
  -e "dbConfigs__0__Conn=Host=34.82.126.136;Port=5432;Database=postgres;Username=postgres;Password=e/cYx\$dW2Z6D+Cj;Ssl Mode=Require;Trust Server Certificate=true;" \
  maham-backend
```

## Files Created/Modified

1. **docker-compose.yml** - Complete Docker setup with two options:
   - Local PostgreSQL database
   - External Supabase with IPv4 fix

2. **appsettings.Docker.json** - Docker-specific configuration:
   - Option 1: Connect to local PostgreSQL container
   - Option 2: Connect to Supabase using IPv4 address

## Quick Start

### For Local Development with PostgreSQL in Docker:
```bash
# Start everything
docker-compose up -d

# Check logs
docker-compose logs -f zradminwebapi

# Stop everything
docker-compose down
```

### For Production with Supabase:
```bash
# Use the IPv4 configuration
docker-compose up -d zradminwebapi
```

## Troubleshooting

1. **Check DNS Resolution:**
```bash
# Inside container
docker exec -it maham_backend nslookup db.ctlaeyiulnukpoxuztla.supabase.co
```

2. **Test Connection:**
```bash
# Test PostgreSQL connection
docker exec -it maham_backend apt-get update && apt-get install -y postgresql-client
docker exec -it maham_backend psql -h 34.82.126.136 -p 5432 -U postgres -d postgres
```

3. **Check Network:**
```bash
# List networks
docker network ls

# Inspect network
docker network inspect backend_maham_network
```

4. **View Logs:**
```bash
docker-compose logs -f zradminwebapi
```

## Additional Notes

- The IPv6 address `[2600:1f1c:f9:4d14:272a:8f80:3771:26f4]` is Supabase's IPv6 endpoint
- Docker on some systems has issues with IPv6 connectivity
- Using explicit IPv4 address `34.82.126.136` bypasses this issue
- Local PostgreSQL in Docker is recommended for development to avoid external dependencies

## Security Considerations

- Don't commit passwords to version control
- Use Docker secrets or environment variables for production
- Consider using SSL/TLS for database connections in production
- Rotate credentials regularly