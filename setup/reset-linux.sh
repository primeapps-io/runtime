#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
cd ..
basePath=$(pwd [-LP])

echo -e "${GREEN}Stoping services...${NC}"
systemctl stop postgres-pre
systemctl stop minio-pre
systemctl stop redis-pre

sleep 3 # Sleep 3 seconds for stop all services

echo -e "${GREEN}Deleting data folders...${NC}"
cd "$basePath/data"

rm -rf pgsql_pre
rm -rf redis_pre
rm -rf minio_pre1
rm -rf minio_pre2
rm -rf minio_pre3
rm -rf minio_pre4
rm -rf primeapps

tar -xzf pgsql_pre.tar.gz pgsql_pre
tar -xzf redis_pre.tar.gz redis_pre
tar -xzf minio_pre1.tar.gz minio_pre1
tar -xzf minio_pre2.tar.gz minio_pre2
tar -xzf minio_pre3.tar.gz minio_pre3
tar -xzf minio_pre4.tar.gz minio_pre4

echo -e "${GREEN}Starting services...${NC}"
systemctl start postgres-pre
systemctl start minio-pre
systemctl start redis-pre

echo -e "${BLUE}Completed${NC}"