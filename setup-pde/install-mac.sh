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
fileGitea=${PRIMEAPPS_FILE_GITEA:-"http://file.primeapps.io/binaries/mac/gitea-1.10.1-darwin-10.6-amd64"}
postgresLocale="en_US"
postgresPath="$basePath/programs/pgsql/bin"
programsPath="$basePath/programs"
programsPathEscape=${programsPath//\//\\/}
dataPath="$basePath/data"
dataPathEscape=${dataPath//\//\\/}
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

# Install Gitea
cd "$basePath/programs"
mkdir gitea
cd gitea
echo -e "${GREEN}Downloading Gitea..${NC}."
curl $fileGitea -L --output gitea
chmod 777 gitea

# Init database instances
cd $postgresPath
echo -e "${GREEN}Initializing database instances...${NC}"
./initdb -D "$basePath/data/pgsql_pre" --no-locale --encoding=UTF8
./initdb -D "$basePath/data/pgsql_pde" --no-locale --encoding=UTF8
./initdb -D "$basePath/data/pgsql_pre_test" --no-locale --encoding=UTF8

# Register database instances
echo -e "${GREEN}Registering database instances...${NC}"

cp "$basePath/setup-pde/plist/postgres-pre.plist" postgres-pre.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" postgres-pre.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" postgres-pre.plist
launchctl load postgres-pre.plist
cp postgres-pre.plist ~/Library/LaunchAgents/

cp "$basePath/setup-pde/plist/postgres-pde.plist" postgres-pde.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" postgres-pde.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" postgres-pde.plist
launchctl load postgres-pde.plist
cp postgres-pde.plist ~/Library/LaunchAgents/

cp "$basePath/setup-pde/plist/postgres-pre-test.plist" postgres-pre-test.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" postgres-pre-test.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" postgres-pre-test.plist
launchctl load postgres-pre-test.plist
launchctl start io.primeapps.postgres.pre-test
cp postgres-pre-test.plist ~/Library/LaunchAgents/

sleep 3 # Sleep 3 seconds for postgres services wakeup

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
./psql -d postgres -h localhost -p 5434 -c "UPDATE pg_database SET datistemplate = TRUE WHERE datname = 'templet0'; UPDATE pg_database SET datallowconn = FALSE WHERE datname = 'templet0';"

# Stop Postgres-PRE-Test, not required for now
launchctl stop io.primeapps.postgres.pre-test

# Init storage instances
echo -e "${GREEN}Initializing storage instances...${NC}"
cd "$basePath/programs/minio"

cp "$basePath/setup-pde/plist/minio-pre.plist" minio-pre.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" minio-pre.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" minio-pre.plist
launchctl load minio-pre.plist
cp minio-pre.plist ~/Library/LaunchAgents/

cp "$basePath/setup-pde/plist/minio-pde.plist" minio-pde.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" minio-pde.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" minio-pde.plist
launchctl load minio-pde.plist
cp minio-pde.plist ~/Library/LaunchAgents/

cp "$basePath/setup-pde/plist/minio-pre-test.plist" minio-pre-test.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" minio-pre-test.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" minio-pre-test.plist
launchctl load minio-pre-test.plist
launchctl start io.primeapps.minio.pre-test
cp minio-pre-test.plist ~/Library/LaunchAgents/

# Stop Minio-PRE-Test, not required for now
sleep 3 # Sleep 3 seconds for minio wakeup
launchctl stop io.primeapps.minio.pre-test

# Init cache instance
echo -e "${GREEN}Initializing cache instances...${NC}"
cd "$basePath/programs/redis"

sed -i -e "s/stop-writes-on-bgsave-error yes/stop-writes-on-bgsave-error no/" redis.conf

mkdir "$basePath/data/redis_pre"
mkdir "$basePath/data/redis_pde"
mkdir "$basePath/data/redis_pre_test"
cp redis.conf "$basePath/data/redis_pre/redis.conf"
cp redis.conf "$basePath/data/redis_pde/redis.conf"
cp redis.conf "$basePath/data/redis_pre_test/redis.conf"

cp "$basePath/setup-pde/plist/redis-pre.plist" redis-pre.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" redis-pre.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" redis-pre.plist
launchctl load redis-pre.plist
cp redis-pre.plist ~/Library/LaunchAgents/

cp "$basePath/setup-pde/plist/redis-pde.plist" redis-pde.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" redis-pde.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" redis-pde.plist
launchctl load redis-pde.plist
cp redis-pde.plist ~/Library/LaunchAgents/

cp "$basePath/setup-pde/plist/redis-pre-test.plist" redis-pre-test.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" redis-pre-test.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" redis-pre-test.plist
launchctl load redis-pre-test.plist
launchctl start io.primeapps.redis.pre-test
cp redis-pre-test.plist ~/Library/LaunchAgents/

# Stop Redis-PRE-Test, not required for now
sleep 3 # Sleep 3 seconds for redis wakeup
launchctl stop io.primeapps.redis.pre-test

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

pathRepository="$basePath/data/gitea/repositories"
pathRepository=${pathRepository//\//\\/} # escape slash
pathLfs="$basePath/data/gitea/lfs"
pathLfs=${pathLfs//\//\\/} # escape slash
pathLog="$basePath/data/gitea/log"
pathLog=${pathLog//\//\\/} # escape slash
pathAppIni="$basePath/programs/gitea/app.ini"

sed -i -e "s/{{RUN_USER}}/$user/" app.ini
sed -i -e "s/{{ROOT}}/$pathRepository/" app.ini
sed -i -e "s/{{LFS_CONTENT_PATH}}/$pathLfs/" app.ini
sed -i -e "s/{{ROOT_PATH}}/$pathLog/" app.ini

cp "$basePath/setup-pde/plist/gitea-pde.plist" gitea-pde.plist
sed -i -e "s/{{DATA}}/$dataPathEscape/" gitea-pde.plist
sed -i -e "s/{{PROGRAMS}}/$programsPathEscape/" gitea-pde.plist
launchctl load gitea-pde.plist
cp gitea-pde.plist ~/Library/LaunchAgents/

echo -e "${GREEN}Creating admin user...${NC}"

while ! echo exit | lsof -i :3000; do sleep 1; done # try to wakeup gitea

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