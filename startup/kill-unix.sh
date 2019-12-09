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

lsof -ti:$PORTAUTH | xargs kill
lsof -ti:$PORTAPP | xargs kill
lsof -ti:$PORTADMIN | xargs kill