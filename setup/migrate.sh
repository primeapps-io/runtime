#!/bin/bash

if [[ "$OSTYPE" == "msys" ]]
then
	./migrate-win.sh
elif [[ "$OSTYPE" == "darwin"* ]]
then
    ./migrate-mac.sh	
else
	./migrate-linux.sh
fi
