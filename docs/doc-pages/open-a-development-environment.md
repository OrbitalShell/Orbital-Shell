---
layout: documents
title: Open a development environment
doctitle: Open a development environment
---
After you get the project source code from GitHub (see [Get, compile and run Orbital Shell]({{site.baseurl}}doc-pages/get-compile-run.html#get-compile-run)), it's time to open the project files in an IDE. Here we explain how to use the best IDE choice for C# .NETCore development: **vscode** and **visual studio**. The project can be compiled using either **dotnet** or **msbuild**, any required configuration is provided in the project for that. In theses examples we assume that the project repository is the folder <code>~/orbital-shell</code>

> check the documentation page [Tools]({{site.baseurl}}doc-pages/tools.html#tools) to get information about tools urls and install
{:.block-text}

<h3>using <b>Visual Studio Code</b></h3>

**open the ide:**

<pre data-enlighter-language="shell" data-enlighter-theme="{{site.data.settings.enjs_shell_theme}}" data-enlighter-linenumbers="false">
cd ~/orbital-shell
code .
</pre>

Click menu <code>Run > Start Debugging</code>:

<figure><a href="{{site.baseurl}}assets/img/project-vscode.png" target="_blank"><img src="{{site.baseurl}}assets/img/project-vscode.png" width="100%"></a><figcaption>Orbital Shell project is opened and running in vscode</figcaption></figure>

<h3>using <b>Visual Studio Community 2019</b></h3>

"..."
