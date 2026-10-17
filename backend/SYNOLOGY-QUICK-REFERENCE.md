# Synology Docker Setup - Quick Reference

## 🚀 Quick Start

### 1. Windows Users
```batch
# Run the deployment script
deploy-to-synology.bat
```

### 2. Mac/Linux Users
```bash
# Make script executable
chmod +x synology-setup.sh

# Copy to Synology
scp synology-setup.sh admin@your-synology-ip:/volume1/docker/maham/

# SSH and run
ssh admin@your-synology-ip
cd /volume1/docker/maham
sudo bash synology-setup.sh
```

## 📋 Pre-Setup Checklist

- [ ] Docker package installed from Package Center
- [ ] SSH enabled (Control Panel → Terminal & SNMP)
- [ ] Admin access to DSM
- [ ] At least 2GB free space on volume1

## 🔧 Manual Setup Steps

### 1. Enable SSH
```
DSM → Control Panel → Terminal & SNMP → Enable SSH service
```

### 2. Configure Firewall
```
DSM → Control Panel → Security → Firewall → Edit Rules

Add these ports:
- 5433 (PostgreSQL)
- 8888 (API)
- 22 (SSH - optional)
```

### 3. Create Docker Network
```bash
sudo docker network create \
  --subnet=172.20.0.0/16 \
  --gateway=172.20.0.1 \
  maham_network
```

### 4. Start Services
```bash
cd /volume1/docker/maham
sudo docker-compose up -d
```

## 🌐 Connection Information

### From External Network
```
PostgreSQL: your-synology-ip:5433
API: http://your-synology-ip:8888
```

### Connection String
```
Host=your-synology-ip;Port=5433;Database=MahamDB;Username=postgres;Password=postgres123;
```

## 📊 Monitoring Commands

### Check Status
```bash
docker ps | grep maham
```

### View Logs
```bash
docker logs maham_postgres -f
docker logs maham_backend -f
```

### Resource Usage
```bash
docker stats --no-stream
```

### Database Connections
```bash
docker exec maham_postgres psql -U postgres -c "SELECT count(*) FROM pg_stat_activity;"
```

## 🛠️ Troubleshooting

### Cannot Connect to PostgreSQL

1. Check container status:
```bash
docker ps | grep postgres
```

2. Check firewall:
```bash
sudo iptables -L -n | grep 5433
```

3. Test local connection:
```bash
docker exec -it maham_postgres psql -U postgres
```

### Docker Network Issues

1. List networks:
```bash
docker network ls
```

2. Inspect network:
```bash
docker network inspect maham_network
```

3. Recreate network:
```bash
docker network rm maham_network
docker network create --subnet=172.20.0.0/16 maham_network
```

### Permission Errors

```bash
# Fix PostgreSQL permissions
sudo chown -R 999:999 /volume1/docker/maham/postgres

# Fix app permissions  
sudo chown -R 1000:1000 /volume1/docker/maham/app
```

## 🔄 Common Operations

### Start Services
```bash
cd /volume1/docker/maham
docker-compose up -d
```

### Stop Services
```bash
docker-compose down
```

### Restart Services
```bash
docker-compose restart
```

### Update Images
```bash
docker-compose pull
docker-compose up -d
```

### Backup Database
```bash
docker exec maham_postgres pg_dump -U postgres MahamDB > backup.sql
```

### Restore Database
```bash
docker exec -i maham_postgres psql -U postgres MahamDB < backup.sql
```

## 📁 Directory Structure

```
/volume1/docker/maham/
├── postgres/
│   ├── data/         # Database files
│   ├── config/       # PostgreSQL configs
│   └── logs/         # Database logs
├── app/
│   ├── appsettings/  # App configuration
│   ├── wwwroot/      # Static files
│   └── logs/         # Application logs
├── backups/          # Database backups
├── scripts/          # Helper scripts
├── docker-compose.yml
└── .env              # Environment variables
```

## 🔐 Security Checklist

- [ ] Change default PostgreSQL password
- [ ] Restrict firewall rules to specific IPs
- [ ] Enable DSM auto-block protection
- [ ] Configure SSL/TLS for API
- [ ] Regular security updates
- [ ] Enable 2FA on DSM admin account
- [ ] Regular backups to external location

## 📱 Synology Mobile App Access

1. Install DS File app
2. Navigate to: `/docker/maham/`
3. View logs and configs directly

## 🔗 Useful DSM Locations

- Docker GUI: Main Menu → Docker
- Firewall: Control Panel → Security → Firewall
- Resource Monitor: Main Menu → Resource Monitor
- Task Scheduler: Control Panel → Task Scheduler
- Log Center: Main Menu → Log Center

## 💡 Performance Tips

### For Low-End NAS (2-4GB RAM)
```yaml
command:
  - "postgres"
  - "-c"
  - "shared_buffers=64MB"
  - "-c"
  - "max_connections=50"
```

### For High-End NAS (8GB+ RAM)
```yaml
command:
  - "postgres"
  - "-c"
  - "shared_buffers=256MB"
  - "-c"
  - "max_connections=200"
```

## 📞 Support Resources

- [Synology Docker Documentation](https://www.synology.com/en-us/dsm/packages/Docker)
- [PostgreSQL Docker Hub](https://hub.docker.com/_/postgres)
- [Docker Compose Reference](https://docs.docker.com/compose/)

## ⚡ Quick Fixes

### Reset Everything
```bash
cd /volume1/docker/maham
docker-compose down -v
rm -rf postgres/data/*
docker-compose up -d
```

### Force Recreate Containers
```bash
docker-compose up -d --force-recreate
```

### Clean Docker System
```bash
docker system prune -a
```

## 📝 Notes

- Default subnet: 172.20.0.0/16
- PostgreSQL port: 5433 (external), 5432 (internal)
- API port: 8888
- Default database: MahamDB
- Default user: postgres