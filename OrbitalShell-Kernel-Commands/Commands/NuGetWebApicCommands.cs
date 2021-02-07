using System;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using System.Net.Http;

namespace OrbitalShell.Commands
{
    [Commands("http web apu nuget commands")]
    [CommandsNamespace(CommandNamespace.net,CommandNamespace.http,"nuget")]
    public class NuGetWebApiCommands : ICommandsDeclaringType
    {
        public const string queryUrl = "{@id}?q={QUERY}&skip={SKIP}&take={TAKE}&prerelease={PRERELEASE}&semVerLevel={SEMVERLEVEL}&packageType={PACKAGETYPE}";

        [Command("get")]
        public CommandResult<String> Get(
            CommandEvaluationContext context,
            [Parameter("query")] string query,
            [Option("s","skip","")] bool skip,
            [Option("t","take","")] bool take,
            [Option("r","pre-release","pre release")] bool preRelease,
            [Option("l","sem-ver-level","sem verion level")] bool semVerLevel,
            [Option("p","package-type","pre release")] bool packageType,
            [Option("u","query-url","nuget web api query service template url")] string url = queryUrl
            )
        {
            string @return = null;

            var queryString = url
                .Replace("{@id}", "")
                .Replace("{QUERY}", query)
                .Replace("{TAKE}", take+"")
                .Replace("{PRERELEASE}", preRelease+"")
                .Replace("{SEMVERLEVEL}", semVerLevel+"")
                .Replace("{PACKAGETYPE}", packageType+"");

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), queryString))
                {
                    var tsk = httpClient.SendAsync(request);
                    var result = tsk.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        @return = result.Content.ReadAsStringAsync().Result;

                        context.Out.Echoln(@return);
                    }
                    else
                        context.Out.Errorln($"can't get response content: {result.ReasonPhrase}");
                }
            }

            return new CommandResult<string>(@return);
        }

    }
}
