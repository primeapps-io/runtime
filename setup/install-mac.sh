#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

cd ..

# Variables
basePath=$(pwd [-LP])
filePostgres=${PRIMEAPPS_FILE_POSTGRES:-"http://file.primeapps.io/binaries/mac/postgresql-12.1-1-osx-binaries.zip"}
fileMinio=${PRIMEAPPS_FILE_MINIO:-"http://file.primeapps.io/binaries/mac/minio"}
fileRedis=${PRIMEAPPS_FILE_REDIS:-"http://file.primeapps.io/binaries/mac/redis-mac-5.0.7.zip"}
postgresLocale="en_US"
postgresPath="$basePath/programs/pgsql/bin"
programsPath="$basePath/programs"
programsPathEscape="${programsPath//\//\\/}"
dataPath="$basePath/data"
dataPathEscape="${dataPath//\//\\/}"
user=$(id -un)

# Create programs directory
mkdir programs
cd programs

# Install PostgreSQL
echo -e "${GREEN}Downloading PostgreSQL...${NC}"
curl $filePostgres -L --output postgres.zip
unzip postgres.zip
rm postgres.zip

# Install Minio
cd "$basePath/programs"
mkdir minio
cd minio
echo -e "${GREEN}Downloading Minio...${NC}"
curl $fileMinio -L --output minio
chmod 777 minio

# Install Redis
cd "$basePath/programs"
echo -e "${GREEN}Downloading Redis...${NC}"
curl $fileRedis -L --output redis.zip
unzip redis.zip
rm redis.zip
mv redis-mac-5.0.7 redis
cd redis
chmod 777 redis-server

# Init database instances
cd $postgresPath
echo -e "${GREEN}Initializing database instances...${NC}"
./initdb -D "$basePath/data/pgsql_pre" --no-locale --encoding=UTF8

# Register database instances
echo -e "${GREEN}Registering database instances...${NC}"

cp "$basePath/setup/plist/postgres-primeapps.plist" postgres-primeapps.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" postgres-primeapps.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" postgres-primeapps.plist
launchctl load postgres-primeapps.plist
cp postgres-primeapps.plist ~/Library/LaunchAgents/

# Wait Postgres wakeup
function timeout() { perl -e 'alarm shift; exec @ARGV' "$@"; }
timeout 15 bash -c 'until echo > /dev/tcp/localhost/5436; do sleep 1; done'

# Create postgres role
echo -e "${GREEN}Creating postgres role for database instances...${NC}"
./psql -d postgres -p 5436 -c "CREATE ROLE postgres SUPERUSER CREATEDB CREATEROLE LOGIN REPLICATION BYPASSRLS;"

# Create databases
echo -e "${GREEN}Creating databases...${NC}"
./createdb -h localhost -U postgres -p 5436 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale auth
./createdb -h localhost -U postgres -p 5436 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale platform

# Restore databases
echo -e "${GREEN}Restoring databases...${NC}"
./pg_restore -h localhost -U postgres -p 5436 --no-owner --role=postgres -Fc -d auth "$basePath/database/auth.bak"
./pg_restore -h localhost -U postgres -p 5436 --no-owner --role=postgres -Fc -d platform "$basePath/database/platform.bak"

# Init storage instances
echo -e "${GREEN}Initializing storage instances...${NC}"
cd "$basePath/programs/minio"

cp "$basePath/setup/plist/minio-primeapps.plist" minio-primeapps.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" minio-primeapps.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" minio-primeapps.plist
launchctl load minio-primeapps.plist
cp minio-primeapps.plist ~/Library/LaunchAgents/

# Init cache instance
echo -e "${GREEN}Initializing cache instances...${NC}"
cd "$basePath/programs/redis"

sed -i -e "s/stop-writes-on-bgsave-error yes/stop-writes-on-bgsave-error no/" redis.conf

mkdir "$basePath/data/redis_pre"
cp redis.conf "$basePath/data/redis_pre/redis.conf"

cp "$basePath/setup/plist/redis-primeapps.plist" redis-primeapps.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" redis-primeapps.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" redis-primeapps.plist
launchctl load redis-primeapps.plist
cp redis-primeapps.plist ~/Library/LaunchAgents/

# Create directory for dump, package, git, etc.
mkdir "$basePath/data/primeapps"

sleep 3 # Sleep 3 seconds for write database before backup

# Backup
echo -e "${GREEN}Compressing data folders...${NC}"
cd "$basePath/data"
tar -czf pgsql_pre.tar.gz pgsql_pre
tar -czf minio_pre.tar.gz minio_pre
tar -czf redis_pre.tar.gz redis_pre

echo -e "${BLUE}Completed${NC}"