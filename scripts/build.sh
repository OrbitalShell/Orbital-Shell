#!/bin/bash

# build solution

rep=`pwd`

cd /mnt/c/TFS/orbital-shell/
dotnet build --no-restore ./OrbitalShell-Kernel-Commands/OrbitalShell-Kernel-Commands.csproj

# copy projects dll to CLI projects

# cp /mnt/c/TFS/orbital-shell/OrbitalShell-ConsoleApp/bin/Debug/net5.0/OrbitalShell-ConsoleApp.dll /mnt/c/TFS/orbital-shell/OrbitalShell-CLI/bin/Debug/net5.0/
# cp /mnt/c/TFS/orbital-shell/OrbitalShell-Kernel/bin/Debug/net5.0/OrbitalShell-Kernel.dll /mnt/c/TFS/orbital-shell/OrbitalShell-CLI/bin/Debug/net5.0/
cp /mnt/c/TFS/orbital-shell/OrbitalShell-Kernel-Commands/bin/Debug/net5.0/OrbitalShell-Kernel-Commands.dll /mnt/c/TFS/orbital-shell/OrbitalShell-CLI/bin/Debug/net5.0/
# cp /mnt/c/TFS/orbital-shell/OrbitalShell-UnitTests/bin/Debug/net5.0/OrbitalShell-UnitTests.dll /mnt/c/TFS/orbital-shell/OrbitalShell-CLI/bin/Debug/net5.0/
# cp /mnt/c/TFS/orbital-shell/OrbitalShell-WebAPI/bin/Debug/net5.0/OrbitalShell-WebAPI.dll /mnt/c/TFS/orbital-shell/OrbitalShell-CLI/bin/Debug/net5.0/

cd $rep
