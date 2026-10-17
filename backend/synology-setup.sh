#!/bin/bash

# Synology NAS Docker & Firewall Automated Setup Script
# Compatible with DSM 6.2+ and DSM 7.0+
# Run with: sudo bash synology-setup.sh

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

# Configuration variables
VOLUME_PATH="/volume1/docker/maham"
DOCKER_NETWORK="maham_network"
DOCKER_SUBNET="172.20.0.0/16"
DOCKER_GATEWAY="172.20.0.1"
POSTGRES_PORT=5433
API_PORT=8888
POSTGRES_PASSWORD="postgres123"

# Logging
LOG_FILE="/var/log/synology-docker-setup.log"
exec 1> >(tee -a $LOG_FILE)
exec 2>&1

echo "$(date '+%Y-%m-%d %H:%M:%S') - Starting Synology Docker Setup" >> $LOG_FILE

# Functions
print_banner() {
    echo -e "${BLUE}============================================${NC}"
    echo -e "${BLUE}  Synology Docker & Firewall Setup Script  ${NC}"
    echo -e "${BLUE}============================================${NC}"
    echo ""
}

check_prerequisites() {
    echo -e "${YELLOW}Checking prerequisites...${NC}"
    
    # Check if running as root
    if [ "$EUID" -ne 0 ]; then 
        echo -e "${RED}Error: Please run as root (use sudo)${NC}"
        exit 1
    fi
    
    # Check DSM version
    DSM_VERSION=$(cat /etc.defaults/VERSION | grep productversion | cut -d'"' -f2)
    DSM_MAJOR=$(echo $DSM_VERSION | cut -d'.' -f1)
    echo -e "${GREEN}✓ DSM Version: $DSM_VERSION${NC}"
    
    # Check if Docker is installed
    if [ ! -f "/var/packages/Docker/enabled" ]; then
        echo -e "${RED}Error: Docker package is not installed${NC}"
        echo -e "${YELLOW}Please install Docker from Package Center first${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ Docker is installed${NC}"
    
    # Check if docker command is available
    if ! command -v docker &> /dev/null; then
        echo -e "${RED}Error: Docker command not found${NC}"
        exit 1
    fi
    echo -e "${GREEN}✓ Docker command available${NC}"
    
    echo ""
}

create_directory_structure() {
    echo -e "${YELLOW}Creating directory structure...${NC}"
    
    # Create main directories
    mkdir -p $VOLUME_PATH/{postgres/{data,config,logs},app/{appsettings,wwwroot,logs},backups,scripts}
    
    # Set permissions
    chown -R 999:999 $VOLUME_PATH/postgres     # PostgreSQL user
    chown -R 1000:1000 $VOLUME_PATH/app        # Application user
    chmod -R 755 $VOLUME_PATH
    
    echo -e "${GREEN}✓ Directory structure created${NC}"
    echo ""
}

setup_docker_network() {
    echo -e "${YELLOW}Setting up Docker network...${NC}"
    
    # Check if network exists
    if docker network ls | grep -q "$DOCKER_NETWORK"; then
        echo -e "${CYAN}Network '$DOCKER_NETWORK' already exists${NC}"
    else
        # Create network
        docker network create \
            --driver bridge \
            --subnet=$DOCKER_SUBNET \
            --gateway=$DOCKER_GATEWAY \
            --opt com.docker.network.bridge.name=br-maham \
            $DOCKER_NETWORK
        
        echo -e "${GREEN}✓ Docker network created${NC}"
    fi
    
    # Display network info
    echo -e "${CYAN}Network configuration:${NC}"
    docker network inspect $DOCKER_NETWORK --format='  Subnet: {{range .IPAM.Config}}{{.Subnet}}{{end}}'
    docker network inspect $DOCKER_NETWORK --format='  Gateway: {{range .IPAM.Config}}{{.Gateway}}{{end}}'
    echo ""
}

configure_firewall() {
    echo -e "${YELLOW}Configuring firewall rules...${NC}"
    
    # Function to add iptables rule if not exists
    add_iptables_rule() {
        local rule="$1"
        if ! iptables -C $rule 2>/dev/null; then
            iptables -I $rule
            echo -e "${GREEN}✓ Added rule: $rule${NC}"
        else
            echo -e "${CYAN}Rule already exists: $rule${NC}"
        fi
    }
    
    # Allow Docker networks
    add_iptables_rule "INPUT -s 172.16.0.0/12 -j ACCEPT"
    add_iptables_rule "INPUT -s 10.0.0.0/8 -j ACCEPT"
    
    # Allow specific ports
    add_iptables_rule "INPUT -p tcp --dport $POSTGRES_PORT -j ACCEPT"
    add_iptables_rule "INPUT -p tcp --dport $API_PORT -j ACCEPT"
    
    # Allow established connections
    add_iptables_rule "INPUT -m conntrack --ctstate ESTABLISHED,RELATED -j ACCEPT"
    
    # Save rules
    iptables-save > /usr/syno/etc/iptables_docker_rules
    
    # Create persistence script
    cat > /usr/local/etc/rc.d/docker-firewall.sh <<'EOF'
#!/bin/bash
# Restore Docker firewall rules on boot
if [ -f /usr/syno/etc/iptables_docker_rules ]; then
    iptables-restore < /usr/syno/etc/iptables_docker_rules
fi
EOF
    chmod +x /usr/local/etc/rc.d/docker-firewall.sh
    
    echo -e "${GREEN}✓ Firewall configured${NC}"
    echo ""
}

create_postgresql_config() {
    echo -e "${YELLOW}Creating PostgreSQL configuration...${NC}"
    
    # postgresql.conf
    cat > $VOLUME_PATH/postgres/config/postgresql.conf <<EOF
# PostgreSQL Configuration for Synology NAS
listen_addresses = '*'
port = 5432
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
log_filename = 'postgresql-%Y-%m-%d.log'
log_file_mode = 0600
log_rotation_age = 1d
log_rotation_size = 100MB
log_line_prefix = '%t [%p]: [%l-1] user=%u,db=%d,app=%a,client=%h '
log_checkpoints = on
log_connections = on
log_disconnections = on
log_duration = off
log_error_verbosity = default
log_hostname = on
log_lock_waits = on
log_statement = 'none'
log_temp_files = -1
log_timezone = 'UTC'
EOF
    
    # pg_hba.conf
    cat > $VOLUME_PATH/postgres/config/pg_hba.conf <<EOF
# PostgreSQL Client Authentication Configuration
# TYPE  DATABASE        USER            ADDRESS                 METHOD

# Local connections
local   all             all                                     trust

# IPv4 local connections
host    all             all             127.0.0.1/32            trust

# Docker networks
host    all             all             172.16.0.0/12           md5
host    all             all             10.0.0.0/8              md5

# Local network (adjust as needed)
host    all             all             192.168.0.0/16          md5

# Allow all (use with caution)
host    all             all             0.0.0.0/0               md5

# IPv6 connections
host    all             all             ::1/128                 trust
EOF
    
    chmod 600 $VOLUME_PATH/postgres/config/*.conf
    chown 999:999 $VOLUME_PATH/postgres/config/*.conf
    
    echo -e "${GREEN}✓ PostgreSQL configuration created${NC}"
    echo ""
}

create_docker_compose() {
    echo -e "${YELLOW}Creating Docker Compose configuration...${NC}"
    
    cat > $VOLUME_PATH/docker-compose.yml <<EOF
version: '3.8'

services:
  postgres:
    image: postgres:15-alpine
    container_name: maham_postgres
    restart: unless-stopped
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: MahamDB
      POSTGRES_INITDB_ARGS: "--encoding=UTF8 --locale=C"
    ports:
      - "0.0.0.0:${POSTGRES_PORT}:5432"
    volumes:
      - ${VOLUME_PATH}/postgres/data:/var/lib/postgresql/data
      - ${VOLUME_PATH}/postgres/config/postgresql.conf:/etc/postgresql/postgresql.conf:ro
      - ${VOLUME_PATH}/postgres/config/pg_hba.conf:/etc/postgresql/pg_hba.conf:ro
      - ${VOLUME_PATH}/postgres/logs:/var/log/postgresql
    command:
      - "postgres"
      - "-c"
      - "config_file=/etc/postgresql/postgresql.conf"
      - "-c"
      - "hba_file=/etc/postgresql/pg_hba.conf"
    networks:
      - ${DOCKER_NETWORK}
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 30s
      timeout: 10s
      retries: 5
      start_period: 30s
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"

  zradminwebapi:
    image: zradminwebapi:latest
    container_name: maham_backend
    restart: unless-stopped
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ASPNETCORE_URLS=http://+:8888
      - DOTNET_SYSTEM_NET_DISABLEIPV6=true
      - ConnectionStrings__DefaultConnection=Host=postgres;Port=5432;Database=MahamDB;Username=postgres;Password=${POSTGRES_PASSWORD};Pooling=true;
    ports:
      - "0.0.0.0:${API_PORT}:8888"
    depends_on:
      postgres:
        condition: service_healthy
    volumes:
      - ${VOLUME_PATH}/app/appsettings:/app/appsettings
      - ${VOLUME_PATH}/app/wwwroot:/app/wwwroot
      - ${VOLUME_PATH}/app/logs:/app/logs
    networks:
      - ${DOCKER_NETWORK}
    logging:
      driver: "json-file"
      options:
        max-size: "10m"
        max-file: "3"

networks:
  ${DOCKER_NETWORK}:
    external: true
EOF
    
    # Create .env file
    cat > $VOLUME_PATH/.env <<EOF
# Environment Configuration
POSTGRES_PASSWORD=${POSTGRES_PASSWORD}
DOCKER_NETWORK=${DOCKER_NETWORK}
VOLUME_PATH=${VOLUME_PATH}
POSTGRES_PORT=${POSTGRES_PORT}
API_PORT=${API_PORT}
EOF
    
    echo -e "${GREEN}✓ Docker Compose configuration created${NC}"
    echo ""
}

create_helper_scripts() {
    echo -e "${YELLOW}Creating helper scripts...${NC}"
    
    # Start script
    cat > $VOLUME_PATH/scripts/start.sh <<'EOF'
#!/bin/bash
cd /volume1/docker/maham
docker-compose up -d
echo "Services started. Check logs with: docker-compose logs -f"
EOF
    
    # Stop script
    cat > $VOLUME_PATH/scripts/stop.sh <<'EOF'
#!/bin/bash
cd /volume1/docker/maham
docker-compose down
echo "Services stopped."
EOF
    
    # Backup script
    cat > $VOLUME_PATH/scripts/backup.sh <<'EOF'
#!/bin/bash
BACKUP_DIR="/volume1/docker/maham/backups"
TIMESTAMP=$(date +%Y%m%d_%H%M%S)
cd /volume1/docker/maham

echo "Starting backup..."
docker exec maham_postgres pg_dump -U postgres MahamDB > $BACKUP_DIR/MahamDB_$TIMESTAMP.sql
tar -czf $BACKUP_DIR/app_data_$TIMESTAMP.tar.gz app/
echo "Backup completed: $BACKUP_DIR/*_$TIMESTAMP.*"

# Keep only last 7 backups
find $BACKUP_DIR -name "*.sql" -mtime +7 -delete
find $BACKUP_DIR -name "*.tar.gz" -mtime +7 -delete
EOF
    
    # Monitor script
    cat > $VOLUME_PATH/scripts/monitor.sh <<'EOF'
#!/bin/bash
echo "=== Docker Container Status ==="
docker ps --filter "name=maham" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"

echo -e "\n=== Resource Usage ==="
docker stats --no-stream --filter "name=maham"

echo -e "\n=== PostgreSQL Connections ==="
docker exec maham_postgres psql -U postgres -c "SELECT datname, count(*) as connections FROM pg_stat_activity GROUP BY datname;" 2>/dev/null || echo "PostgreSQL not running"

echo -e "\n=== Disk Usage ==="
du -sh /volume1/docker/maham/* | head -10

echo -e "\n=== Recent Logs ==="
docker logs maham_postgres --tail 5 2>&1 | head -5
docker logs maham_backend --tail 5 2>&1 | head -5
EOF
    
    # Make scripts executable
    chmod +x $VOLUME_PATH/scripts/*.sh
    
    echo -e "${GREEN}✓ Helper scripts created${NC}"
    echo ""
}

setup_synology_task() {
    echo -e "${YELLOW}Setting up Synology Task Scheduler...${NC}"
    
    # Create task configuration
    cat > $VOLUME_PATH/scripts/synology-task.sh <<'EOF'
#!/bin/bash
# This script should be added to Synology Task Scheduler
# Control Panel -> Task Scheduler -> Create -> Scheduled Task -> User-defined script

# Start Docker containers on boot
sleep 60  # Wait for Docker to fully start
cd /volume1/docker/maham
/usr/local/bin/docker-compose up -d

# Log the action
echo "$(date '+%Y-%m-%d %H:%M:%S') - Docker containers started" >> /var/log/maham-docker.log
EOF
    
    chmod +x $VOLUME_PATH/scripts/synology-task.sh
    
    echo -e "${CYAN}To enable auto-start:${NC}"
    echo "1. Open DSM Control Panel -> Task Scheduler"
    echo "2. Create -> Triggered Task -> User-defined script"
    echo "3. Task name: 'Start Maham Docker'"
    echo "4. User: root"
    echo "5. Event: Boot-up"
    echo "6. Script: bash $VOLUME_PATH/scripts/synology-task.sh"
    echo ""
}

test_setup() {
    echo -e "${YELLOW}Testing setup...${NC}"
    
    # Test network
    echo -n "Docker network: "
    if docker network ls | grep -q $DOCKER_NETWORK; then
        echo -e "${GREEN}✓${NC}"
    else
        echo -e "${RED}✗${NC}"
    fi
    
    # Test firewall rules
    echo -n "Firewall rules: "
    if iptables -L -n | grep -q $POSTGRES_PORT; then
        echo -e "${GREEN}✓${NC}"
    else
        echo -e "${YELLOW}⚠ Manual verification needed${NC}"
    fi
    
    # Test directories
    echo -n "Directory structure: "
    if [ -d "$VOLUME_PATH/postgres/data" ]; then
        echo -e "${GREEN}✓${NC}"
    else
        echo -e "${RED}✗${NC}"
    fi
    
    # Test Docker
    echo -n "Docker service: "
    if docker info &>/dev/null; then
        echo -e "${GREEN}✓${NC}"
    else
        echo -e "${RED}✗${NC}"
    fi
    
    echo ""
}

display_summary() {
    # Get IP address
    IP_ADDRESS=$(ip route get 1 | awk '{print $NF;exit}' 2>/dev/null || hostname -I | awk '{print $1}')
    
    echo -e "${BLUE}============================================${NC}"
    echo -e "${BLUE}          Setup Complete Summary            ${NC}"
    echo -e "${BLUE}============================================${NC}"
    echo ""
    echo -e "${GREEN}Installation Directory:${NC} $VOLUME_PATH"
    echo -e "${GREEN}Docker Network:${NC} $DOCKER_NETWORK ($DOCKER_SUBNET)"
    echo ""
    echo -e "${CYAN}Connection Information:${NC}"
    echo -e "  PostgreSQL: ${YELLOW}$IP_ADDRESS:$POSTGRES_PORT${NC}"
    echo -e "  API Endpoint: ${YELLOW}http://$IP_ADDRESS:$API_PORT${NC}"
    echo ""
    echo -e "${CYAN}PostgreSQL Credentials:${NC}"
    echo -e "  Username: ${YELLOW}postgres${NC}"
    echo -e "  Password: ${YELLOW}$POSTGRES_PASSWORD${NC}"
    echo -e "  Database: ${YELLOW}MahamDB${NC}"
    echo ""
    echo -e "${CYAN}Connection String:${NC}"
    echo -e "  ${YELLOW}Host=$IP_ADDRESS;Port=$POSTGRES_PORT;Database=MahamDB;Username=postgres;Password=$POSTGRES_PASSWORD;${NC}"
    echo ""
    echo -e "${CYAN}Quick Commands:${NC}"
    echo -e "  Start services:  ${YELLOW}cd $VOLUME_PATH && docker-compose up -d${NC}"
    echo -e "  Stop services:   ${YELLOW}cd $VOLUME_PATH && docker-compose down${NC}"
    echo -e "  View logs:       ${YELLOW}cd $VOLUME_PATH && docker-compose logs -f${NC}"
    echo -e "  Backup:          ${YELLOW}bash $VOLUME_PATH/scripts/backup.sh${NC}"
    echo -e "  Monitor:         ${YELLOW}bash $VOLUME_PATH/scripts/monitor.sh${NC}"
    echo ""
    echo -e "${GREEN}✓ Setup completed successfully!${NC}"
    echo -e "${YELLOW}Note: Remember to configure auto-start in Task Scheduler${NC}"
    echo ""
}

# Main execution
main() {
    print_banner
    check_prerequisites
    create_directory_structure
    setup_docker_network
    configure_firewall
    create_postgresql_config
    create_docker_compose
    create_helper_scripts
    setup_synology_task
    test_setup
    display_summary
    
    # Save configuration summary
    display_summary > $VOLUME_PATH/SETUP_INFO.txt
    
    echo -e "${CYAN}Configuration saved to: $VOLUME_PATH/SETUP_INFO.txt${NC}"
    echo -e "${CYAN}Setup log saved to: $LOG_FILE${NC}"
}

# Run main function
main "$@"