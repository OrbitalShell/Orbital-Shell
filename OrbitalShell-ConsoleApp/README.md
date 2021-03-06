# Orbital Shell - ConsoleApp
<b>Orbital Shell ConsoleApp library</b> (previously "Dot Net Console App Toolkit") helps build fastly nice multi-plateform (windows, osx, arm) console applications using C# and .NET Core 3.1 and .NET Standard 2.1

[![licence mit](https://img.shields.io/badge/licence-MIT-blue.svg)](license.md) This project is licensed under the terms of the MIT license: [LICENSE.md](LICENSE.md)  

![last commit](https://img.shields.io/github/last-commit/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)
![version](https://img.shields.io/github/v/tag/franck-gaspoz/dotnet-console-app-toolkit?style=plastic)

# Features

The toolkit provides functionalities needed to build console applications running in a terminal (WSL/WSL2, cmd.exe, ConEmu, bash, ...) with text interface. That includes:
- <b>a text printer engine </b>that supports <b>print directives</b> allowing to manage console functionalities from text itself, as html would do but with a simplest grammar (that can be configured). That makes possible colored outputs, cursor control, text scrolling and also dynamic C# execution (scripting), based on several APIs like <b>System.Console</b> and <b> ANSI VT100 / VT52</b>. The print directives are available as commands strings, as a command from the integrated shell or from an underlying shell

    ``` csharp
    // -- example of a string containing print directives --
    
    // 1) from the toolkit shell command line:
    echo "(br,f=yellow,b=red)yellow text on red background(br)(f=cyan)current time is: (exec=System.DateTime.Now,br)"
    
    // 2) invoking the toolkit:
    orbsh.exe "(br,f=yellow,b=red)yellow text on red background(br)(f=cyan)current time is: (exec=System.DateTime.Now,br)"
    
    // 3) from C# using DotNetConsole.cs
    Echo($"{Br}{Yellow}{BRed}yellow text on red background{Br}{Cyan}current time is: {System.DateTime.Now}{Br}");
     ```
     <image src="Doc/Images/2020-06-13 06_18_08-Window.png"/>
    
- <b>UI controls</b> for displaying texts and graphical characters in a various way and handling user inputs

- <b>libraries of methods</b> for performing various print operations, data conversions, process management, ..

- <b>data types</b> related to the system and the environment that provides usefull methods
