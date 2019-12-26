#!/bin/bash

if [[ "$OSTYPE" == "msys" ]]
then
	./reset-win.sh
elif [[ "$OSTYPE" == "darwin"* ]]
then
    ./reset-mac.sh	
else
	./reset-linux.sh
fi
