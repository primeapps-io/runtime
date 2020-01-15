#!/bin/bash

if [[ "$OSTYPE" == "msys" ]]
then
	./uninstall-win.sh
elif [[ "$OSTYPE" == "darwin"* ]]
then
    ./uninstall-mac.sh	
else
	./uninstall-linux.sh
fi
