#!/bin/bash

# Shell Script to Enable Remote Access on Linux
# Run as root or with sudo

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
NC='\033[0m' # No Color

echo -e "${GREEN}Setting up Remote Access for PostgreSQL and API...${NC}"

# Check if running as root
if [ "$EUID" -ne 0 ]; then 
    echo -e "${RED}Please run as root or with sudo${NC}"
    exit 1
fi

# Detect Linux distribution
if [ -f /etc/os-release ]; then
    . /etc/os-release
    OS=$NAME
    VER=$VERSION_ID
else
    echo -e "${RED}Cannot detect Linux distribution${NC}"
    exit 1
fi

echo -e "${CYAN}Detected OS: $OS $VER${NC}"

# Function to configure PostgreSQL
configure_postgresql() {
    echo -e "\n${YELLOW}Configuring PostgreSQL for remote access...${NC}"
    
    # Find PostgreSQL config directory
    PG_VERSION=$(psql --version 2>/dev/null | awk '{print $3}' | sed 's/\..*//')
    
    if [ -z "$PG_VERSION" ]; then
        echo -e "${YELLOW}PostgreSQL not installed locally, skipping local config${NC}"
        return
    fi
    
    # Common PostgreSQL config paths
    if [ -f "/etc/postgresql/$PG_VERSION/main/postgresql.conf" ]; then
        PG_CONFIG="/etc/postgresql/$PG_VERSION/main/postgresql.conf"
        PG_HBA="/etc/postgresql/$PG_VERSION/main/pg_hba.conf"
    elif [ -f "/var/lib/pgsql/$PG_VERSION/data/postgresql.conf" ]; then
        PG_CONFIG="/var/lib/pgsql/$PG_VERSION/data/postgresql.conf"
        PG_HBA="/var/lib/pgsql/$PG_VERSION/data/pg_hba.conf"
    else
        echo -e "${YELLOW}PostgreSQL config not found in standard locations${NC}"
        return
    fi
    
    # Backup configs
    cp $PG_CONFIG ${PG_CONFIG}.backup.$(date +%Y%m%d)
    cp $PG_HBA ${PG_HBA}.backup.$(date +%Y%m%d)
    
    # Enable remote connections
    sed -i "s/#listen_addresses = 'localhost'/listen_addresses = '*'/" $PG_CONFIG
    sed -i "s/listen_addresses = 'localhost'/listen_addresses = '*'/" $PG_CONFIG
    
    # Add remote access rules
    echo "# Allow remote connections" >> $PG_HBA
    echo "host    all             all             0.0.0.0/0               md5" >> $PG_HBA
    echo "host    all             all             ::/0                    md5" >> $PG_HBA
    
    # Restart PostgreSQL
    if command -v systemctl &> /dev/null; then
        systemctl restart postgresql
    elif command -v service &> /dev/null; then
        service postgresql restart
    fi
    
    echo -e "${GREEN}PostgreSQL configured for remote access${NC}"
}

# Function to configure firewall
configure_firewall() {
    echo -e "\n${YELLOW}Configuring firewall rules...${NC}"
    
    # Check for UFW (Ubuntu/Debian)
    if command -v ufw &> /dev/null; then
        echo "Using UFW firewall..."
        ufw allow 5432/tcp comment 'PostgreSQL'
        ufw allow 5433/tcp comment 'PostgreSQL Docker'
        ufw allow 8888/tcp comment 'Maham API'
        ufw --force enable
        ufw status numbered
        
    # Check for firewall-cmd (CentOS/RHEL/Fedora)
    elif command -v firewall-cmd &> /dev/null; then
        echo "Using firewalld..."
        firewall-cmd --permanent --add-port=5432/tcp
        firewall-cmd --permanent --add-port=5433/tcp
        firewall-cmd --permanent --add-port=8888/tcp
        firewall-cmd --permanent --add-service=postgresql
        firewall-cmd --reload
        firewall-cmd --list-all
        
    # Check for iptables
    elif command -v iptables &> /dev/null; then
        echo "Using iptables..."
        iptables -A INPUT -p tcp --dport 5432 -j ACCEPT
        iptables -A INPUT -p tcp --dport 5433 -j ACCEPT
        iptables -A INPUT -p tcp --dport 8888 -j ACCEPT
        
        # Save iptables rules
        if command -v netfilter-persistent &> /dev/null; then
            netfilter-persistent save
        elif [ -f /etc/sysconfig/iptables ]; then
            iptables-save > /etc/sysconfig/iptables
        fi
        
        iptables -L -n | grep -E "5432|5433|8888"
    else
        echo -e "${YELLOW}No supported firewall found${NC}"
    fi
    
    echo -e "${GREEN}Firewall configured${NC}"
}

# Function to install Docker if not present
install_docker() {
    if ! command -v docker &> /dev/null; then
        echo -e "\n${YELLOW}Docker not found. Install it? (y/n)${NC}"
        read -r response
        if [[ "$response" =~ ^[Yy]$ ]]; then
            curl -fsSL https://get.docker.com -o get-docker.sh
            sh get-docker.sh
            usermod -aG docker $SUDO_USER
            systemctl enable docker
            systemctl start docker
            
            # Install docker-compose
            curl -L "https://github.com/docker/compose/releases/latest/download/docker-compose-$(uname -s)-$(uname -m)" -o /usr/local/bin/docker-compose
            chmod +x /usr/local/bin/docker-compose
            
            echo -e "${GREEN}Docker installed successfully${NC}"
        fi
    else
        echo -e "${GREEN}Docker is already installed${NC}"
    fi
}

# Function to test ports
test_ports() {
    echo -e "\n${YELLOW}Testing port availability...${NC}"
    
    for port in 5432 5433 8888; do
        if nc -z localhost $port 2>/dev/null; then
            echo -e "  Port $port: ${YELLOW}IN USE${NC}"
        else
            echo -e "  Port $port: ${GREEN}AVAILABLE${NC}"
        fi
    done
}

# Function to display network info
show_network_info() {
    echo -e "\n${CYAN}Network Information:${NC}"
    
    # Get primary IP
    PRIMARY_IP=$(ip route get 1.1.1.1 | awk '{print $7; exit}')
    ALL_IPS=$(hostname -I)
    
    echo -e "  Primary IP: ${GREEN}$PRIMARY_IP${NC}"
    echo -e "  All IPs: $ALL_IPS"
    
    echo -e "\n${CYAN}Connection Strings for Remote Access:${NC}"
    echo -e "  ${YELLOW}PostgreSQL (Direct):${NC}"
    echo -e "    Host=$PRIMARY_IP;Port=5432;Database=MahamDB;Username=postgres;Password=postgres123;"
    echo -e ""
    echo -e "  ${YELLOW}PostgreSQL (Docker):${NC}"
    echo -e "    Host=$PRIMARY_IP;Port=5433;Database=MahamDB;Username=postgres;Password=postgres123;"
    echo -e ""
    echo -e "  ${YELLOW}API Endpoint:${NC}"
    echo -e "    http://$PRIMARY_IP:8888"
}

# Function to create systemd service for auto-start
create_systemd_service() {
    echo -e "\n${YELLOW}Create systemd service for auto-start? (y/n)${NC}"
    read -r response
    if [[ "$response" =~ ^[Yy]$ ]]; then
        cat > /etc/systemd/system/maham-backend.service <<EOF
[Unit]
Description=Maham Backend Service
After=docker.service
Requires=docker.service

[Service]
Type=simple
WorkingDirectory=/opt/maham/backend
ExecStart=/usr/local/bin/docker-compose up
ExecStop=/usr/local/bin/docker-compose down
Restart=always
User=root

[Install]
WantedBy=multi-user.target
EOF
        
        systemctl daemon-reload
        systemctl enable maham-backend
        echo -e "${GREEN}Systemd service created${NC}"
        echo "  Start service: systemctl start maham-backend"
        echo "  Stop service: systemctl stop maham-backend"
        echo "  View logs: journalctl -u maham-backend -f"
    fi
}

# Main execution
main() {
    echo -e "${CYAN}=== Remote Access Setup Script ===${NC}"
    
    # Configure PostgreSQL
    configure_postgresql
    
    # Configure firewall
    configure_firewall
    
    # Check/Install Docker
    install_docker
    
    # Test ports
    test_ports
    
    # Show network info
    show_network_info
    
    # Create systemd service
    create_systemd_service
    
    echo -e "\n${GREEN}=== Setup Complete! ===${NC}"
    echo -e "${YELLOW}Remember to:${NC}"
    echo "  1. Use strong passwords in production"
    echo "  2. Restrict IP ranges for better security"
    echo "  3. Enable SSL/TLS for encrypted connections"
    echo "  4. Regularly update and patch your systems"
    
    echo -e "\n${CYAN}Docker Commands:${NC}"
    echo "  Start all services:     docker-compose up -d"
    echo "  Start only PostgreSQL:  docker-compose up -d postgres"
    echo "  Check logs:            docker-compose logs -f"
    echo "  Stop all services:     docker-compose down"
}

# Run main function
main