---
layout: documents
title: Get, compile and run Orbital Shell
doctitle: Get, compile and run Orbital Shell
---
Using a shell, for example **PowerShell**:
* use **git** and dotnet to get a clone of the Orbital Shell repository in the folder **orbital-shell**
* use **"dotnet"** to compile the solution

<pre data-enlighter-language="shell" data-enlighter-theme="{{site.data.settings.enjs_shell_theme}}" data-enlighter-linenumbers="false">
cd ~
git clone https://github.com/OrbitalShell/orbital-shell.git
cd ./orbital-shell
dotnet build OrbitalShell.sln
cd ./OrbitalShell-CLI/bin/Debug/netcoreapp3.1
</pre>

You can then launch **Orbital Shell** from this location:

<pre data-enlighter-language="shell" data-enlighter-theme="{{site.data.settings.enjs_shell_theme}}" data-enlighter-linenumbers="false">
./orbsh.exe
</pre>

You should add **orbsh.exe** path to system PATH :

<pre data-enlighter-language="shell" data-enlighter-theme="rowhammer" data-enlighter-linenumbers="false">
~/orbital-shell/OrbitalShell-CLI/bin/Debug/netcoreapp3.1/
</pre>
