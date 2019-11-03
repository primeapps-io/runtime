#!/bin/bash

if [[ "$OSTYPE" == "msys" ]]
then
	./uninstall-win.sh
else
	./uninstall-unix.sh
fi
