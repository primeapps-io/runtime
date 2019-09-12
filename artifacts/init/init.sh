#!/bin/bash
GREEN='\033[0;32m'
NC='\033[0m' # No Color
sleep 30
echo -e "${GREEN}Creating Cluster...${NC}"
pgo create cluster -w 'pr!m€Appsi0d' --metrics --pgbackrest --custom-config 'pgo-custom-pg-config' primeapps-database
echo -e "${GREEN}Done.${NC}"

until pg_isready; do echo waiting for cluster; sleep 15; done;


echo -e "${GREEN}Creating Databases:${NC}"
createdb --template=template0 --encoding=UTF8 auth
createdb --template=template0 --encoding=UTF8 platform
echo -e "${GREEN}Done.${NC}"

echo -e "${GREEN}Restoring Databases:${NC}"
pg_restore -w -Fc --clean -e -d platform /root/platform.dmp
pg_restore -w -Fc --clean -e -d auth /root/auth.dmp
echo -e "${GREEN}Done.${NC}"
