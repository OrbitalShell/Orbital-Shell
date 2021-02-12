# Orbital Shell

<b>Orbital Shell</b> is a multi-plateform (**windows, linux, macos**) command shell, inspired by <b><i>bash</i></b> and POSIX recommendations. It provides any usual bash shell feature (even if modernized) and nice syntaxes and features allowing to access (get/set/call) C# objects. Developed using **C# 8, .NET Core 3.1/Net 5 and .NET Standard 2.1**

This shell integrates the <u>most usefull shell commands</u>, and is intented to be extended by coding new commands or downloading new commands modules within a repository of modules. Of course it can be enterly customized by using the features integrated to the shell (<u>scripts, functions, aliases, settings, parametrization,...</u>). Having a strong <u>ANSI/VT-100-220-300-500</u> support, it provides structured and colorized display of data and information (support of <u>ASCII, Unicode and 24 bits colors</u>).

## About the project

> Find anything about this project on the project's Web Site at: [Orbital SHell Git-Pages](https://franck-gaspoz.github.io/orbital-shell) 

> The full documentation of the **Orbital Shell**, for users and developers, is available here: [Orbital SHell Git-Pages (documentation)](https://franck-gaspoz.github.io/orbital-shell/documentation) 


[![licence mit](https://img.shields.io/badge/licence-MIT-blue.svg)](license) This project is licensed under the terms of the MIT license: [LICENSE](LICENSE)  

<u>project repositories status:</u>

![orbital-shell](https://img.shields.io/badge/orbital--shell-repository-lightgrey?style=plastic)
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/orbital-shell?style=plastic)
![version](https://img.shields.io/github/v/tag/franck-gaspoz/orbital-shell?style=plastic) ![version](https://img.shields.io/badge/Version-1.0.beta-green)

## Features

<img src="docs/assets/img/git-doc/orbital-shell.png"/>
<center><small><i>a view of the shell running in Windows Terminal</i></small></center>


- Is a '**bash** style' command shell
- The existing **modules** of commands contains **the most classical shell commands** (ls/dir,rm,mv,cp,find,echo,clear,more,env,set,export,history,alias,...), tests commands, and a **text editor command**
  - dedicated repositories are used to get/update modules of commands
- The shell command engine implements
  - A **command line reader** with usual input keyboard shortcuts, auto completion and input feedback (colorization)
  - **Streams** standard input (*stdIn*), standard output (*stdOut*), and standard error (*StdErr*)
  - **Streams redirections** ( **<** **>** )
  - commands **pipelines**:
    - sequences of commands ( **|** ) and conditional sequences of commands ( **&&**, **||** )
    - groups of commands **(** .. **;** .. **)**
    - background execution ( **&** )
  - **Batches of commands** (scripts)   
  - **Cancellation** of commands execution (Ctrl-Z)
- **Variables**
  - Variables can store any data of **any C# object type**
    - The command line syntax has been extended to **support an object notation to indicate accessing an object member or invoking an object method**
    - commands returns a **result code** and can returns **values**. When returning values and when specified, a pipe ( **|** ) between commands can **pass objects instead of streams**
  - The **environment variables** are of type **String**
  - Are getted and setted with **$**, **set** and **=** 
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
    public CommandResult<List<string>> Rm(
        [Parameter("file or folder path")] WildcardFilePath path,
        [Option("r", "also remove files and folders in sub directories")] bool recurse,
        [Option("i","prompt before any removal")] bool interactive,
        [Option("v", "explain what is being done")] bool verbose,
        [Option("d", "remove empty directories")] bool rmEmptyDirs,
        [Option("na", "do not print file system attributes when verbose")] bool noattributes,
        [Option("s", "don't remove any file/or folder, just simulate the operation (enable verbose)")] bool simulate
    )
    {
        var r = new List<string>();
        if (path.CheckExists())
        {
            /* ... */
            context.Out.Echoln("this is the (uon,f=red)'rm'(uoff,rdc) command!");
            /* ... */
        }
        return new CommandResult<List<string>>(r,ReturnCode.OK);
    }
    ```
  - that will be auto documented like this by the shell help feature:
  
    <img src="docs/assets/img/git-doc/2020-06-13%2006_36_43-Window.png"/>

*notice:*

> The project orbital shell was dependent of the project **DotNetConsoleAppToolkit**. Now the library *DotNetConsoleAppToolkit* project repository is **ARCHIVED** because it has been integrated in the **orbital-shell** project repository, so it is still under development but code has been moved to this repository

> ![dot-net-console-app-toolkit](https://img.shields.io/badge/dotnet--console--app--toolkit-repository-lightgrey?style=plastic)
> ![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
> ![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)   

## Packages dependencies:

Microsoft.CodeAnalysis.CSharp.Scripting 3.7.0-1.final
