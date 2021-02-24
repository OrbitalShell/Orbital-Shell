#!orbsh
# build/public cli .net 5 release
proj
#cd OrbitalShell-CLI
dotnet publish --configuration Release --output bin/publish/net5.0/
