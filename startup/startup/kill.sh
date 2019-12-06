#!/bin/bash
GREEN='\033[0;32m'
NC='\033[0m' # No Color

if [[ "$OSTYPE" == "msys" ]]
then
	./kill-win.sh "$@"
else 
	./kill-unix.sh "$@"
fi