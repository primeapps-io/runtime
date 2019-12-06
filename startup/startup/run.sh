#!/bin/bash
GREEN='\033[0;32m'
NC='\033[0m' # No Color

./kill.sh "$@" &
./auth.sh "$@" &
./app.sh "$@" &
./admin.sh "$@"