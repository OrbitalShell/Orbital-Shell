﻿#!orbsh
# build/publish orbital shell kernel packages

set version 1.0.9.nupkg

cls
echo "(b=darkgreen,f=black)                                             "
echo "(b=darkgreen,f=black) build/publish orbital shell kernel packages (rdc)"
echo "(b=darkgreen,f=black)                                             "
echo
proj
#dotnet build OrbitalShell.sln -c Release

#nuget-push OrbitalShell-ConsoleApp/bin/Debug/OrbitalShell-ConsoleApp.$version $key
#nuget-push OrbitalShell-Kernel/bin/Debug/OrbitalShell-Kernel.$version $key
#nuget-push OrbitalShell-Kernel-Commands/bin/Debug/OrbitalShell-Kernel-Commands.$version $key

nuget-push OrbitalShell-ConsoleApp/bin/Release/OrbitalShell-ConsoleApp.$version $key
nuget-push OrbitalShell-Kernel/bin/Release/OrbitalShell-Kernel.$version $key
nuget-push OrbitalShell-Kernel-Commands/bin/Release/OrbitalShell-Kernel-Commands.$version $key

echo "(b=darkgreen,f=yellow) Done. (rdc)"
