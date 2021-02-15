#!orbsh
# commands - notes
# ------------------------------------------------------------------------------------------------

# list orbital shell nupkg packages files (after nuget install)
# -------------------------------------------------------------

cd $modules
nuget install orbitalshell -Prerelease
find . -p "*Orbital*lib*" -f -s
find . -p "*contentFiles*" -f -s

# nuget
# -----

nuget spec OrbitalShell.csproj
nuget pack OrbitalShell.nuspec

# publid OrbitalShell-cli
# --------------------------------------------------------------

cd OrbitalShell-cli
# framework-dependent
dotnet publish --configuration Release --output bin/publish/netcoreapp3.1/
# framework-independent
dotnet publish --runtime win-x64 --configuration Release --output bin/publish/win-x64/
dotnet publish --runtime linux-x64 --configuration Release --output bin/publish/linux-x64/

which dotnet
cd "C:/Program Files/dotnet/dotnet.exe"

# get nuget binaries (.netcore 3.1 assemblies)
# --------------------------------------------------------------

#...

