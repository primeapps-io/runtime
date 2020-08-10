#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
cd ..
basePath=$(pwd [-LP])

echo -e "${GREEN}Deleting services...${NC}"
launchctl remove io.primeapps.postgres
launchctl remove io.primeapps.minio
launchctl remove io.primeapps.redis

rm ~/Library/LaunchAgents/postgres-primeapps.plist
rm ~/Library/LaunchAgents/minio-primeapps.plist
rm ~/Library/LaunchAgents/redis-primeapps.plist

echo -e "${GREEN}Deleting $basePath/data...${NC}"
rm -rf "$basePath/data"

echo -e "${GREEN}Deleting $basePath/programs...${NC}"
rm -rf "$basePath/programs"

echo -e "${BLUE}Completed${NC}"