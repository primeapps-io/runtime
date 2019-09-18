#!/bin/bash
GREEN='\033[0;32m';
NC='\033[0m'; # No Color
sleep 15

create_and_wait_cluster(){
    echo -e "${GREEN}Creating Cluster...${NC}"
    pgo create cluster -w $PGPASSWORD --metrics --pgbackrest --custom-config 'pgo-custom-pg-config' primeapps-database

    until pg_isready; do echo Waiting for cluster...; sleep 15; done;
}

create_and_restore_databases(){
    echo -e "${GREEN}Creating Databases...${NC}"
    createdb --template=template0 --encoding=UTF8 auth
    createdb --template=template0 --encoding=UTF8 platform

    echo -e "${GREEN}Restoring Databases...${NC}"
    pg_restore -Fc -d platform /root/platform.dmp
    pg_restore -Fc -d auth /root/auth.dmp
}

main(){
echo -e "${GREEN}Checking Cluster...${NC}"
if pg_isready; then
    echo -e "${GREEN}Cluster is ready.${NC}"
    echo -e "${GREEN}Checking databases...${NC}"
    if psql -lqt | cut -d \| -f 1 | grep -qw platform; then
        echo -e "${GREEN}Databases are ready.${NC}"
    else
        create_and_restore_databases
    fi
else
    create_and_wait_cluster
    create_and_restore_databases
fi
echo -e "${GREEN}Done!${NC}"
}

main