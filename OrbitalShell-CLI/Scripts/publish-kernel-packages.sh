#!orbsh
# build/publish orbital shell kernel packages

set version 1.0.7.nupkg

cls
echo "(b=darkgreen,f=black)                                             "
echo "(b=darkgreen,f=black) build/publish orbital shell kernel packages (rdc)"
echo "(b=darkgreen,f=black)                                             "
echo
proj
dotnet build OrbitalShell.sln -c Debug

nuget-push OrbitalShell-ConsoleApp/bin/Debug/net5.0/OrbitalShell-ConsoleApp.$version $key
nuget-push OrbitalShell-Kernel/bin/Debug/net5.0/OrbitalShell-Kernel.$version $key
nuget-push OrbitalShell-Kernel-Commands/bin/Debug/net5.0/OrbitalShell-Kernel-Commands.$version $key

echo "(b=darkgreen,f=yellow) Done. (rdc)"
