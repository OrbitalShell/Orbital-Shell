#!porbsh
# build/publish orbital shell binaries
cls
echo "(b=darkgreen,f=black)build/publish orbital shell binaries(rdc)"
proj
cd OrbitalShell-CLI

# framework-dependent

echo "(f=green)publish framework-dependent binaries: (b=darkgreen,f=yellow)netcoreapp3(rdc)"
dotnet publish --configuration Release --output bin/publish/netcoreapp3.1/

# framework-independent (self-contained) - current plateforms

echo "(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)netcoreapp3(rdc)"
dotnet publish --runtime win-x64 --configuration Release --output bin/publish/win-x64/

echo "(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)linux-x64(rdc)"
dotnet publish --runtime linux-x64 --configuration Release --output bin/publish/linux-x64/

echo "(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)linux-musl-x64(rdc)"
dotnet publish --runtime linux-musl-x64 --configuration Release --output bin/publish/linux-musl-x64/

echo "(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)linux-arm(rdc)"
dotnet publish --runtime linux-arm --configuration Release --output bin/publish/linux-arm/

echo "(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)linux-arm64(rdc)"
dotnet publish --runtime linux-arm64 --configuration Release --output bin/publish/linux-arm64/

echo "(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)osx-x64(rdc)"
dotnet publish --runtime osx-x64 --configuration Release --output bin/publish/osx-x64/

# total ~ 250 Mo
