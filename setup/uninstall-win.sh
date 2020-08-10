#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
cd ..
basePath=$(pwd -W)

echo -e "${GREEN}Stoping services...${NC}"
net stop Postgres-PrimeApps
net stop MinIO-PrimeApps
net stop Redis-PrimeApps

echo -e "${GREEN}Deleting services...${NC}"
sc delete Postgres-PrimeApps
sc delete MinIO-PrimeApps
sc delete Redis-PrimeApps

echo -e "${GREEN}Deleting $basePath/data...${NC}"
rm -rf "$basePath/data"

echo -e "${GREEN}Deleting $basePath/programs...${NC}"
rm -rf "$basePath/programs"

echo -e "${BLUE}Completed${NC}"