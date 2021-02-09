using System;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using System.Net.Http;
using OrbitalShell.Component.Console;
using Newtonsoft.Json;

namespace OrbitalShell.Commands.NuGetServerApi
{
    [Commands("github http server api commands")]
    [CommandsNamespace(CommandNamespace.net, CommandNamespace.http, "github")]
    public class GitHubServerApiCommands : ICommandsDeclaringType
    {
        
    }
}
