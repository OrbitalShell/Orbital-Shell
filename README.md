## Orbital Shell

<img src="docs/assets/img/robotazteque.png" align="left" style="float:left;margin-right:8px"/><b>Orbital Shell</b> is a multi-plateform (**windows, linux, macos, arm**) command shell (according to .Net Core supported platforms and APIs compatibilities), inspired by <b><i>bash</i></b> and **POSIX** recommendations.<br><br>It provides any usual bash shell feature (even if modernized) and 'user friendly' syntaxes allowing to access (get/set/call) C# objects.<br><br>Developed using **C# 8, .NET Core 3.1/Net 5 and .NET Standard 2.1**

<br><br>

![.NET](https://github.com/OrbitalShell/Orbital-Shell/workflows/.NET/badge.svg?branch=master)
![last commit](https://img.shields.io/github/last-commit/orbitalshell/Orbital-Shell?style=plastic)
![releasever](https://img.shields.io/github/v/release/orbitalshell/Orbital-Shell?style=plastic) 
![releasedate](https://img.shields.io/github/release-date/orbitalshell/Orbital-Shell?style=plastic) 
<br>
![openi](https://img.shields.io/github/issues/orbitalshell/Orbital-Shell)
![closei](https://img.shields.io/github/issues-closed/orbitalshell/Orbital-Shell)
![closei](https://img.shields.io/github/issues-pr/orbitalshell/Orbital-Shell)
![closei](https://img.shields.io/github/issues-pr-closed/orbitalshell/Orbital-Shell)
<br>
![toplanguage](https://img.shields.io/github/languages/top/orbitalshell/Orbital-Shell)
![lngcount](https://img.shields.io/github/languages/count/orbitalshell/Orbital-Shell)
<br>
![codesize](https://img.shields.io/github/languages/code-size/orbitalshell/Orbital-Shell)
![reposize](https://img.shields.io/github/repo-size/orbitalshell/Orbital-Shell)

This shell integrates the <u>most usefull shell commands</u>, and is intented to be extended by coding new commands or downloading new commands modules within a repository of modules. Of course it can be enterly customized by using the features integrated to the shell (<u>scripts, functions, commands, aliases, settings, parametrization,...</u>). Having a strong <u>ANSI/VT-100-220-300-500</u> support, it provides structured and colorized display of data and information (support of <u>ASCII, Unicode and 24 bits colors</u>).

<br>

## About the project

Find any information and documentation about this project on the project's Web Site @ [Orbital SHell Git-Pages](https://orbitalshell.github.io/Orbital-Shell/) 

 &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<img src="docs/assets/img/2021-02-12 03_47_28-Window.png"/>

Developers and users manuals are available in the project web site @ [Orbital SHell Git-Pages (documentation)](https://orbitalshell.github.io/Orbital-Shell/documentation) 

> [![licence mit](https://img.shields.io/badge/licence-MIT-blue.svg)](license) This project is licensed under the terms of the MIT license: [LICENSE](LICENSE)  

<br>

## Contribute !

😄 We are looking for **collaborators** 👯 to help for evolving this tool ! If you **like** the **command line**, the **bash** syntax, programming **cli tools**, **parsers**, **data streams**, ANSI, C# 8, batch, and so on... you may like to develop orbital shell! Any idea, suggest, code, feedback is welcomed ! 

You can directly **FORK** the project @ [https://github.com/OrbitalShell/Orbital-Shell.git](https://github.com/OrbitalShell/Orbital-Shell.git) and start developing to prepare your first pull request, it will be well received. You can also peek **an issue** and start working on. 

Join the **orbital shell team** and get advantages of the team tools on github. We can contact 💬 by mail, skype and so on...

  - read the [contribution guide lines](https://github.com/OrbitalShell/Orbital-Shell/blob/master/CONTRIBUTING.md) and the [code of conduct of the project](https://github.com/OrbitalShell/Orbital-Shell/blob/master/CODE_OF_CONDUCT.md)
  - read the project README @ [https://github.com/OrbitalShell/Orbital-Shell/blob/master/README.md](https://github.com/OrbitalShell/Orbital-Shell/blob/master/README.md)
  - visit the project's web site @ [https://orbitalshell.github.io/Orbital-Shell/](https://orbitalshell.github.io/Orbital-Shell/)
  - visit the project repository @ [https://github.com/OrbitalShell/Orbital-Shell/](https://github.com/OrbitalShell/Orbital-Shell/)
  
**Join the project**: 
- Send a mail to the project team @ [mailto://orbital-shell@outlook.fr](mailto://orbital-shell@outlook.fr) or to [me](mailto://franck.gaspoz@gmail.com)
- or **push to repository** you will receive an invite to join the project via github

**The project team like to meet people and wish to share pleasure of programming and technology enthusiasm, with fun and good spirit** 

<br>

## Features

<img src="docs/assets/img/git-doc/orbital-shell.png"/>*a view of the shell running in Windows Terminal*

<br>

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

<br>

## Orbital shell uses these libraries:

- Microsoft.CodeAnalysis.CSharp.Scripting
- NewtonSoft.Json

<br>

## notice / caveat

> The project orbital shell was dependent of the project **DotNetConsoleAppToolkit**. Now the library *DotNetConsoleAppToolkit* project repository is **ARCHIVED** since it has been integrated into the **orbital-shell** project repository as project **OrbitalShell-ConsoleApp**, so it is still under development but code has been moved to this repository
![dot-net-console-app-toolkit](https://img.shields.io/badge/dotnet--console--app--toolkit-repository-lightgrey?style=plastic)
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)

<img src="docs/assets/img/tra4brains.png"/> | :warning: this project might be considered as a toy for old developer<br><br><img src="docs/assets/img/pegi46small.png"/><br><br>
-- | --

*Warning: due to the average age of our main contributors, this project might contains **vintage** architecture,design and code. Nevertheless the source code is intensively based on young timers preferred patterns, C# 8 language usage (intellisense agrees), and it pretends to fit to good practices and architecture guidelines.*
