# Orbital Shell
A new commands shell, conforming to posix standard, running on any platform supported by .NET Core. Built on <b>Dot Net Console App Toolkit</b>, a library that helps build fastly nice multi-plateforms (windows, linux, macos) console applications using C# and .NET Core 3.1 and .NET Standard 2.1

[![licence mit](https://img.shields.io/badge/licence-MIT-blue.svg)](license.md) This project is licensed under the terms of the MIT license: [LICENSE.md](LICENSE.md)

## Orbital Shell (orbsh)

This powerfull shell integrates the most usefull shell commands, and is intented to be extended either by coding new commands or downloading new commands modules within a commands modules repository.

<img src="Doc/orbital-shell.png"/>

<img src="Doc/2020-06-13 02_34_57-Window-github.png"/>

The full documentation about the toolkit is available [here](https://github.com/franck-gaspoz/dotnet-console-app-toolkit/blob/master/README.md)

## Project status

What is implemented:

* command line reader and parser
* command processor
* commands methods (attributes allowing to declare command syntaxes directly in C#)
* print directives (extended ansi and programmatic text output features)

What is being implemented:

* variables
* pipes

## Packages dependencies:

Microsoft.CodeAnalysis.CSharp.Scripting 3.7.0-1.final
