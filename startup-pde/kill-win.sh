#!/bin/bash

PORTAUTH=5020
PORTAPP=5021

for i in "$@"
do
case $i in
    -pa=*|--port-auth=*)
    PORTAUTH="${i#*=}"
    ;;
    -pp=*|--port-app=*)
    PORTAPP="${i#*=}"
    ;;
    *)
    # unknown option
    ;;
esac
done

netstatAuth=$(netstat -aon | findstr $PORTAUTH)
netstatApp=$(netstat -aon | findstr $PORTAPP)

/bin/kill -W -f "$(echo $netstatAuth | cut -d' ' -f5)"
/bin/kill -W -f "$(echo $netstatApp | cut -d' ' -f5)"