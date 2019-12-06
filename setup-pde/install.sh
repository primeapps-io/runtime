#!/bin/bash

if [[ "$OSTYPE" == "msys" ]]
then
	./install-win.sh
elif [[ "$OSTYPE" == "darwin"* ]]
then
    ./install-mac.sh	
else
	./install-linux.sh
fi
