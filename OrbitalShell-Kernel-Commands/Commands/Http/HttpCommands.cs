using System.Net.Http;

using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell;

namespace OrbitalShell.Commands.Http
{
    [Commands("http commands")]
    [CommandsNamespace(CommandNamespace.net, CommandNamespace.http)]
    public class HttpCommands : ICommandsDeclaringType
    {
        [Command("query url (http)")]
        public CommandResult<HttpContentBody> Qurl(
            CommandEvaluationContext context,
            [Parameter("query string")] string queryString,
            [Option("q", "quiet", "if not set (default), output get result on stdout", false, true)] bool quiet = false,
            [Option("b", "bin", "get as a binary stream", false, true)] bool binary = false
            )
        {
            object @return = null;
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
                    //@return = result.Content.ReadAsStringAsync().Result;
                    @return =
                        !binary ?
                            result.Content.ReadAsStringAsync().Result :
                            result.Content.ReadAsByteArrayAsync().Result;

                    if (!quiet) context.Out.Echo(@return, true, true);
                }
                else
                    context.Errorln($"can't get response content: {result.ReasonPhrase}");
            }

            return new CommandResult<HttpContentBody>(new HttpContentBody(@return));
        }

    }
}
