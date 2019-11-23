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

kill $(netstat -aon | findstr $PORTAUTH)
kill $(netstat -aon | findstr $PORTAPP)