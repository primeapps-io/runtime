#!/bin/bash

if [[ "$OSTYPE" == "msys" ]]
then
	./reset-win.sh
else
	./reset-unix.sh
fi
