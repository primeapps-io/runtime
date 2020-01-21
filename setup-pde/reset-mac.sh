#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
cd ..
basePath=$(pwd [-LP])

echo -e "${GREEN}Stoping services...${NC}"
launchctl stop io.primeapps.postgres.pre
launchctl stop io.primeapps.postgres.pde
launchctl stop io.primeapps.postgres.pre-test
launchctl stop io.primeapps.minio.pre
launchctl stop io.primeapps.minio.pde
launchctl stop io.primeapps.minio.pre-test
launchctl stop io.primeapps.redis.pre
launchctl stop io.primeapps.redis.pde
launchctl stop io.primeapps.redis.pre-test
launchctl stop io.primeapps.gitea.pde

sleep 3 # Sleep 3 seconds for stop all services

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
rm -rf primeapps

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
launchctl start io.primeapps.postgres.pre
launchctl start io.primeapps.postgres.pde
#launchctl start io.primeapps.postgres.pre-test
launchctl start io.primeapps.minio.pre
launchctl start io.primeapps.minio.pde
#launchctl start io.primeapps.minio.pre-test
launchctl start io.primeapps.redis.pre
launchctl start io.primeapps.redis.pde
#launchctl start io.primeapps.redis.pre-test
launchctl start io.primeapps.gitea.pde

echo -e "${BLUE}Completed${NC}"