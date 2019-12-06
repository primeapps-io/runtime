#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
cd ..
basePath=$(pwd -W)

echo -e "${GREEN}Stoping services...${NC}"
net stop Postgres-PRE
net stop MinIO-PRE
net stop Redis-PRE

echo -e "${GREEN}Deleting services...${NC}"
sc delete Postgres-PRE
sc delete MinIO-PRE
sc delete Redis-PRE

echo -e "${GREEN}Deleting $basePath/data...${NC}"
rm -rf "$basePath/data"

echo -e "${GREEN}Deleting $basePath/programs...${NC}"
rm -rf "$basePath/programs"

echo -e "${BLUE}Completed${NC}"