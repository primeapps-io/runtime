#!/bin/bash
GREEN='\033[0;32m'
YELLOW='\033[0;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Variables
os="linux"

if [[ "$OSTYPE" == "msys" ]]
then
	os="win"
elif [[ "$OSTYPE" == "darwin"* ]]
then
	os="osx"
fi

# Get parameters
for i in "$@"
do
case $i in
    -c=*|--connection-string=*)
    connectionString="${i#*=}"
    ;;
    *)
    # unknown option
    ;;
esac
done

# Publish Migrator
cd ../PrimeApps.Migrator
echo -e "${GREEN}Publishing Migrator...${NC}"
dotnet publish "PrimeApps.Migrator.csproj" --self-contained false --runtime "$os-x64" -c Debug

# Run PRE migrate command
cd bin/Debug/netcoreapp2.2/"$os-x64"/publish
echo -e "${GREEN}Running Migrator (PDE)...${NC}"
./migrator update-pde $connectionString
echo -e "${GREEN}Running Migrator (PRE)...${NC}"
./migrator update-pre $connectionString

