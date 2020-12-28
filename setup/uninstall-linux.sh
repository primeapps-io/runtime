#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
cd ..
basePath=$(pwd [-LP])

echo -e "${GREEN}Deleting services...${NC}"
rm /etc/systemd/system/postgres-pre.service
rm /etc/systemd/system/minio-pre.service
rm /etc/systemd/system/redis-pre.service

systemctl daemon-reload
systemctl reset-failed

echo -e "${GREEN}Deleting $basePath/data...${NC}"
rm -rf  -f "$basePath/data"

echo -e "${GREEN}Deleting $basePath/programs...${NC}"
rm -rf -f  "$basePath/programs"

echo -e "${BLUE}Completed${NC}"
