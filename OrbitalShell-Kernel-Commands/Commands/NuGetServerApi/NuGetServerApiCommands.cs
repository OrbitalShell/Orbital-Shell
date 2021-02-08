using System;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using System.Net.Http;
using OrbitalShell.Component.Console;
using Newtonsoft.Json;

namespace OrbitalShell.Commands.NuGetServerApi
{
    [Commands("http web apu nuget commands")]
    [CommandsNamespace(CommandNamespace.net,CommandNamespace.http,"nuget")]
    public class NuGetServerApiCommands : ICommandsDeclaringType
    {
        /// <summary>
        /// type: SearchQueryService/3.0.0-rc<br/>
        /// Query endpoint of NuGet Search service (primary) used by RC clients
        /// </summary>
        public const string queryUrl = "https://api-v2v3search-0.nuget.org/query?q={QUERY}&skip={SKIP}&take={TAKE}&prerelease={PRERELEASE}&semVerLevel={SEMVERLEVEL}&packageType={PACKAGETYPE}";

        [Command("call nuget web query service and output results")]
        public CommandResult<QueryResultRoot> NugetQuery(
            CommandEvaluationContext context,
            [Parameter("the search terms to used to filter packages",true)] string query,
            [Option("s","skip", "the number of results to skip, for pagination")] int skip=-1,
            [Option("t","take", "the number of results to return, for pagination")] int take=-1,
            [Option("r","pre-release", "true or false determining whether to include pre-release packages (default no)")] bool preRelease = false,
            [Option("l","sem-ver-level", "a SemVer 1.0.0 version string")] string semVerLevel = "2.0.0",
            [Option("p","package-type", "the package type to use to filter packages (added in SearchQueryService/3.5.0)")] string packageType = null,
            [Option("u","query-url","nuget web api query service template url")] string url = queryUrl
            )
        {
            QueryResultRoot @return = null;

            query ??= "";
            var queryString = url.Replace("{QUERY}", query.Trim());

            if (skip > -1) queryString = queryString.Replace("{SKIP}", skip + "");
            if (take > -1) queryString = queryString.Replace("{TAKE}", take + "");
            queryString = queryString.Replace("{PRERELEASE}", preRelease.ToString().ToLower());
            if (semVerLevel != null) queryString = queryString.Replace("{SEMVERLEVEL}", semVerLevel);
            if (packageType != null) queryString = queryString.Replace("{PACKAGETYPE}", packageType);
            queryString = queryString
                .Replace("&skip={SKIP}", "")
                .Replace("&take={TAKE}", "")
                .Replace("&prerelease={PRERELEASE}", "")
                .Replace("&semVerLevel={SEMVERLEVEL}", "")
                .Replace("&packageType={PACKAGETYPE}", "");

            context.Out.Echo(context.ShellEnv.Colors.Log + $"GET {queryString} ...");

            using (var httpClient = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"), queryString))
                {
                    var tsk = httpClient.SendAsync(request);
                    var result = tsk.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var res = result.Content.ReadAsStringAsync().Result;

                        context.Out.Echoln(" Done(rdc)");
                        context.Out.Echo(ANSI.RSTXTA + ANSI.CPL(1) + ANSI.EL(ANSI.ELParameter.p2));     // TODO: add as ANSI combo
                        
                        var obj = JsonConvert.DeserializeObject<QueryResultRoot>(res);
                        @return = obj;

                        if (obj != null && obj.Data != null)                        
                            context.Out.Echoln(obj.ToString());                        
                        else 
                            context.Errorln("invalid json");                                                
                    }
                    else
                        context.Errorln($"can't get response content: {result.ReasonPhrase}");
                }
            }

            return new CommandResult<QueryResultRoot>(@return);
        }

    }
}
