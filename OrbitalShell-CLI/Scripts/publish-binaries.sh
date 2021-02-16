#!porbsh
# build/publish orbital shell binaries

set version 1.0.5
set nugetext .nupkg

cls
echo "(b=darkgreen,f=black)                                      "
echo "(b=darkgreen,f=black) build/publish orbital shell binaries (rdc)"
echo "(b=darkgreen,f=black)                                      "
echo
proj
cd OrbitalShell-CLI

echo "(br)(b=darkgreen,f=black)dotnet publish(rdc)(br)"

# framework-dependent

echo "(br)(f=green) publish framework-dependent binaries: (b=darkgreen,f=yellow)netcoreapp3.1 (rdc)(br)"
dotnet publish --configuration Release --output bin/publish/netcoreapp3.1/

# framework-independent (self-contained) - current plateforms

echo "(br)(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)netcoreapp3.1(rdc)(br)"
dotnet publish --runtime win-x64 --configuration Release --output bin/publish/win-x64/

echo "(br)(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)linux-x64(rdc)(br)"
#dotnet publish --runtime linux-x64 --configuration Release --output bin/publish/linux-x64/

echo "(br)(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)linux-musl-x64(rdc)(br)"
#dotnet publish --runtime linux-musl-x64 --configuration Release --output bin/publish/linux-musl-x64/

echo "(br)(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)linux-arm(rdc)(br)"
#dotnet publish --runtime linux-arm --configuration Release --output bin/publish/linux-arm/

echo "(br)(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)linux-arm64(rdc)(br)"
#dotnet publish --runtime linux-arm64 --configuration Release --output bin/publish/linux-arm64/

echo "(br)(f=green)publish self-contained binaries: (b=darkgreen,f=yellow)osx-x64(rdc)(br)"
#dotnet publish --runtime osx-x64 --configuration Release --output bin/publish/osx-x64/

# total ~ 250 Mo

cd build
echo "(br)(b=darkgreen,f=black)nuget publish(rdc)(br)"

echo "(br)(f=green)nuget publish framework-dependent binaries: (b=darkgreen,f=yellow)netcoreapp3.1 (rdc)(br)"
echo "(b=darkgreen,f=yellow)PACK(rdc)"
nuget pack OrbitalShell.nuspec -Properties version=$version
echo "(b=darkgreen,f=yellow)PUSH(rdc)"
nuget-push OrbitalShell.$version$nugetext $key

echo "(br)(f=green)nuget publish self-contained binaries: (b=darkgreen,f=yellow)win-x64(rdc)(br)"
echo "(b=darkgreen,f=yellow)PACK(rdc)"
nuget pack OrbitalShell-win-x64.nuspec -Properties version=$version
echo "(b=darkgreen,f=yellow)PUSH(rdc)"
nuget-push OrbitalShell-win-x64.$version$nugetext $key

echo "(br)(f=green)nuget publish self-contained binaries: (b=darkgreen,f=yellow)linux-x64(rdc)(br)"
echo "(b=darkgreen,f=yellow)PACK(rdc)"
nuget pack OrbitalShell-linux-x64.nuspec -Properties version=$version
echo "(b=darkgreen,f=yellow)PUSH(rdc)"
nuget-push OrbitalShell-linux-x64.$version$nugetext $key

echo "(br)(f=green)nuget publish self-contained binaries: (b=darkgreen,f=yellow)linux-musl-x64(rdc)(br)"
echo "(b=darkgreen,f=yellow)PACK(rdc)"
nuget pack OrbitalShell-linux-musl-x64.nuspec -Properties version=$version
echo "(b=darkgreen,f=yellow)PUSH(rdc)"
nuget-push OrbitalShell-linux-musl-x64.$version$nugetext $key

echo "(br)(f=green)nuget publish self-contained binaries: (b=darkgreen,f=yellow)linux-arm(rdc)(br)"
echo "(b=darkgreen,f=yellow)PACK(rdc)"
nuget pack OrbitalShell-linux-arm.nuspec -Properties version=$version
echo "(b=darkgreen,f=yellow)PUSH(rdc)"
nuget-push OrbitalShell-linux-arm.$version$nugetext $key

echo "(br)(f=green)nuget publish self-contained binaries: (b=darkgreen,f=yellow)linux-arm64(rdc)(br)"
echo "(b=darkgreen,f=yellow)PACK(rdc)"
nuget pack OrbitalShell-linux-arm64.nuspec -Properties version=$version
echo "(b=darkgreen,f=yellow)PUSH(rdc)"
nuget-push OrbitalShell-linux-arm64.$version$nugetext $key

echo "(br)(f=green)nuget publish self-contained binaries: (b=darkgreen,f=yellow)osx-x64(rdc)(br)"
echo "(b=darkgreen,f=yellow)PACK(rdc)"
nuget pack OrbitalShell-osx-x64.nuspec -Properties version=$version
echo "(b=darkgreen,f=yellow)PUSH(rdc)"
nuget-push OrbitalShell-osx-x64.$version$nugetext $key
