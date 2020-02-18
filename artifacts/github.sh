#!/bin/bash
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m' # No Color

rm -rf pre
rm -rf PrimeApps.Auth
rm -rf PrimeApps.App
rm -rf PrimeApps.Admin
rm -rf database
rm -rf setup
rm -rf startup
rm PrimeApps.Auth.zip
rm PrimeApps.App.zip
rm PrimeApps.Admin.zip
rm database.zip
rm setup.zip
rm startup.zip

git clone https://github.com/primeapps-io/pre.git

AUTH_URL="http://file.primeapps.io/pre/PrimeApps.Auth.zip"
APP_URL="http://file.primeapps.io/pre/PrimeApps.App.zip"
ADMIN_URL="http://file.primeapps.io/pre/PrimeApps.Admin.zip"
DATABASE_URL="http://file.primeapps.io/pre/database.zip"
SETUP_URL="http://file.primeapps.io/pre/setup.zip"
STARTUP_URL="http://file.primeapps.io/pre/startup.zip"

echo "Downloading $AUTH_URL"
wget $AUTH_URL

echo "Downloading $APP_URL"
wget $APP_URL

echo "Downloading $ADMIN_URL"
wget $ADMIN_URL

echo "Downloading $DATABASE_URL"
wget $DATABASE_URL

echo "Downloading $SETUP_URL"
wget $SETUP_URL

echo "Downloading $STARTUP_URL"
wget $STARTUP_URL

echo "Unzipping PrimeApps.Auth.zip"
unzip PrimeApps.Auth.zip -d PrimeApps.Auth

echo "Unzipping PrimeApps.App.zip"
unzip PrimeApps.App.zip -d PrimeApps.App

echo "Unzipping PrimeApps.Admin.zip"
unzip PrimeApps.Admin.zip -d PrimeApps.Admin

echo "Unzipping database.zip"
unzip database.zip

echo "Unzipping setup.zip"
unzip setup.zip

echo "Unzipping startup.zip"
unzip startup.zip

cp -R PrimeApps.Auth/. pre/PrimeApps.Auth
cp -R PrimeApps.App/. pre/PrimeApps.App
cp -R PrimeApps.Admin/. pre/PrimeApps.Admin
cp -R database/. pre/database
cp -R setup/. pre/setup
cp -R startup/. pre/startup

cd pre

git config user.name "PrimeApps"
git config user.email "admin@primeapps.io"

git add .
git commit -m "Updates and bugfixes"
git push https://primeappsio:76559642717f22a6384e0720df32813e1a477b2e@github.com/primeapps-io/pre.git

cd ..

rm -rf pre
rm -rf PrimeApps.Auth
rm -rf PrimeApps.App
rm -rf PrimeApps.Admin
rm -rf database
rm -rf setup
rm -rf startup
rm PrimeApps.Auth.zip
rm PrimeApps.App.zip
rm PrimeApps.Admin.zip
rm database.zip
rm setup.zip
rm startup.zip
