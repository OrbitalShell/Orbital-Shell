using System;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using System.Net.Http;

namespace OrbitalShell.Commands
{
    [Commands("http commands")]
    [CommandsNamespace(CommandNamespace.net,CommandNamespace.http)]
    public class HttpCommands : ICommandsDeclaringType
    {
        [Command("get")]
        public CommandResult<String> Get(
            CommandEvaluationContext context,
            [Parameter("query string")] string queryString
            )
        {
            string @return = null;

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
