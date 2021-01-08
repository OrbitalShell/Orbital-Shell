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

    * <a href="#dwnghclone" class="js-scroll-trigger">Get the full source code from GitHub and the binary of the CLI (join to the project as an active team member!</a>

    * <a href="#dwnnuget" class="js-scroll-trigger">Get Orbital Shell SDK package from NuGet</a>

- type: paragraph.html
  section_id: dwnghclone
  title: Get the full source code from GitHub
#  background_style: bg-info
#  text_style: text-left text-white
  text-style: text-left
  text: |
    using <b>PowerShell</b>, <b>git</b>, <b>dotnet</b>, <b>vscode</b> or <b>Visual Studio</b>, get and compile the projects from the GitHub repository of the Orbital Shell project:

    <pre data-enlighter-language="shell" data-enlighter-linenumbers="false">
    cd MyProjects
    git clone https://github.com/franck-gaspoz/orbital-shell.git
    cd ./orbital-shell
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
    <pre data-enlighter-language="shell" data-enlighter-linenumbers="false">
    cd MyProject
    dotnet add package OrbitalShell --version 1.0.0.0      
    </pre>

    then you can use the Orbital Shell SDK from your C# code.    
---
