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

echo -e "${GREEN}Deleting data folders...${NC}"
cd "$basePath/data"

rm -rf pgsql_pre
rm -rf minio_pre
rm -rf redis_pre
rm -rf primeapps

tar -xzf pgsql_pre.tar.gz pgsql_pre
tar -xzf minio_pre.tar.gz minio_pre
tar -xzf redis_pre.tar.gz redis_pre

echo -e "${GREEN}Starting services...${NC}"
net start Postgres-PRE
net start MinIO-PRE
net start Redis-PRE

echo -e "${BLUE}Completed${NC}"