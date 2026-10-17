# 🔒 HTTPS Deployment Configuration

## Overview
The application now automatically generates and configures SSL certificates for HTTPS deployment if none exist, ensuring secure communication is always available.

## ✅ Key Features

### 1. **Automatic Certificate Generation**
- If no certificate exists at `/root/certs/certificate.pfx`, the deployment automatically generates a self-signed certificate
- Certificate is valid for 365 days
- Includes Subject Alternative Names (SAN) for both hostname and IP addresses

### 2. **Flexible HTTPS Configuration**
- HTTPS is enabled by default (can be disabled via `USE_HTTPS=false` secret)
- Supports both HTTP (port 8080) and HTTPS (port 8443) simultaneously
- Automatic fallback to HTTP if HTTPS configuration fails

### 3. **GitHub Actions Integration**
The deployment workflow now:
1. Checks for existing certificate
2. Generates certificate if missing
3. Configures Docker container with SSL
4. Verifies both HTTP and HTTPS endpoints

## 📋 Configuration

### GitHub Secrets

| Secret | Description | Default | Required |
|--------|-------------|---------|----------|
| `USE_HTTPS` | Enable HTTPS deployment | `true` | No |
| `CERT_PASSWORD` | Certificate password | `DefaultPassword123!` | No |
| `SERVER_HOST` | Server hostname/IP | Auto-detected | No |
| `SERVER_USER` | SSH username | - | Yes |
| `SERVER_SSH_KEY` | SSH private key | - | Yes |

### Certificate Locations

- **Certificate Directory**: `/root/certs/`
- **PFX Certificate**: `/root/certs/certificate.pfx`
- **Private Key**: `/root/certs/private.key`
- **Certificate**: `/root/certs/certificate.crt`

## 🚀 Deployment Process

### 1. **Initial Deployment**
When deploying for the first time:

```bash
# GitHub Actions will automatically:
1. Build Docker image
2. Check for certificate at /root/certs/certificate.pfx
3. Generate self-signed certificate if missing
4. Start container with HTTPS enabled
5. Verify health on both HTTP and HTTPS
```

### 2. **Manual Certificate Generation**
To manually generate a certificate on the server:

```bash
# SSH into server
ssh user@server

# Create certificate directory
mkdir -p /root/certs
cd /root/certs

# Generate certificate
openssl genpkey -algorithm RSA -out private.key -pkeyopt rsa_keygen_bits:2048
openssl req -new -x509 -key private.key -out certificate.crt -days 365 \
  -subj "/C=US/ST=State/L=City/O=Organization/CN=your-domain.com"
openssl pkcs12 -export -out certificate.pfx \
  -inkey private.key -in certificate.crt \
  -password pass:YourPassword123!
```

### 3. **Using Let's Encrypt Certificate**
For production with a real domain:

```bash
# Install certbot
apt-get update && apt-get install -y certbot

# Generate Let's Encrypt certificate
certbot certonly --standalone -d your-domain.com --email admin@your-domain.com --agree-tos

# Convert to PFX
openssl pkcs12 -export -out /root/certs/certificate.pfx \
  -inkey /etc/letsencrypt/live/your-domain.com/privkey.pem \
  -in /etc/letsencrypt/live/your-domain.com/fullchain.pem \
  -password pass:YourPassword123!
```

## 🔍 Verification

### Health Check Endpoints

After deployment, the following endpoints are checked:

**HTTP (Port 8080):**
- `http://server:8080/health`
- `http://server:8080/swagger`
- `http://server:8080/scalar`

**HTTPS (Port 8443):**
- `https://server:8443/health`
- `https://server:8443/swagger`
- `https://server:8443/scalar`

### Manual Verification

```bash
# Check HTTP
curl http://your-server:8080/health

# Check HTTPS (with self-signed cert)
curl -k https://your-server:8443/health

# Check certificate details
openssl s_client -connect your-server:8443 -showcerts
```

## 🐳 Docker Configuration

### Container Environment Variables

```yaml
ASPNETCORE_ENVIRONMENT: Production
ASPNETCORE_URLS: "http://+:8080;https://+:8443"
ASPNETCORE_Kestrel__Certificates__Default__Path: /certs/certificate.pfx
ASPNETCORE_Kestrel__Certificates__Default__Password: YourPassword123!
```

### Volume Mounts

```yaml
volumes:
  - /root/certs:/certs:ro  # Read-only mount for certificates
```

## 🛠️ Troubleshooting

### Issue: Certificate not found
**Solution**: The deployment will automatically generate one. Check logs:
```bash
docker logs maham_backend --tail 50
```

### Issue: HTTPS not working
**Check**:
1. Certificate exists: `ls -la /root/certs/`
2. Container has access: `docker exec maham_backend ls -la /certs/`
3. Ports are open: `netstat -tlnp | grep -E '8080|8443'`

### Issue: Browser shows certificate warning
**Expected**: Self-signed certificates always show warnings. For production, use Let's Encrypt or a commercial certificate.

## 📊 Deployment Summary

After successful deployment, you'll see:

```
✅ Status: Successfully deployed to production

Application URLs:
- HTTP:  http://your-server:8080
- HTTPS: https://your-server:8443 🔒

API Documentation:
- Swagger: http://your-server:8080/swagger
- Scalar:  http://your-server:8080/scalar

Health Check Results:
- ✅ Container running
- ✅ Application responding
- ✅ Port 8080 accessible
- ✅ Port 8443 accessible (HTTPS)
- 🔒 SSL certificate configured
```

## 🔐 Security Notes

1. **Self-Signed Certificates**: Suitable for development/testing. Use proper certificates for production.
2. **Certificate Password**: Store securely in GitHub Secrets, never commit to code.
3. **Firewall**: Ensure ports 8080 and 8443 are open in your firewall.
4. **Certificate Renewal**: Self-signed certificates expire after 365 days. Set up monitoring.

## 📝 Quick Commands

```bash
# Check deployment status
docker ps | grep maham_backend

# View logs
docker logs maham_backend --tail 50

# Restart with new certificate
docker restart maham_backend

# Test HTTPS
curl -k https://localhost:8443/health
```