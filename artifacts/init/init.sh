#!/bin/bash
GREEN='\033[0;32m'
NC='\033[0m' # No Color
sleep 30

echo -e "${GREEN}Creating Cluster...${NC}"

echo "primeapps:$PGOPASSWORD:pgoadmin
primeappstest:$PGOPASSWORD:pgoadmin
primeappsreadonly:$PGOPASSWORD:pgoreader" > .pgouser

pgo create cluster -w $PGPASSWORD --metrics --pgbackrest --custom-config 'pgo-custom-pg-config' primeapps-database

until pg_isready; do echo Waiting for cluster...; sleep 15; done;

echo -e "${GREEN}Creating Databases...${NC}"
createdb --template=template0 --encoding=UTF8 auth
createdb --template=template0 --encoding=UTF8 platform

echo -e "${GREEN}Restoring Databases...${NC}"
pg_restore -Fc -d platform /root/platform.dmp
pg_restore -Fc -d auth /root/auth.dmp

echo -e "${GREEN}Done!${NC}"