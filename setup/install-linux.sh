#!/bin/bash
#Usage: ./install-linux.sh username

GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
basePath=$(pwd [-LP])
filePostgres="https://get.enterprisedb.com/postgresql/postgresql-10.11-2-linux-x64-binaries.tar.gz"
fileMinio="https://dl.min.io/server/minio/release/linux-amd64/minio"
fileRedis="https://github.com/iboware/redis-linux/archive/redis-linux.tar.gz"

#For Local Testing
# filePostgres="/home/iboware/Downloads/postgresql-10.11-2-linux-x64-binaries.tar.gz"
# fileMinio="/home/iboware/Downloads/minio"
# fileRedis="/home/iboware/Downloads/redis-linux-redis-linux.tar.gz"


postgresLocale="en_US.UTF8"
postgresPath="$basePath/programs/pgsql/bin"
programsPath="$basePath/programs"
programsPathEscape="${programsPath//\//\\/}"
dataPath="$basePath/data"
dataPathEscape="${dataPath//\//\\/}"
su_user=$(id -un)
user="primeapps"

if [ "$1" != "" ]; then
    user=$1
fi

# Create programs directory
mkdir programs
cd programs

# Install PostgreSQL
echo -e "${GREEN}Downloading PostgreSQL...${NC}"
curl $filePostgres -L --output postgresql.tar.gz
# cp $filePostgres postgresql.tar.gz
tar -zxvf postgresql.tar.gz
rm postgresql.tar.gz

# Install Minio
cd "$basePath/programs"
mkdir minio
cd minio
echo -e "${GREEN}Downloading Minio...${NC}"
curl $fileMinio -L --output minio
# cp $fileMinio minio
chown $user minio
chmod +x minio

# Install Redis
cd "$basePath/programs"
echo -e "${GREEN}Downloading Redis...${NC}"
curl $fileRedis -L --output redis.tar.gz
# cp $fileRedis redis.tar.gz
tar -zxvf redis.tar.gz
rm redis.tar.gz
mv redis-linux-redis-linux redis
cd redis
chown $user redis-server
chmod +x redis-server

# Init database instances
cd $postgresPath
echo -e "${GREEN}Initializing database instances...${NC}"

mkdir -p $basePath/data/pgsql_pre/
chown -R $user $basePath/data/pgsql_pre/
sudo -u $user bash -c "./initdb -D ${basePath}/data/pgsql_pre --no-locale --encoding=UTF8"

# Register database instances
echo -e "${GREEN}Registering database instances...${NC}"

cp "$basePath/service/postgres-pre.service" postgres-pre.service
sed -i "s/{{DATA}}/${dataPathEscape}/g" postgres-pre.service
sed -i "s/{{PROGRAMS}}/${programsPathEscape}/g" postgres-pre.service
sed -i "s/{{USER}}/${user}/g" postgres-pre.service
cp postgres-pre.service /etc/systemd/system/postgres-pre.service

systemctl start postgres-pre
systemctl enable postgres-pre

while ! echo exit | nc localhost 5436; do sleep 1; done

echo -e "${GREEN}Creating Postgres Role{NC}"
# Create postgres role
echo -e "${GREEN}Creating postgres role for database instances...${NC}"
sudo -u $user bash -c './psql -d postgres -h localhost -p 5436 -c "CREATE ROLE postgres SUPERUSER CREATEDB CREATEROLE LOGIN REPLICATION BYPASSRLS;"'
echo -e "${GREEN}Done...${NC}"

# Create databases
echo -e "${GREEN}Creating databases...${NC}"
sudo -u $user bash -c "./createdb -h localhost -U postgres -p 5436 --template=template0 --encoding=UTF8 --lc-ctype=${postgresLocale} --lc-collate=${postgresLocale} auth"
sudo -u $user bash -c "./createdb -h localhost -U postgres -p 5436 --template=template0 --encoding=UTF8 --lc-ctype=${postgresLocale} --lc-collate=${postgresLocale} platform"

# Restore databases
echo -e "${GREEN}Restoring databases...${NC}"
sudo -u $user bash -c "./pg_restore -h localhost -U postgres -p 5436 --no-owner --role=postgres -Fc -d auth ${basePath}/database/auth.bak"
sudo -u $user bash -c "./pg_restore -h localhost -U postgres -p 5436 --no-owner --role=postgres -Fc -d platform ${basePath}/database/platform.bak"

# Init storage instances
echo -e "${GREEN}Initializing storage instances...${NC}"
cd "$basePath/programs/minio"

cp "$basePath/service/minio-pre.service" minio-pre.service
sed -i "s/{{DATA}}/$dataPathEscape/g" minio-pre.service
sed -i "s/{{PROGRAMS}}/$programsPathEscape/g" minio-pre.service
sed -i "s/{{USER}}/$user/g" minio-pre.service

cp minio-pre.service /etc/systemd/system/minio-pre.service
mkdir -p $basePath/data/minio_pre/
chown $user $basePath/data/minio_pre
chmod u+rxw $basePath/data/minio_pre
systemctl start minio-pre
systemctl enable minio-pre

# Init cache instance
echo -e "${GREEN}Initializing cache instances...${NC}"
cd "$basePath/programs/redis"

mkdir -p "$basePath/data/redis_pre"
cp redis.conf "$basePath/data/redis_pre/redis.conf"

cp "$basePath/service/redis-pre.service" redis-pre.service
sed -i "s/{{DATA}}/$dataPathEscape/g" redis-pre.service
sed -i "s/{{PROGRAMS}}/$programsPathEscape/g" redis-pre.service
sed -i "s/{{USER}}/$user/g" redis-pre.service

cp redis-pre.service /etc/systemd/system/redis-pre.service
systemctl start redis-pre
systemctl enable redis-pre

# Create directory for dump, package, git, etc.
mkdir -p "$basePath/data/primeapps"

sleep 3 # Sleep 3 seconds for write database before backup

# Backup
echo -e "${GREEN}Compressing data folders...${NC}"
cd "$basePath/data"
tar -czf pgsql_pre.tar.gz pgsql_pre
tar -czf minio_pre.tar.gz minio_pre
tar -czf redis_pre.tar.gz redis_pre

echo -e "${BLUE}Completed${NC}"