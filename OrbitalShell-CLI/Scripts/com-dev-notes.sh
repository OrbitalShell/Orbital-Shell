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
nuget sources Add -Name githuborbsh -Source https://nuget.pkg.github.com/OrbitalShell/index.json

# github
# ------

ssh-keygen -t rsa -b 4096

# publid OrbitalShell-cli
# --------------------------------------------------------------

cd OrbitalShell-cli
# framework-dependent
dotnet publish --configuration Release --output bin/publish/netcoreapp3.1/
# framework-independent (self-contained)
dotnet publish --runtime win-x64 --configuration Release --output bin/publish/win-x64/
dotnet publish --runtime linux-x64 --configuration Release --output bin/publish/linux-x64/
dotnet publish --runtime linux-musl-x64 --configuration Release --output bin/publish/linux-musl-x64/
dotnet publish --runtime linux-arm --configuration Release --output bin/publish/linux-arm/
dotnet publish --runtime linux-arm64 --configuration Release --output bin/publish/linux-arm64/
dotnet publish --runtime osx-x64 --configuration Release --output bin/publish/osx-x64/

which dotnet
cd "C:/Program Files/dotnet/dotnet.exe"	

# get nuget binaries (.netcore 3.1 assemblies)
# --------------------------------------------------------------

# framework-dependent
nuget install orbitalshell
# framework-independent (self-contained)
nuget install orbitalshell-win-x64
nuget install orbitalshell-linux-x64
nuget install orbitalshell-linux-musl-x64
nuget install orbitalshell-linux-arm
nuget install orbitalshell-linux-arm64
nuget install orbitalshell-osx-x64
