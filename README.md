## Orbital Shell

<img src="docs/assets/img/robotazteque.png" align="top" style="float:left;margin-right:8px"/> | <b>Orbital Shell</b> is a multi-plateform (**windows, linux, macos, arm**) command shell (according to .Net Core supported platforms and APIs compatibilities), inspired by <b><i>bash</i></b> and **POSIX** recommendations.<br><br>It provides any usual bash shell feature (even if modernized) and 'user friendly' syntaxes allowing to access (get/set/call) C# objects.<br><br>Developed using **C# 8, .NET Core 3.1/Net 5 and .NET Standard 2.1**
-- | --
&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; | &nbsp;

This shell integrates the <u>most usefull shell commands</u>, and is intented to be extended by coding new commands or downloading new commands modules within a repository of modules. Of course it can be enterly customized by using the features integrated to the shell (<u>scripts, functions, commands, aliases, settings, parametrization,...</u>). Having a strong <u>ANSI/VT-100-220-300-500</u> support, it provides structured and colorized display of data and information (support of <u>ASCII, Unicode and 24 bits colors</u>).

## About the project

Find any information and documentation about this project on the project's Web Site @ [Orbital SHell Git-Pages](https://franck-gaspoz.github.io/orbital-shell) 

 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; | <img src="docs/assets/img/2021-02-12 03_47_28-Window.png"/>
-- | --

Developers and users manuals are available in the project web site @ [Orbital SHell Git-Pages (documentation)](https://franck-gaspoz.github.io/orbital-shell/documentation) 

> [![licence mit](https://img.shields.io/badge/licence-MIT-blue.svg)](license) This project is licensed under the terms of the MIT license: [LICENSE](LICENSE)  

> <u>project repositories status:</u><br>
![orbital-shell](https://img.shields.io/badge/orbital--shell-repository-lightgrey?style=plastic)
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/orbital-shell?style=plastic)
![version](https://img.shields.io/badge/Version-1.0.1--beta4-green)
![version](https://img.shields.io/github/v/tag/franck-gaspoz/orbital-shell?style=plastic) 

## Features

<img src="docs/assets/img/git-doc/orbital-shell.png"/>*a view of the shell running in Windows Terminal*


- Is a '**bash** style' POSIX command shell
- The **modules** that are integrated into the shell provide **the most usefull shell commands** (ls/dir,rm,mv,cp,find,echo,clear,more,env,set,export,history,alias,...), tests commands, **text editor command**, and so on... , covering the fields of file system,text files,console output, data management, user input
  - modules can contains any item that the shell is intended to handle: commands, **hooks**, scripts/functions, settings, help, doc, assets, ...
  - dedicated repositories are used to get known modules list
  - modules are versioned and delivered throught **nupkg** packets, currently hosted at **NuGet**
- _The shell command engine implements:_
  - A **command line reader** with usual input keyboard shortcuts, auto completion and input feedback (colorization)
  - **Streams** standard input (*stdIn*), standard output (*stdOut*), and standard error (*StdErr*)
  - **Streams redirections** ( **<** **>** )
  - <u>**pipelines** of commands:</u>aa
    - sequences of commands ( **|** ) and conditional sequences of commands ( **&&**, **||** )
    - groups of commands **(** .. **;** .. **)**
    - background execution ( **&** )
    - commands return a **result code** and can returns **values**. When returning values and when specified, a pipe ( **|** ) between commands can **pass objects instead of streams**
  - <u>**Batches of commands**</u> (scripts)   
    - **Cancellation** of commands execution **Task** (Ctrl-Z)
    - traditional batch of shell commands (.sh,.orbsh)
    - C# language scripts (.cssh)
  - <u>**Variables**</u>
    - Variables can store any data of **any C# object type**
      - The command line syntax has been extended to **support an object notation to indicate accessing an object member or invoking an object method**
      - The **environment variables** are of type **String**
      - Are getted and setted with **$** (in any command arg), **set** and **=**
    - **Functions** can be defined by a **shell script** or by a **C# compiled source**
    - **Aliases** for command names
    - **Shell environment** : the shell initialization feature deploy and setup a shell environment for the current user (profile folder and initialization scripts: *.profile*,*.alias*,*.history*,*.orbsh*)
    - **Extensibility**:
      - The default command line grammar can be changed to get other syntaxes (***zsh***, ***DOS***,...) or a new one
      - The command line engine can be overriden and extended to enhance the existing features (auto complete inputs,parsing inputs,running commands,...)
    - **A simple way to define shell commands** using C# method and parameters attributes, avoiding the developer to handle syntax analyzing and shell integration (command help, pipelines, standard stream redirections) councerns, allowing to support either simple values types (int,float,string,date time,..) and object types (even generic collection), and that can interacts together and with the shell throught data objects

    ``` csharp
    // a Unix rm command implementation:
    [Command("remove file(s) and/or the directory(ies)")]
    public 
        CommandResult<(List<FileSystemPath> items, FindCounts counts)> 
        Rm(
            CommandEvaluationContext context,
            [Parameter("file or folder path")] WildcardFilePath path,
            [Option("r", "recurse", "also remove files and folders in sub directories")] bool recurse,
            [Option("i", "interactive", "prompt before any removal")] bool interactive,
            [Option("v", "verbose", "explain what is being done")] bool verbose,
            [Option("e", "delete-empty", "remove empty directories")] bool rmEmptyDirs,
            [Option("s", "short", "short display: do not print file system attributes when verbose")] bool noattributes,
            [Option("m", "simulate", "don't remove any file/or folder, just simulate the operation (enable verbose)")] bool simulate
        )
    {
        var r = new List<FileSystemPath>();
        var counts = new FindCounts();
        if (path.CheckExists(context))
        {
            var items = FindItems(context, path.FullName, path.WildCardFileName ?? "*", !recurse, true, false, !noattributes, !recurse, null, false, counts, false, false);
            
            /* ... */
            
            return new CommandResult<(List<FileSystemPath>, FindCounts)>((r, counts), ReturnCode.OK);
        }
        else
            return new CommandResult<(List<FileSystemPath>, FindCounts)>((r, counts), ReturnCode.Error);
    }
    ```

  - such a command will be auto documented like this by the shell **help command**:
  
    <img src="docs/assets/img/git-doc/2021-02-12%2014_28_35-Window.png"/>


## Orbital shell uses these libraries:

- Microsoft.CodeAnalysis.CSharp.Scripting
- NewtonSoft.Json
- Octopus (module GitHubRestApi)

## *notice / advert*

> The project orbital shell was dependent of the project **DotNetConsoleAppToolkit**. Now the library *DotNetConsoleAppToolkit* project repository is **ARCHIVED** because it has been integrated in the **orbital-shell** project repository as the project , so it is still under development but code has been moved to this repository
![dot-net-console-app-toolkit](https://img.shields.io/badge/dotnet--console--app--toolkit-repository-lightgrey?style=plastic)
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)

&nbsp; | &nbsp;
-- | --
<img src="docs/assets/img/git-doc/tbrain.png"/> | this project might be conidered like a toy for old developper
