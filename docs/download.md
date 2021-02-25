---
layout: page
title: Download
#background_style: bg-info
#background_image: url('assets/img/backgrounds/image-from-rawpixel-id-1199650-jpeg.jpg')
# Add a link to the the top menu
menus:
  header:
    title: Download
    weight: 2

sections:
- type: paragraph.html
  section_id: help
  #background_image: url('assets/img/backgrounds/image-from-rawpixel-id-1199650-jpeg.jpg')
  #background-color: red
  title: Select your Orbital Shell preferred download
  text: >+
    There is the list of the available downloads of **Orbital Shell** :

    * <a href="#dwnghbin" class="js-scroll-trigger">Get the binary package for Windows 10 Any CPU from NuGet</a>
    
    * <a href="#dwnghclone" class="js-scroll-trigger">Get the full source code from GitHub and the binary of the CLI (join to the project as an active team member!)</a>
    
    * <a href="#dwncmp" class="js-scroll-trigger">Get the project, compile it and run Orbital Shell</a>

    * <a href="#dwnnuget" class="js-scroll-trigger">Get Orbital Shell SDK package from NuGet</a>

- type: rawparagraph.html
  section_id: dwnghbin
  title: Get the binary from NuGet - Windows 10 Any CPU
#  background_style: bg-info
#  text_style: text-left text-white
  text-style: text-left
  text: |
    using <b>PowerShell</b> and <b>nuget</b> get binary package for <b>Windows 10 Any CPU</b> of Orbital Shell:

    <pre data-enlighter-language="shell" data-enlighter-theme="site.data.settings.enjs_shell_theme" data-enlighter-linenumbers="false">
    cd $Env:ProgramFiles
    nuget install OrbitalShell -version 1.0.0-beta-preview-3
    cd OrbitalShell.1.0.0-beta-preview-3
    dir
    </pre>

    <pre data-enlighter-language="shell" data-enlighter-theme="rowhammer" data-enlighter-linenumbers="false">
    Mode                LastWriteTime         Length Name
    ----                -------------         ------ ----
    d-----       09/01/2021     18:16                Component
    d-----       09/01/2021     18:16                cs
    d-----       09/01/2021     18:16                de
    d-----       09/01/2021     18:16                Defaults
    d-----       09/01/2021     18:16                es
    d-----       09/01/2021     18:16                fr
    d-----       09/01/2021     18:16                it
    d-----       09/01/2021     18:16                ja
    d-----       09/01/2021     18:16                ko
    d-----       09/01/2021     18:16                pl
    d-----       09/01/2021     18:16                pt-BR
    d-----       09/01/2021     18:16                ru
    d-----       09/01/2021     18:16                Shell
    d-----       09/01/2021     18:16                tr
    d-----       09/01/2021     18:16                zh-Hans
    d-----       09/01/2021     18:16                zh-Hant
    -a----       08/10/2020     07:37          77824 dotnet-console-app-toolkit.dll
    -a----       08/10/2020     07:37          41160 dotnet-console-app-toolkit.pdb
    -a----       09/01/2021     12:29         185344 DotNetConsoleAppToolkit-Shell.dll
    -a----       09/01/2021     12:29          89080 DotNetConsoleAppToolkit-Shell.pdb
    -a----       07/05/2020     03:40        5249920 Microsoft.CodeAnalysis.CSharp.dll
    -a----       07/05/2020     03:40          31608 Microsoft.CodeAnalysis.CSharp.Scripting.dll
    -a----       07/05/2020     03:40        2372488 Microsoft.CodeAnalysis.dll
    -a----       07/05/2020     03:40         134008 Microsoft.CodeAnalysis.Scripting.dll
    -a----       09/01/2021     16:25          77824 OrbitalShell-ConsoleApp.dll
    -a----       09/01/2021     16:25          41224 OrbitalShell-ConsoleApp.pdb
    -a----       09/01/2021     16:25         185344 OrbitalShell-Kernel.dll
    -a----       09/01/2021     16:25          89056 OrbitalShell-Kernel.pdb
    -a----       09/01/2021     16:25          30874 orbsh.deps.json
    -a----       09/01/2021     16:25         206848 orbsh.dll
    -a----       09/01/2021     16:25         375296 orbsh.exe
    -a----       09/01/2021     16:25          10352 orbsh.pdb
    -a----       09/01/2021     16:25            214 orbsh.runtimeconfig.dev.json
    -a----       09/01/2021     16:25            154 orbsh.runtimeconfig.json
    </pre>

    add the path of Orbital Shell binaries to the system PATH (example) :

    <pre data-enlighter-language="shell" data-enlighter-theme="rowhammer" data-enlighter-linenumbers="false">
    C:\Program Files\OrbitalShell.1.0.0-beta-preview-3
    </pre>

    from a PowerShell console (for example: Windows Terminal), you can run <b>Orbital Shell</b> :

    <pre data-enlighter-language="shell" data-enlighter-theme="site.data.settings.enjs_shell_theme" data-enlighter-linenumbers="false">
    orbsh
    </pre>

- type: rawparagraph.html
  section_id: dwnghclone
  title: Get the full source code from GitHub
  background_style: bg-light
#  text_style: text-left text-white
  text-style: text-left
  text: |
    using <b>PowerShell</b> and <b>git</b> get the projects from the GitHub repository of the Orbital Shell project:

    <pre data-enlighter-language="shell" data-enlighter-theme="site.data.settings.enjs_shell_theme" data-enlighter-linenumbers="false">
    cd MyProjects
    git clone https://github.com/OrbitalShell/orbital-shell.git
    cd ./orbital-shell
    </pre>

- type: rawparagraph.html
  section_id: dwncmp
  title: Get, compile and run Orbital Shell
  #background_style: bg-light
#  text_style: text-left text-white
  text-style: text-left
  text: |
    using <b>PowerShell</b>,<b>dotnet</b> and <b>git</b>, get a clone of the Orbital Shell repository, compile it and run it:

    <p></p>
    <pre data-enlighter-language="shell" data-enlighter-theme="site.data.settings.enjs_shell_theme" data-enlighter-linenumbers="false">
    cd ~
    git clone https://github.com/OrbitalShell/orbital-shell.git
    cd ./orbital-shell
    dotnet build OrbitalShell.sln
    cd .OrbitalShell-CLI/bin/Debug/netcoreapp3.1
    ./orbsh.exe
    </pre>   

- type: rawparagraph.html
  section_id: dwnnuget
  title: Get Orbital Shell SDK package from NuGet
  background_style: bg-light
#  text_style: text-left text-white
  text-style: text-left
  text: |
    using <b>PowerShell</b>,<b>dotnet</b>, get and link to the Orbital Shell SDK nuget package from your C# project:

    <p></p>
    <pre data-enlighter-language="shell" data-enlighter-theme="site.data.settings.enjs_shell_theme" data-enlighter-linenumbers="false">
    cd MyProject
    dotnet add package OrbitalShell --version 1.0.0-beta-preview-2      
    </pre>

    then you can use the Orbital Shell SDK from your C# code.        
---
