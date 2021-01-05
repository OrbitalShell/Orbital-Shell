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

    * [Get the full source code from GitHub and the binary of the CLI (join to the project as an active team member!)](#dwnghclone).

    * [Get Orbital Shell SDK package from NuGet](#dwnnuget).

- type: paragraph.html
  section_id: dwnghclone
  title: Get the full source code from GitHub
#  background_style: bg-info
#  text_style: text-left text-white
  text-style: text-left
  text: >
    using <b>PowerShell</b>, <b>git</b>, <b>dotnet</b>, <b>vscode</b> or <b>Visual Studio</b>, get and compile the projects from the GitHub repository of the Orbital Shell project:

    <pre data-enlighter-language="shell" >
      cd MyProjects
      git clone https://github.com/franck-gaspoz/orbital-shell.git
      cd ./orbital-shell
    </pre>

- type: rawparagraph.html
  section_id: dwnnuget
  title: Get Orbital Shell SDK package from NuGet
#  background_style: bg-info
#  text_style: text-left text-white
  text-style: text-left
  text: >+
    using <b>PowerShell</b>,<b>dotnet</b>, get and link to the Orbital Shell SDK nuget package from your C# project:

    <p></p>
    <pre data-enlighter-language="shell" >
      cd MyProject
      dotnet add package OrbitalShell --version 1.0.0.0      
    </pre>

    then you can use the Orbital Shell SDK from your C# code.

    <p></p>
    <h3>Sample: write a command relating to file system</h3> 

    First add the necessary imports:

    <p></p>
    <pre data-enlighter-language="csharp" data-enlighter-theme="classic" >
      using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
      using DotNetConsoleAppToolkit.Component.CommandLine.Parsing;
      using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
      using DotNetConsoleAppToolkit.Component.CommandLine.Variable;
      using DotNetConsoleAppToolkit.Console;
      using DotNetConsoleAppToolkit.Lib;
      using DotNetConsoleAppToolkit.Lib.Data;
      using DotNetConsoleAppToolkit.Lib.FileSystem;
      using System;
      using System.Collections.Generic;
      using System.Data;
      using System.IO;
      using System.Linq;
      using System.Threading;
      using System.Threading.Tasks;
      using static DotNetConsoleAppToolkit.Component.CommandLine.CommandLineReader.Interaction;
      using static DotNetConsoleAppToolkit.DotNetConsole;
      using static DotNetConsoleAppToolkit.Lib.FileSystem.FileSystem;
      using static DotNetConsoleAppToolkit.Lib.Str;
      using sc = System.Console;
    </pre>

    Then you can write a method and its body for your new command implementation:

    <p></p>
    <pre data-enlighter-language="csharp" data-enlighter-theme="classic" >
      [Command("search for files and/or folders")]
      public CommandResult<(List<FileSystemPath> items,FindCounts counts)> Find(
          CommandEvaluationContext context, 
          [Parameter("search path")] DirectoryPath path,
          [Option("p", "select names that matches the pattern", true, true)] string pattern,
          [Option("i", "if set and p is set, perform a non case sensisitive search")] bool ignoreCase,
          [Option("f","check pattern on fullname instead of name")] bool checkPatternOnFullName,
          [Option("c", "files that contains the string", true, true)] string contains,
          [Option("a", "print file system attributes")] bool attributes,
          [Option("s","print short pathes")] bool shortPathes,
          [Option("all", "select files and directories")] bool all,
          [Option("d", "select only directories")] bool dirs,
          [Option("t", "search in top directory only")] bool top
          )
        {
          if (path.CheckExists())
          {
            /* ... */
          }
          return new CommandResult<(List<FileSystemPath>, FindCounts)>( (new List<FileSystemPath>(),new FindCounts()) , ReturnCode.Error);
        } 
    </pre>


    If you build your code as a .net standard 2.1 class library, it will be usable as a module in Orbital Shell:

    <p></p>
    <pre data-enlighter-language="shell" >
      # orbsh
      module -l MyCommands.dll    
    </pre>
---
