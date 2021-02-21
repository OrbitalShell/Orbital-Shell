To run orbsh_installer.sh, you should do the following steps.

1. Configure the vars in the script's "Config" section, as for example :
    DISTRIB="debian"    # Or ubuntu, alpine and so on...
    VERSION="10"        # and their version

    APP_PATH="/usr/local"
    APP_NAME="Orbital-Shell"
    BUILD_PATH="OrbitalShell-CLI/bin/Debug/netcoreapp3.1"

2. Run the script : 
    sudo ./orbh_installer.sh