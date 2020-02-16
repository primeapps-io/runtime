#!/bin/bash
BLUE='\033[0;34m'
RED='\033[0;31m'
NC='\033[0m' # No Color

cd migrator

result=$(./migrator update-${PRIMEAPPS_ENVIRONMENT:-'pre'})

echo "$result"

if [[ $result == *'"has_error": "true"'* ]]; then
    echo -e "${RED}Result has error!${NC}"
    exit 1
fi

echo -e "${BLUE}Completed${NC}"