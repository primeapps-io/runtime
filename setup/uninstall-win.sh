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
net stop Postgres-PRE-Test
net stop Postgres-PDE
net stop MinIO-PRE
net stop MinIO-PRE-Test
net stop MinIO-PDE
net stop Redis-PRE
net stop Redis-PRE-Test
net stop Redis-PDE
net stop Gitea-PDE

echo -e "${GREEN}Deleting services...${NC}"
sc delete Postgres-PRE
sc delete Postgres-PRE-Test
sc delete Postgres-PDE
sc delete MinIO-PRE
sc delete MinIO-PRE-Test
sc delete MinIO-PDE
sc delete Redis-PRE
sc delete Redis-PRE-Test
sc delete Redis-PDE
sc delete Gitea-PDE

echo -e "${GREEN}Deleting $basePath/data...${NC}"
rm -rf "$basePath/data"

echo -e "${GREEN}Deleting $basePath/programs...${NC}"
rm -rf "$basePath/programs"

echo -e "${BLUE}Completed${NC}"