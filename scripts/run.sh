#!/bin/bash

# build solution

rep=`pwd`

cd /mnt/c/TFS/orbital-shell/
echo "lanching orbsh ..."
sudo dotnet run --no-build --no-restore --project OrbitalShell-CLI/OrbitalShell-CLI.csproj -v n

cd $rep
