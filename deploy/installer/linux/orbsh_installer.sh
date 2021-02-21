#!/usr/bin/env bash

# Orbital Shell Linux Installer 
# by Yobatman38 - < yobatman [ at ] gmail.com >
# Licensed under the MIT license.


## Config
DISTRIB="debian"
VERSION="10"

APP_PATH="/usr/local"
APP_NAME="Orbital-Shell"
BUILD_PATH="OrbitalShell-CLI/bin/Debug/netcoreapp3.1"


## Main script
echo -e "### Orbital Shell Linux Installer"
if [[ $EUID -ne 0 ]]; then
   echo "This script must be run as root" 1>&2
   exit 1
fi

if [[ -d "/usr/local/Orbital-Shell" ]]; then
    rm -R ${APP_PATH}/${APP_NAME}
fi

mkdir -p ${APP_PATH}/${APP_NAME}
cd ${APP_PATH}/${APP_NAME}

echo -e "\n--> Configure Microsoft key in APT repository"
wget https://packages.microsoft.com/config/${DISTRIB}/${VERSION}/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

echo -e "\n--> Update Linux"
apt-get update

echo -e "\n--> Install Linux Packages"
apt-get install -y apt-transport-https git dotnet-sdk-3.1 dotnet-runtime-3.1

echo -e "\n--> Get Orbital-Shell"
cd ${APP_PATH}
git clone https://github.com/OrbitalShell/Orbital-Shell.git

echo -e "\n--> Build Orbital Shell"
dotnet build ${APP_PATH}/${APP_NAME}/OrbitalShell.sln
chmod +x ${APP_PATH}/${APP_NAME}/${BUILD_PATH}/orbsh

if [[ -h "/bin/orbsh" ]]; then
    rm /bin/orbsh
fi

ln -s ${APP_PATH}/${APP_NAME}/${BUILD_PATH}/orbsh /bin/orbsh

echo -e "\n--> Run Orbital-Shell"
orbsh
