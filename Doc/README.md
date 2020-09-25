<hr>

# Orbital Shell - Developer guide
**version 1.0 10/9/2020**  
*franck.gaspoz@gmail.com*

[![licence mit](https://img.shields.io/badge/licence-MIT-blue.svg)](license.md) This project is licensed under the terms of the MIT license: [LICENSE.md](LICENSE.md)

![version](https://img.shields.io/badge/Version-1.0.beta-green)

<hr>

## Index
<b><ol style="line-height:100%">
<li>Repositories, projects and namespaces
<ul style="line-height:100%">
<li>dotnet-console-app-toolkit
<li>dotnet-console-app-toolkit-shell
<li>orbital-shell
<li>orbital-shell-android
</ul>
<li>Software architecture
<li>Setup a developement environment
<li>Roadmap
</ol>
</b>

<hr>

## 1. Repositories, projects and namespaces

#### dotnet-console-app-toolkit (DotNetConsoleAppToolkit)

library that provides support for developing console applications (ANSI support)  
<b>C# 8 + .NET Standard 2.1</b>  
 
[![git repository](https://img.shields.io/badge/GIT-repository-green)](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git) [https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git) 

![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit)  
![code size](https://img.shields.io/github/languages/code-size/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![repo size](https://img.shields.io/github/repo-size/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![lines of code](https://img.shields.io/tokei/lines/github/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)  
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)  

*namespaces*:  
<b>DotNetConsoleAppToolkit</b>
<ul style="line-height:100%;margin:0px">
<li>    Console  
<li>    Component  
<ul style="line-height:100%;margin:0px">
<li>       UI  
</ul>
<li>   Lib  
</ul>

<hr>

#### dotnet-console-app-toolkit-shell (DotNetConsoleAppToolkit-Shell)

library that provides features of a command shell having the full ***bash*** (*Bourne Again shell*) features (POSIX)  
<b>C# 8 + .NET Standard 2.1</b>  
  
[![git repository](https://img.shields.io/badge/GIT-repository-green)](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git)
[https://github.com/franck-gaspoz/dotnet-console-app-toolkit-shell.git](https://github.com/franck-gaspoz/dotnet-console-app-toolkit-shell.git)
  
![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit-shell)  
![code size](https://img.shields.io/github/languages/code-size/franck-gaspoz/dotnet-console-app-toolkit-shell?style=plastic)
![repo size](https://img.shields.io/github/repo-size/franck-gaspoz/dotnet-console-app-toolkit-shell?style=plastic)
![lines of code](https://img.shields.io/tokei/lines/github/franck-gaspoz/dotnet-console-app-toolkit-shell?style=plastic)   
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit-shell?style=plastic) 

*namespaces*:   
<b>DotNetConsoleAppToolkit</b>
<ul style="line-height:100%">
<li>    Component 
<ul style="line-height:100%;margin:0px">
<li>        CommandLine  
<ul style="line-height:100%;margin:0px">
<li>        CommandBatch
<li>        CommandLineReader  
<li>        CommandModel  
<li>        Data  
<li>        Defaults  
<li>        Parsing  
<li>        Pipeline  
<li>        Processor  
<li>        Variable 
</ul>
</ul>
<li>    Console  
<li>    Lib  
<ul style="line-height:100%;margin:0px">
<li>        Data  
<li>        FileSystem  
</ul>
<li>    Shell  
<ul style="line-height:100%;margin:0px">
<li>        Commands  
<ul style="line-height:100%;margin:0px">
<li>CommandLineProcessorCommands
<li>CommandLineProcessorCommands
<li>ConsoleCommands
<li>SystemCommands
<li>FileSystemCommands
<li>TextEditor
<li>TestCommands
</ul>
</ul>
</ul>

<hr>

#### orbital-shell (OrbitalShell)

the orbital shell build for .NET Core targeting Windows,Linux and OSX  
<b>C# 8 + NET Core 3.1</b>  
  
[![git repository](https://img.shields.io/badge/GIT-repository-green)](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git) [https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git)

![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit)  
![code size](https://img.shields.io/github/languages/code-size/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![repo size](https://img.shields.io/github/repo-size/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![lines of code](https://img.shields.io/tokei/lines/github/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)  
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit?style=plastic) 

*namespace*:  
<b>OrbitalShell  </b>

<hr>

#### orbital-shell-android (OrbitalShell-Android)

the orbital shell build for Xamarin-Mono Android (APK)  
<b>C# 8 + <i>Mono ?</i></b>  
  
[![git repository](https://img.shields.io/badge/GIT-repository-green)](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git) [https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git)  

![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit)  
![code size](https://img.shields.io/github/languages/code-size/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![repo size](https://img.shields.io/github/repo-size/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![lines of code](https://img.shields.io/tokei/lines/github/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)   
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit?style=plastic) 

*namespace*:  
<b>OrbitalShell  </b>

<hr>

## 2. Software architecture
### 2.1. Packages dependencies

<center>  
<img alt="package dependencies" src="OrbitalShellArchitecture.png"><br>  
<i>packages dependencies</i>
</center>

### 2.2. Synoptic

## 3. Roadmap

- tests sur **OrbitalShell-Android**
- remanier et pousser projets dans GIT
- gestion code de retour, fixer comportement && et ||
- activation substitution des alias
- ligne de commande: activer flux et redirections
- ligne de commande: parser groupe de commandes et flux/redirections sur groupe
- variables du shell et paramètres de commandes (namespace réservé de *env*)
- commande **set**
- syntaxe d'affectation en subsitution de **set**
