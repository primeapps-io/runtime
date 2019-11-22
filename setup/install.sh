#!/bin/bash

if [[ "$OSTYPE" == "msys" ]]
then
	./install-win.sh
else
	./install-unix.sh
fi
