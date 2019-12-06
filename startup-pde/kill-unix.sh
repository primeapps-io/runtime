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

lsof -ti:$PORTAUTH | xargs kill
lsof -ti:$PORTAPP | xargs kill