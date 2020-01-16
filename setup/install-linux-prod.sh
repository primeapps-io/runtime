#!/bin/bash
#Usage: ./install-linux.sh username

GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
basePath=$(pwd [-LP])
postgresLocale="en_US.UTF8"
postgresPath="$basePath/programs/pgsql/bin"

filePostgres="https://get.enterprisedb.com/postgresql/postgresql-10.11-2-linux-x64-binaries.tar.gz"
fileMinio="https://dl.min.io/server/minio/release/linux-amd64/minio"
fileRedis="https://github.com/iboware/redis-linux/archive/redis-linux.tar.gz"

fileDatabase="https://github.com/primeapps-io/pre/releases/download/v1.20.006.1/database.zip"
fileApp="https://github.com/primeapps-io/pre/releases/download/v1.20.006.1/PrimeApps.App.zip"
fileAuth="https://github.com/primeapps-io/pre/releases/download/v1.20.006.1/PrimeApps.Auth.zip"
fileAdmin="https://github.com/primeapps-io/pre/releases/download/v1.20.006.1/PrimeApps.Admin.zip"

dataPath="$basePath/data"
programsPath="$basePath/programs"

dataPathEscape="${dataPath//\//\\/}"
programsPathEscape="${programsPath//\//\\/}"

PORTAUTH=5000
PORTAPP=5001
PORTADMIN=5005
CLIENTID=primeapps_app
CLIENTSECRET=secret
APPDOMAIN="primeapps.app"
AUTHDOMAIN="primeapps.auth"
ADMINDOMAIN="primeapps.admin"

su_user=$(id -un)
user=$(logname)

# Create programs directory
mkdir programs
cd programs

# Install PostgreSQL
echo -e "${GREEN}Downloading PostgreSQL...${NC}"

if [ ! -f "postgresql.tar.gz" ]; then 
curl $filePostgres -L --output postgresql.tar.gz
fi
tar -zxvf postgresql.tar.gz

# Install Minio
cd "$basePath/programs"
mkdir minio
cd minio
echo -e "${GREEN}Downloading Minio...${NC}"
if [ ! -f "minio" ]; then 
curl $fileMinio -L --output minio
fi
chown $user minio
chmod +x minio

# Install Redis
cd "$basePath/programs"
echo -e "${GREEN}Downloading Redis...${NC}"
if [ ! -f "redis.tar.gz" ]; then 
curl $fileRedis -L --output redis.tar.gz
fi
tar -zxvf redis.tar.gz

mv redis-linux-redis-linux redis
cd redis
chown $user redis-server
chmod +x redis-server

# Get database
echo -e "${GREEN}Downloading Database...${NC}"

if [ ! -f "database.zip" ]; then 
curl $fileDatabase -L --output database.zip
fi
unzip database.zip -d $basePath
chown iboware --recursive $basePath/database

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

echo -e "${GREEN}Creating Postgres Role ${NC}"
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

apt -v &> /dev/null && apt install -y nginx 
which yum &> /dev/null && yum install -y  nginx

if [ ! -f "App.zip" ]; then 
curl $fileApp -L --output App.zip
fi
if [ ! -f "Auth.zip" ]; then
curl $fileAuth -L --output Auth.zip
fi
if [ ! -f "Admin.zip" ]; then
curl $fileAdmin -L --output Admin.zip
fi

mkdir -p $programsPath/App
unzip App.zip -d $programsPath/App
chown iboware --recursive $programsPath/App

mkdir -p $programsPath/Auth
unzip Auth.zip -d $programsPath/Auth
chown iboware --recursive $programsPath/Auth

mkdir -p $programsPath/Admin
unzip Admin.zip -d $programsPath/Admin
chown iboware --recursive $programsPath/Admin

echo -e "${GREEN}Creating Auth Service ${NC}"

cp "$basePath/service/primeapps-auth.service" primeapps-auth.service
sed -i "s/{{DIRECTORYPATH}}/$programsPathEscape/g" primeapps-auth.service
sed -i "s/{{AUTHPORT}}/$PORTAUTH/g" primeapps-auth.service
sed -i "s/{{USER}}/$user/g" primeapps-auth.service

cp primeapps-auth.service /etc/systemd/system/primeapps-auth.service

systemctl start primeapps-auth
systemctl enable primeapps-auth

echo -e "${GREEN}Creating App Service ${NC}"

cp "$basePath/service/primeapps-app.service" primeapps-app.service
sed -i "s/{{DIRECTORYPATH}}/$programsPathEscape/g" primeapps-app.service
sed -i "s/{{APPPORT}}/$PORTAPP/g" primeapps-app.service
sed -i "s/{{AUTHPORT}}/$PORTAUTH/g" primeapps-app.service
sed -i "s/{{USER}}/$user/g" primeapps-app.service
sed -i "s/{{CLIENTID}}/$CLIENTID/g" primeapps-app.service
sed -i "s/{{CLIENTSECRET}}/$CLIENTSECRET/g" primeapps-app.service

cp primeapps-app.service /etc/systemd/system/primeapps-app.service

systemctl start primeapps-app
systemctl enable primeapps-app

echo -e "${GREEN}Creating Admin Service ${NC}"

cp "$basePath/service/primeapps-admin.service" primeapps-admin.service
sed -i "s/{{DIRECTORYPATH}}/$programsPathEscape/g" primeapps-admin.service
sed -i "s/{{ADMINPORT}}/$PORTADMIN/g" primeapps-admin.service
sed -i "s/{{AUTHPORT}}/$PORTAUTH/g" primeapps-admin.service
sed -i "s/{{USER}}/$user/g" primeapps-admin.service
sed -i "s/{{CLIENTID}}/$CLIENTID/g" primeapps-admin.service
sed -i "s/{{CLIENTSECRET}}/$CLIENTSECRET/g" primeapps-admin.service

cp primeapps-admin.service /etc/systemd/system/primeapps-admin.service

systemctl start primeapps-admin
systemctl enable primeapps-admin


echo -e "${GREEN}Creating Auth Website ${NC}"

cp "$basePath/nginx/proxy-pass" $AUTHDOMAIN
sed -i "s/{{DOMAIN}}/$AUTHDOMAIN/g" $AUTHDOMAIN
sed -i "s/{{PORT}}/$PORTAUTH/g" $AUTHDOMAIN
cp $AUTHDOMAIN /etc/nginx/sites-available/$AUTHDOMAIN
sudo ln -s /etc/nginx/sites-available/$AUTHDOMAIN /etc/nginx/sites-enabled/$AUTHDOMAIN

echo -e "${GREEN}Creating App Website ${NC}"

cp "$basePath/nginx/proxy-pass" $APPDOMAIN
sed -i "s/{{DOMAIN}}/$APPDOMAIN/g" $APPDOMAIN
sed -i "s/{{PORT}}/$PORTAPP/g" $APPDOMAIN
cp $APPDOMAIN /etc/nginx/sites-available/$APPDOMAIN
sudo ln -s /etc/nginx/sites-available/$APPDOMAIN /etc/nginx/sites-enabled/$APPDOMAIN

echo -e "${GREEN}Creating Admin Website ${NC}"

cp "$basePath/nginx/proxy-pass" $ADMINDOMAIN
sed -i "s/{{DOMAIN}}/$ADMINDOMAIN/g" $ADMINDOMAIN
sed -i "s/{{PORT}}/$PORTADMIN/g" $ADMINDOMAIN
cp $ADMINDOMAIN /etc/nginx/sites-available/$ADMINDOMAIN
sudo ln -s /etc/nginx/sites-available/$ADMINDOMAIN /etc/nginx/sites-enabled/$ADMINDOMAIN

systemctl daemon-reload
echo -e "${BLUE}Completed${NC}"
