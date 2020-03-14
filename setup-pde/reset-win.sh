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
net stop Postgres-PDE
net stop MinIO-PRE
net stop MinIO-PDE
net stop Redis-PRE
net stop Redis-PDE
net stop Gitea-PDE

sleep 3 # Sleep 3 seconds for stop all services

echo -e "${GREEN}Deleting data folders...${NC}"
cd "$basePath/data"

rm -rf pgsql_pre
rm -rf pgsql_pde
rm -rf minio_pre
rm -rf minio_pde
rm -rf redis_pre
rm -rf redis_pde
rm -rf gitea
rm -rf primeapps

tar -xzf pgsql_pre.tar.gz pgsql_pre
tar -xzf pgsql_pde.tar.gz pgsql_pde
tar -xzf minio_pre.tar.gz minio_pre
tar -xzf minio_pde.tar.gz minio_pde
tar -xzf redis_pre.tar.gz redis_pre
tar -xzf redis_pde.tar.gz redis_pde
tar -xzf gitea.tar.gz gitea

echo -e "${GREEN}Starting services...${NC}"
net start Postgres-PRE
net start Postgres-PDE
net start MinIO-PRE
net start MinIO-PDE
net start Redis-PRE
net start Redis-PDE
net start Gitea-PDE

echo -e "${BLUE}Completed${NC}"