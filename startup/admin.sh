#!/bin/bash
GREEN='\033[0;32m'
NC='\033[0m' # No Color

echo -e "${GREEN}admin${NC}"

cd ..
cd PrimeApps.Admin
export ASPNETCORE_ENVIRONMENT=Production
export AppSettings__ClientSecret=secret

dotnet PrimeApps.Admin.dll --urls='http://localhost:5005'