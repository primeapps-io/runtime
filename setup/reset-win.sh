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

echo -e "${GREEN}Deleting data folders...${NC}"
cd "$basePath/data"

rm -rf pgsql_pre
rm -rf pgsql_pde
rm -rf pgsql_pre_test
rm -rf minio_pre
rm -rf minio_pde
rm -rf minio_pre_test
rm -rf redis_pre
rm -rf redis_pde
rm -rf redis_pre_test
rm -rf gitea

tar -xzf pgsql_pre.tar.gz pgsql_pre
tar -xzf pgsql_pde.tar.gz pgsql_pde
tar -xzf pgsql_pre_test.tar.gz pgsql_pre_test
tar -xzf minio_pre.tar.gz minio_pre
tar -xzf minio_pde.tar.gz minio_pde
tar -xzf minio_pre_test.tar.gz minio_pre_test
tar -xzf redis_pre.tar.gz redis_pre
tar -xzf redis_pde.tar.gz redis_pde
tar -xzf redis_pre_test.tar.gz redis_pre_test
tar -xzf gitea.tar.gz gitea

echo -e "${GREEN}Starting services...${NC}"
net start Postgres-PRE
#net start Postgres-PRE-Test
net start Postgres-PDE
net start MinIO-PRE
#net start MinIO-PRE-Test
net start MinIO-PDE
net start Redis-PRE
#net start Redis-PRE-Test
net start Redis-PDE
net start Gitea-PDE

echo -e "${BLUE}Completed${NC}"