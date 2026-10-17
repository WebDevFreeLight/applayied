# Environment Variables Setup

This project uses environment variables to manage configuration and sensitive data securely.

## Quick Start

1. Copy the example environment file:
   ```bash
   cp .env.example .env
   ```

2. Edit the `.env` file with your actual values:
   ```bash
   # Update these values with your actual configuration
   POSTGRES_USER=your_actual_username
   POSTGRES_PASSWORD=your_secure_password
   POSTGRES_DB=your_database_name
   SUPABASE_HOST=your_supabase_host
   SUPABASE_IP=your_supabase_ip
   ```

3. Run the application:
   ```bash
   docker-compose up -d
   ```

## Environment Variables

### Application Configuration
- `ASPNETCORE_ENVIRONMENT`: Application environment (Production, Development, Staging)
- `ASPNETCORE_URLS`: Application URLs
- `DOTNET_SYSTEM_NET_DISABLEIPV6`: Disable IPv6 for compatibility

### Port Configuration
- `API_PORT`: Port for the API service (default: 8887)
- `POSTGRES_PORT`: External port for PostgreSQL (default: 5433)

### Database Configuration
- `POSTGRES_USER`: PostgreSQL username
- `POSTGRES_PASSWORD`: PostgreSQL password (⚠️ Keep this secure!)
- `POSTGRES_DB`: Database name
- `POSTGRES_HOST_AUTH_METHOD`: Authentication method

### Performance Settings
- `POSTGRES_MAX_CONNECTIONS`: Maximum database connections
- `POSTGRES_SHARED_BUFFERS`: Shared buffer size

### External Services
- `SUPABASE_HOST`: Supabase database host
- `SUPABASE_IP`: Supabase IP address for IPv4 mapping

### Container Configuration
- `API_CONTAINER_NAME`: Name for the API container
- `POSTGRES_CONTAINER_NAME`: Name for the PostgreSQL container
- `NETWORK_NAME`: Docker network name

### Health Check Settings
- `HEALTHCHECK_INTERVAL`: Health check interval
- `HEALTHCHECK_TIMEOUT`: Health check timeout
- `HEALTHCHECK_RETRIES`: Number of health check retries

## Security Notes

- ✅ The `.env` file is automatically ignored by Git (see `.gitignore`)
- ✅ Use `.env.example` as a template for new environments
- ⚠️ Never commit actual passwords or sensitive data to version control
- 🔒 Use strong passwords for production environments
- 🔐 Consider using Docker secrets for production deployments

## Troubleshooting

### Common Issues

1. **Port conflicts**: Change `API_PORT` or `POSTGRES_PORT` if ports are already in use
2. **Database connection issues**: Verify `POSTGRES_USER`, `POSTGRES_PASSWORD`, and `POSTGRES_DB`
3. **Supabase connectivity**: Check `SUPABASE_HOST` and `SUPABASE_IP` values

### Validation

To verify your environment setup:
```bash
# Check if containers are running
docker-compose ps

# View container logs
docker-compose logs zradminwebapi
docker-compose logs postgres
```

## Production Deployment

For production environments:
1. Use strong, unique passwords
2. Consider using Docker secrets instead of environment variables
3. Regularly rotate credentials
4. Monitor access logs
5. Use HTTPS/TLS for all connections

## Development vs Production

Create separate `.env` files for different environments:
- `.env.development`
- `.env.staging` 
- `.env.production`

Use docker-compose override files:
```bash
# Development
docker-compose -f docker-compose.yml -f docker-compose.dev.yml up

# Production
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up
```