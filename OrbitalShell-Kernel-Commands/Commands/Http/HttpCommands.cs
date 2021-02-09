using System;
using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;
using System.Net.Http;

namespace OrbitalShell.Commands.Http
{
    [Commands("http commands")]
    [CommandsNamespace(CommandNamespace.net,CommandNamespace.http)]
    public class HttpCommands : ICommandsDeclaringType
    {
        [Command("get")]
        public CommandResult<HttpContentBody> Get(
            CommandEvaluationContext context,
            [Parameter("query string")] string queryString
            )
        {
            string @return = null;
            if (string.IsNullOrWhiteSpace(queryString))
            {
                context.Errorln("uri must not be empty");
                return new CommandResult<HttpContentBody>(HttpContentBody.EmptyHttpContentBody);
            }
            if (!queryString.ToLower().StartsWith("http://") && !queryString.ToLower().StartsWith("https://"))
                queryString = "http://" + queryString;

            using (var httpClient = new HttpClient())
            {
                using var request = new HttpRequestMessage(new HttpMethod("GET"), queryString);
                var tsk = httpClient.SendAsync(request);
                var result = tsk.Result;
                if (result.IsSuccessStatusCode)
                {
                    @return = result.Content.ReadAsStringAsync().Result;

                    context.Out.Echo(@return, true, true);
                }
                else
                    context.Errorln($"can't get response content: {result.ReasonPhrase}");
            }

            return new CommandResult<HttpContentBody>(new HttpContentBody(@return));
        }

    }
}
