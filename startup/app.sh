#!/bin/bash
GREEN='\033[0;32m'
NC='\033[0m' # No Color

PORTAUTH=5000
PORTAPP=5001
CLIENTID=primeapps_app

for i in "$@"
do
case $i in
    -pa=*|--port-auth=*)
    PORTAUTH="${i#*=}"
    ;;
    -pp=*|--port-app=*)
    PORTAPP="${i#*=}"
    ;;
    -ci=*|--client-id=*)
    CLIENTID="${i#*=}"
    ;;
    *)
    # unknown option
    ;;
esac
done

echo -e "${GREEN}app${NC}"

cd ..
cd PrimeApps.App

export ASPNETCORE_ENVIRONMENT=Development
export AppSettings__ClientId=$CLIENTID
export AppSettings__ClientSecret=secret
export AppSettings__AuthenticationServerURL="http://localhost:$PORTAUTH"

dotnet PrimeApps.App.dll --urls="http://localhost:$PORTAPP"