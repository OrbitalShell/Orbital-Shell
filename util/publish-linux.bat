@rem dotnet publish "C:\Users\franc\Documents\Visual Studio 2019\Projects\Applications\dotnet-console-app-toolkit\dotnet-console-app-toolkit-shell" --configuration Release --framework netcoreapp3.1 --output "C:\Users\franc\Documents\Visual Studio 2019\Projects\Applications\dotnet-console-app-toolkit\dotnet-console-app-toolkit-shell\bin\Release\linux-x64" --self-contained false --runtime linux-x64
@rem  --verbosity quiet
cd dotnet-console-app-toolkit-shell
dotnet build --runtime ubuntu.18.04-x64 -o "C:\Users\franc\Documents\Visual Studio 2019\Projects\Applications\dotnet-console-app-toolkit\dotnet-console-app-toolkit-shell\bin\Release\ubuntu.18.04-x64" -c Release
cd ..
@rem dir "C:\Users\franc\Documents\Visual Studio 2019\Projects\Applications\dotnet-console-app-toolkit\dotnet-console-app-toolkit-shell\bin\Release\ubuntu.18.04-x64"


