using System;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using System.Net.Http;
using OrbitalShell.Component.Console;
using Newtonsoft.Json;
using OrbitalShell.Lib.FileSystem;
using System.IO;
using System.Net.Http.Headers;
using System.Net;

namespace OrbitalShell.Module.NuGetServerApi
{
    [Commands("github http server api commands")]
    [CommandsNamespace(CommandNamespace.net, CommandNamespace.http, "github")]
    public class GitHubRestApiCommands : ICommandsDeclaringType
    {
        public const string GetUserUrl = "https://api.github.com/users/{ID}";

        [Command("get infos about a github user")]
        public CommandResult<GitHubUser> GithubGetUser(
            CommandEvaluationContext context,
            [Parameter(0, "github user ID")] string id,
            [Option("u", "get-user-url", "github server api get user service template url", true, true)] string url = GetUserUrl
        ) {
            GitHubUser @return = null;
            var queryString = url.Replace("{ID}", id.Trim());

            context.Out.Echo(context.ShellEnv.Colors.Log + $"GET {queryString} ... ");

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), queryString);

            var tsk = httpClient.SendAsync(request);
            var result = tsk.Result;
            if (result.IsSuccessStatusCode)
            {
                var res = result.Content.ReadAsStringAsync().Result;

                context.Out.Echoln(" Done(rdc)");
                context.Out.Echo(ANSI.RSTXTA + ANSI.CPL(1) + ANSI.EL(ANSI.ELParameter.p2));     // TODO: add as ANSI combo

                var obj = JsonConvert.DeserializeObject<GitHubUser>(res);
                @return = obj;

                if (obj != null)
                    obj.Echo(new EchoEvaluationContext(context.Out, context));
                else
                    context.Errorln("invalid json");
            }
            else
                context.Errorln($"can't get response content: {result.ReasonPhrase}");

            return new CommandResult<GitHubUser>(@return);
        }
    }
}
