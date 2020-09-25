# Orbital Shell - Developer guide
**version 1.0 10/9/2020**  
*franck.gaspoz@gmail.com*

[![licence mit](https://img.shields.io/badge/licence-MIT-blue.svg)](license.md) This project is licensed under the terms of the MIT license: [LICENSE.md](LICENSE.md)

![version](https://img.shields.io/badge/Version-1.0.beta-green)

<hr>

## Index
- [**Repositories, projects and namespaces**](#repos)

  - [dotnet-console-app-toolkit](#dotnet-console-app-toolkit)
  - [orbital-shell](#orbital-shell)
    - [DotNetCconsoleAppToolkit-Shell](#dotnet-console-app-toolkit-shell)
    - [OrbitalShell](#orbitalshell)
    - [OrbitalShell-Android](#orbital-shell-android)
    
- [**Set up a developement environment**](#setupdevenv)
- [**Software architecture**](#archi)
- [**Roadmap**](#roadmap)
</ol>
</b>

<hr>

<a id="repos"/>

## 1. Repositories, projects and namespaces

<a id="dotnet-console-app-toolkit"/>

#### repository: dotnet-console-app-toolkit
#### project: DotNetConsoleAppToolkit

library that provides support for developing console applications (ANSI support)  
<b>C# 8 + .NET Standard 2.1</b>  
 
[![git repository](https://img.shields.io/badge/GIT-repository-green)](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git) [https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git) 

![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)  
![code size](https://img.shields.io/github/languages/code-size/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![repo size](https://img.shields.io/github/repo-size/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![lines of code](https://img.shields.io/tokei/lines/github/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)  
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)  

*namespaces*:  <b>DotNetConsoleAppToolkit</b>
<ul style="line-height:100%;margin:0px">
<li>    Console  
<li>    Component  
<ul style="line-height:100%;margin:0px">
<li>       UI  
</ul>
<li>   Lib  
</ul>

<hr>

<a id="orbital-shell"/>

#### repository: orbital-shell  

[![git repository](https://img.shields.io/badge/GIT-repository-green)](https://github.com/franck-gaspoz/dotnet-console-app-toolkit.git) [https://github.com/franck-gaspoz/orbital-shell.git](https://github.com/franck-gaspoz/orbital-shell.git)

![version](https://img.shields.io/github/v/tag/franck-gaspoz/orbital-shell?style=plastic)  
![code size](https://img.shields.io/github/languages/code-size/franck-gaspoz/orbital-shell?style=plastic)
![repo size](https://img.shields.io/github/repo-size/franck-gaspoz/orbital-shell?style=plastic)
![lines of code](https://img.shields.io/tokei/lines/github/franck-gaspoz/orbital-shell?style=plastic)  
![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/orbital-shell?style=plastic) 

<a id="dotnet-console-app-toolkit-shell"/>

#### project: DotNetConsoleAppToolkit-Shell

**project url**: [https://github.com/franck-gaspoz/orbital-shell/DotNetConsoleAppToolkit-Shell](https://github.com/franck-gaspoz/orbital-shell/tree/master/DotNetConsoleAppToolkit-Shell) 

library extending DotNetConsoleAppToolkit that provides features for building a command shell  
<b>C# 8 + .NET Standard 2.1</b>  

*namespaces*: <b>DotNetConsoleAppToolkit</b>

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
<li>ConsoleCommands
<li>SystemCommands
<li>FileSystemCommands
<li>TextEditor
<li>TestCommands
</ul>
</ul>
</ul>

<hr>

<a id="orbitalshell"/>

#### project: OrbitalShell

**project url**: [https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell](https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell)  
  
the orbital shell build for .NET Core targeting Windows,Linux and OSX  
<b>C# 8 + NET Core 3.1</b>  

*namespace*: <b>OrbitalShell  </b>

<hr>

<a id="orbital-shell-android"/>

#### project: OrbitalShell-Android
**project url**: [https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell-Android](https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell-Android) 

the orbital shell build for Xamarin-Mono Android (APK)  
<b>C# 8 + <i>Mono ?</i></b>  
  
*namespace*: <b>OrbitalShell  </b>

<hr>

<a id="setupdevenv"/>

## 2. Set up a development environment

<a id="archi"/>

## 3. Software architecture
### 3.1. Packages dependencies

<center>  
<img alt="package dependencies" src="Assets/OrbitalShellArchitecture.png"><br>  
<i>packages dependencies</i>
</center>

### 3.2. Flow diagram

*coming soon...*

<a id="roadmap"/>

## 4. Roadmap

- tests sur **OrbitalShell-Android**
- remanier et pousser projets dans GIT
- gestion code de retour, fixer comportement && et ||
- activation substitution des alias
- ligne de commande: activer flux et redirections
- ligne de commande: parser groupe de commandes et flux/redirections sur groupe
- variables du shell et paramètres de commandes (namespace réservé de *env*)
- commande **set**
- syntaxe d'affectation en subsitution de **set**
