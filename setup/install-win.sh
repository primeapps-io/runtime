#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
cd ..
basePath=$(pwd -W)
filePostgres="http://get.enterprisedb.com/postgresql/postgresql-11.5-2-windows-x64-binaries.zip"
fileMinio="https://dl.min.io/server/minio/release/windows-amd64/minio.exe"
fileRedis="https://github.com/microsoftarchive/redis/releases/download/win-3.0.504/Redis-x64-3.0.504.zip"
fileGitea="https://github.com/go-gitea/gitea/releases/download/v1.9.5/gitea-1.9.5-windows-4.0-amd64.exe"
fileWinSW="https://github.com/kohsuke/winsw/releases/download/winsw-v2.2.0/WinSW.NET4.exe"
postgresLocale="en-US"
postgresPath="$basePath/programs/pgsql/bin"

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
curl $fileMinio -L --output minio.exe

# Install Redis
cd "$basePath/programs"
mkdir redis
cd redis
echo -e "${GREEN}Downloading Redis...${NC}"
curl $fileRedis -L --output Redis-x64-3.0.504.zip
unzip Redis-x64-3.0.504.zip
rm Redis-x64-3.0.504.zip

# Install Gitea
cd "$basePath/programs"
mkdir gitea
cd gitea
echo -e "${GREEN}Downloading Gitea..${NC}."
curl $fileGitea -L --output gitea.exe

# Download WinSW
cd "$basePath/programs"
mkdir winsw
cd winsw
echo -e "${GREEN}Downloading WinSW...${NC}"
curl $fileWinSW -L --output winsw.exe

# Init database instances
cd $postgresPath
echo -e "${GREEN}Initializing database instances...${NC}"
./initdb -D "$basePath/data/pgsql_pre" --no-locale --encoding=UTF8
./initdb -D "$basePath/data/pgsql_pde" --no-locale --encoding=UTF8
./initdb -D "$basePath/data/pgsql_pre_test" --no-locale --encoding=UTF8

# Register database instances
echo -e "${GREEN}Registering database instances...${NC}"
./pg_ctl register -D "$basePath/data/pgsql_pre" -o "-F -p 5433" -N "Postgres-PRE"
./pg_ctl register -D "$basePath/data/pgsql_pde" -o "-F -p 5434" -N "Postgres-PDE"
./pg_ctl register -D "$basePath/data/pgsql_pre_test" -o "-F -p 5435" -N "Postgres-PRE-Test" -S "demand"

# Start database instances
echo -e "${GREEN}Starting database instances...${NC}"
net start "Postgres-PRE"
net start "Postgres-PDE"
net start "Postgres-PRE-Test"

# Create postgres role
echo -e "${GREEN}Creating postgres role for database instances...${NC}"
./psql -d postgres -p 5433 -c "CREATE ROLE postgres SUPERUSER CREATEDB CREATEROLE LOGIN REPLICATION BYPASSRLS;"
./psql -d postgres -p 5434 -c "CREATE ROLE postgres SUPERUSER CREATEDB CREATEROLE LOGIN REPLICATION BYPASSRLS;"
./psql -d postgres -p 5435 -c "CREATE ROLE postgres SUPERUSER CREATEDB CREATEROLE LOGIN REPLICATION BYPASSRLS;"

# Create databases
echo -e "${GREEN}Creating databases...${NC}"
./createdb -h localhost -U postgres -p 5433 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale auth
./createdb -h localhost -U postgres -p 5433 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale platform
./createdb -h localhost -U postgres -p 5434 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale studio
./createdb -h localhost -U postgres -p 5434 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale templet0
./createdb -h localhost -U postgres -p 5435 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale auth
./createdb -h localhost -U postgres -p 5435 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale platform

# Restore databases
echo -e "${GREEN}Restoring databases...${NC}"
./pg_restore -h localhost -U postgres -p 5433 --no-owner --role=postgres -Fc -d auth "$basePath/database/auth.bak"
./pg_restore -h localhost -U postgres -p 5433 --no-owner --role=postgres -Fc -d platform "$basePath/database/platform.bak"
./pg_restore -h localhost -U postgres -p 5434 --no-owner --role=postgres -Fc -d studio "$basePath/database/studio.bak"
./pg_restore -h localhost -U postgres -p 5434 --no-owner --role=postgres -Fc -d templet0 "$basePath/database/templet0.bak"
./pg_restore -h localhost -U postgres -p 5435 --no-owner --role=postgres -Fc -d auth "$basePath/database/auth.bak"
./pg_restore -h localhost -U postgres -p 5435 --no-owner --role=postgres -Fc -d platform "$basePath/database/platform.bak"

# Set templet0 db as template
echo -e "${GREEN}Setting templet0 db as template database...${NC}"
./psql -d postgres -p 5434 -c "UPDATE pg_database SET datistemplate = TRUE WHERE datname = 'templet0'; UPDATE pg_database SET datallowconn = FALSE WHERE datname = 'templet0';"

# Stop Postgres-PRE-Test, not required for now
net stop "Postgres-PRE-Test"

# Init storage instances
echo -e "${GREEN}Initializing storage instances...${NC}"
cd "$basePath/programs/minio"
cp "$basePath/programs/winsw/winsw.exe" minio-pre.exe
cp "$basePath/programs/winsw/winsw.exe" minio-pde.exe
cp "$basePath/programs/winsw/winsw.exe" minio-pre-test.exe
cp "$basePath/setup/minio-pre.xml" minio-pre.xml
cp "$basePath/setup/minio-pde.xml" minio-pde.xml
cp "$basePath/setup/minio-pre-test.xml" minio-pre-test.xml

./minio-pre.exe install
./minio-pde.exe install
./minio-pre-test.exe install

echo -e "${GREEN}Starting storage instances...${NC}"
net start "MinIO-PRE"
net start "MinIO-PDE"
net start "MinIO-PRE-Test"

# Stop Minio-PRE-Test, not required for now
sleep 3 # Sleep 3 seconds for minio wakeup
net stop "MinIO-PRE-Test"

# Init cache instance
echo -e "${GREEN}Initializing cache instances...${NC}"
cd "$basePath/programs/redis"
cp "$basePath/programs/winsw/winsw.exe" redis-pre.exe
cp "$basePath/programs/winsw/winsw.exe" redis-pde.exe
cp "$basePath/programs/winsw/winsw.exe" redis-pre-test.exe
cp "$basePath/setup/redis-pre.xml" redis-pre.xml
cp "$basePath/setup/redis-pde.xml" redis-pde.xml
cp "$basePath/setup/redis-pre-test.xml" redis-pre-test.xml

mkdir "$basePath/data/redis_pre"
mkdir "$basePath/data/redis_pde"
mkdir "$basePath/data/redis_pre_test"
cp redis.windows.conf "$basePath/data/redis_pre/redis.windows.conf"
cp redis.windows.conf "$basePath/data/redis_pde/redis.windows.conf"
cp redis.windows.conf "$basePath/data/redis_pre_test/redis.windows.conf"

./redis-pre.exe install
./redis-pde.exe install
./redis-pre-test.exe install

echo -e "${GREEN}Starting cache instances...${NC}"
net start "Redis-PRE"
net start "Redis-PDE"
net start "Redis-PRE-Test"

# Stop Redis-PRE-Test, not required for now
sleep 3 # Sleep 3 seconds for redis wakeup
net stop "Redis-PRE-Test"

# Init git instance
echo -e "${GREEN}Creating Gitea database...${NC}"
cd $postgresPath
./createdb -h localhost -U postgres -p 5434 --template=template0 --encoding=UTF8 --lc-ctype=$postgresLocale --lc-collate=$postgresLocale gitea

echo -e "${GREEN}Configuring app.ini...${NC}"
cd "$basePath/data"
mkdir gitea
cd "$basePath/data/gitea"
mkdir repositories
mkdir lfs
mkdir log

cd "$basePath/programs/gitea"
cp "$basePath/setup/app.ini" app.ini

hostname=$(hostname)
pathRepository="$basePath/data/gitea/repositories"
pathRepository=${pathRepository//\//\\/} # escape slash
pathLfs="$basePath/data/gitea/lfs"
pathLfs=${pathLfs//\//\\/} # escape slash
pathLog="$basePath/data/gitea/log"
pathLog=${pathLog//\//\\/} # escape slash
pathExe="$basePath/programs/gitea/gitea.exe"
pathExe=${pathExe//\//\\} # replace slash to backslash
pathAppIni="$basePath/programs/gitea/app.ini"
pathAppIni=${pathAppIni//\//\\} # replace slash to backslash

sed -i "s/{{RUN_USER}}/$hostname/" app.ini
sed -i "s/{{ROOT}}/$pathRepository/" app.ini
sed -i "s/{{LFS_CONTENT_PATH}}/$pathLfs/" app.ini
sed -i "s/{{ROOT_PATH}}/$pathLog/" app.ini

echo -e "${GREEN}Initializing git instance...${NC}"
sc create "Gitea-PDE" start=auto binPath=""$pathExe" web --config "$pathAppIni""

echo -e "${GREEN}Starting git instance...${NC}"
net start "Gitea-PDE"

echo -e "${GREEN}Creating admin user...${NC}"
curl -s -L http://localhost:3000 > /dev/null
sleep 5 # Sleep 5 seconds for gitea wakeup
./gitea admin create-user --username=primeapps --password='123456' --email='admin@primeapps.io' --admin=true --must-change-password=false --config="$pathAppIni"

echo -e "${GREEN}Creating template repository...${NC}"
curl -X POST "http://localhost:3000/api/v1/admin/users/primeapps/repos" -H "accept: application/json" -H "authorization: Basic cHJpbWVhcHBzOjEyMzQ1Ng==" -H "Content-Type: application/json" -d "{ \"auto_init\": false, \"name\": \"template\", \"private\": false}"

cd $basePath
cd ..
mkdir temp_primeapps
cd temp_primeapps

git clone http://primeapps:123456@localhost:3000/primeapps/template.git template
git clone https://git.primeapps.io/primeapps/template.git template-remote

cd template
mkdir components
mkdir functions
mkdir scripts

cp -a ../template-remote/components/. components/
cp -a ../template-remote/functions/. functions/
cp -a ../template-remote/scripts/. scripts/

git add .
git commit -m "Initial commit"
git push origin

cd $basePath
cd ..
rm -rf temp_primeapps

# Create directory for dump, package, git, etc.
mkdir "$basePath/data/primeapps"

sleep 3 # Sleep 3 seconds for write database before backup

# Backup
echo -e "${GREEN}Compressing data folders...${NC}"
cd "$basePath/data"
tar -czf pgsql_pre.tar.gz pgsql_pre
tar -czf pgsql_pde.tar.gz pgsql_pde
tar -czf pgsql_pre_test.tar.gz pgsql_pre_test
tar -czf minio_pre.tar.gz minio_pre
tar -czf minio_pde.tar.gz minio_pde
tar -czf minio_pre_test.tar.gz minio_pre_test
tar -czf redis_pre.tar.gz redis_pre
tar -czf redis_pde.tar.gz redis_pde
tar -czf redis_pre_test.tar.gz redis_pre_test
tar -czf gitea.tar.gz gitea

echo -e "${BLUE}Completed${NC}"