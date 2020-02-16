#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

cd ..

# Variables
basePath=$(pwd [-LP])
filePostgres=${PRIMEAPPS_FILE_POSTGRES:-"http://file.primeapps.io/binaries/linux/postgresql-10.11-2-linux-x64-binaries.tar.gz"}
fileMinio=${PRIMEAPPS_FILE_MINIO:-"http://file.primeapps.io/binaries/linux/minio"}
fileRedis=${PRIMEAPPS_FILE_REDIS:-"http://file.primeapps.io/binaries/linux/redis-linux-redis-linux.tar.gz"}
fileGitea=${PRIMEAPPS_FILE_GITEA:-"http://file.primeapps.io/binaries/linux/gitea-1.10.1-linux-amd64"}
