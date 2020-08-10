#!/bin/bash

PORTAUTH=5000
PORTAPP=5001
PORTADMIN=5005

for i in "$@"
do
case $i in
    -pa=*|--port-auth=*)
    PORTAUTH="${i#*=}"
    ;;
    -pp=*|--port-app=*)
    PORTAPP="${i#*=}"
    ;;
    -pp=*|--port-admin=*)
    PORTADMIN="${i#*=}"
    ;;    
    *)
    # unknown option
    ;;
esac
done

netstatAuth=$(netstat -aon | findstr $PORTAUTH)
netstatApp=$(netstat -aon | findstr $PORTAPP)
netstatAdmin=$(netstat -aon | findstr $PORTADMIN)

/bin/kill -W -f "$(echo $netstatAuth | cut -d' ' -f5)"
/bin/kill -W -f "$(echo $netstatApp | cut -d' ' -f5)"
/bin/kill -W -f "$(echo $netstatAdmin | cut -d' ' -f5)"