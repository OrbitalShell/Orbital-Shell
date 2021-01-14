---
layout: documents
title: Repository and Projects
doctitle: Repository and Projects
---
<h3>Contents of the Orbital Shell project repository</h3>
Use **PowerShell** and **git** to get a clone of the Orbital Shell repository in the folder **orbital-shell** :
<pre data-enlighter-language="shell" data-enlighter-theme="{{site.data.settings.enjs_shell_theme}}" data-enlighter-linenumbers="false">
cd ~
git clone https://github.com/franck-gaspoz/orbital-shell.git
cd ./orbital-shell
dir
</pre>
you get the following folders ands files:
<pre data-enlighter-language="shell" data-enlighter-theme="rowhammer" data-enlighter-linenumbers="false">
Mode                LastWriteTime         Length Name
----                -------------         ------ ----
d-----       04/01/2021     16:02                .vscode
d-----       05/01/2021     22:13                docs
d-----       09/01/2021     14:06                OrbitalShell-CLI
d-----       09/01/2021     14:06                OrbitalShell-ConsoleApp
d-----       09/01/2021     14:06                OrbitalShell-Kernel
-a----       20/05/2020     13:05           2581 .gitattributes
-a----       20/05/2020     13:05           6084 .gitignore
-a----       13/06/2020     04:12           2542 LICENSE.md
-a----       09/01/2021     14:10           2329 OrbitalShell.sln
-a----       25/09/2020     21:54           5738 README.md
</pre>

Let's see what are these files and folders in detail :

|------+------+-------------| 
| name | type | description |
|------+------+-------------|
|**[docs](#docs){:.js-scroll-trigger}**|Jekyll web site|Orbital Shell project Git Pages|
|=================+============+=================+================|
|**[OrbitalShell-ConsoleApp](#apptlk){:.js-scroll-trigger}**|DotNet standard lib 2.1|.net console application toolkit|
|**[OrbitalShell-Kernel](#apptlkshell){:.js-scroll-trigger}**|DotNet standard lib 2.1|.net console application toolkit : shell extension |
|**[OrbitalShell-CLI](#orbsh){:.js-scroll-trigger}**|DotNet Core 5 console app|Orbital Shell CLI|
{:.table-rsp .table-light}

So we have 3 .NetCore projects folders, 1 documentation folder and 1 utilitary (mainly scripts) folder.
We also have two **solution** projects files:
* **.vscode** is used by **vscode** when opening the repository as a solution (<code>vscode .</code>)
* **OrbitalShell.sln** is used to open project files in a **Visual Studio** (2019) solution.

Let's watch to these folders in detail:
{:.marginbottom}

<h4 class="doc-subtitle" id="docs"><i class="fas fa-folder ico-yellow marginrighthalf"></i> docs<span class="text-tech">(web jekyll)</span></h4>

These is the **gh-pages** folder, containing the files for the **GitHub** pages of [Orbital Shell GitHub Project Pages](https://franck-gaspoz.github.io/orbital-shell/) (this web site){: target="_blank"}

This is the content of a **Jekyll** static web site generated - based on **Liquid**, **kramdown**, **Ruby**

Browse in repository: [https://github.com/franck-gaspoz/orbital-shell/tree/master/docs](https://github.com/franck-gaspoz/orbital-shell/tree/master/docs){: target="_blank"}

<h4 class="doc-subtitle" id="apptlk"><i class="fas fa-folder ico-yellow marginrighthalf"></i> OrbitalShell-ConsoleApp<span class="text-tech">(NetCore Standard 2.1)</span></h4>

Library that provides support for developing console applications (ANSI support). Named **DotNetConsoleAppToolkit** in the past (having it's own repository, now closed: [https://github.com/franck-gaspoz/dotnet-console-app-toolkit](https://github.com/franck-gaspoz/dotnet-console-app-toolkit)), this project has been integrated in the Orbital Shell repository, for convenience.

> *provided namespaces*:<br><br>
> * **OrbitalShell**
>   * Console  
>   * Component  
>     * UI  
>   * Lib
{:.block-text}

Browse in repository: [https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell-ConsoleApp](https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell-ConsoleApp){: target="_blank"}

<h4 class="doc-subtitle" id="apptlkshell"><i class="fas fa-folder ico-yellow marginrighthalf"></i> OrbitalShell-Kernel<span class="text-tech">(NetCore Standard 2.1)</span></h4>

Library extending **OrbitalShell-AppConsole** that provides features for building a command shell. __It is the Orbital Shell Kernel__. 

> *provided namespaces*:<br><br>
> * **OrbitalShell**
>   * Component
>     * CommandLine
>       * CommandBatch
>       * CommandLineReader
>       * CommandModel
>       * Data
>       * Defaults
>       * Parsing
>       * Pipeline
>       * Processor
>       * Variable
>   * Console
>   * Lib
>     * Data
>     * FileSystem
>   * Shell
>     * Commands
>       * CommandLineProcessorCommands
>       * ConsoleCommands
>       * SystemCommands
>       * FileSystemCommands
>       * TextEditor
>       * TestCommands
{:.block-text}

Browse in repository: [https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell-Kernel](https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell-Kernel){: target="_blank"}

<h4 class="doc-subtitle" id="orbsh"><i class="fas fa-folder ico-yellow marginrighthalf"></i> OrbitalShell-CLI<span class="text-tech">(.Net Core 3.1)</span></h4>

This console project provides the Orbital Shell executable, but is also a build of code,files and script thats build the distribution of Orbital Shell.

> *provided namespaces*:<br><br>
> * **OrbitalShell**
>   * Component
>     * CommandLine
>       * Defaults
>       * Processor
{:.block-text}

Browse in repository: [https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell-CLI](https://github.com/franck-gaspoz/orbital-shell/tree/master/OrbitalShell-CLI){: target="_blank"}
