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

namespace OrbitalShell.Commands.NuGetServerApi
{
    [Commands("nuget http server api commands")]
    [CommandsNamespace(CommandNamespace.net,CommandNamespace.http,"nuget")]
    public class NuGetServerApiCommands : ICommandsDeclaringType
    {
        /// <summary>
        /// type: SearchQueryService/3.0.0-rc<br/>
        /// Query endpoint of NuGet Search service (primary) used by RC clients
        /// </summary>
        public const string QueryUrl = "https://api-v2v3search-0.nuget.org/query?q={QUERY}&skip={SKIP}&take={TAKE}&prerelease={PRERELEASE}&semVerLevel={SEMVERLEVEL}&packageType={PACKAGETYPE}";
        public const string PushUrl = "https://www.nuget.org/api/v2/package";
        public const string DownloadUrl = "https://api.nuget.org/v3-flatcontainer/{LOWER_ID}/{LOWER_VERSION}/{LOWER_ID}.{LOWER_VERSION}.nupkg";
        public const string GetVerUrl = "https://api.nuget.org/v3-flatcontainer/{ID}/index.json";
        public const string ProtocolVersion = "4.1.0";

        [Command("get versions of a nuget package")]
        public CommandResult<PackageVersions> NugetVer(
            CommandEvaluationContext context,
            [Parameter(0, "package (.nuget) ID")] string id,
            [Option("u", "get-url", "nuget server api query service template url", true, true)] string url = GetVerUrl
            )
        {
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

                if (res != null && !string.IsNullOrWhiteSpace(res))
                {
                    context.Out.Echoln(res);

                    var obj = JsonConvert.DeserializeObject<PackageVersions>(res);

                    return new CommandResult<PackageVersions>(obj);
                }
                else
                    context.Warning("result is empty");
            }
            else
                context.Errorln($"can't get response content: {result.ReasonPhrase}");

            return new CommandResult<PackageVersions>(null);
        }

        [Command("download a nuget package")]
        public CommandResult<string> NugetDownload(
            CommandEvaluationContext context,
            [Parameter(0, "package (.nuget) ID")] string id,
            [Parameter(1, "package version")] string ver,
            [Option("o","output","output path",true,true)] string @out = ".",
            [Option("u", "download-url", "nuget server api query service template url", true, true)] string url = DownloadUrl
            )
        {
            id = id.Trim().ToLower();
            ver = ver.ToLower();
            var queryString = url
                .Replace("{LOWER_ID}", id)
                .Replace("{LOWER_VERSION}", ver);

            context.Out.Echo(context.ShellEnv.Colors.Log + $"GET {queryString} ... ");

            using var httpClient = new HttpClient();
            using var request = new HttpRequestMessage(new HttpMethod("GET"), queryString);

            var tsk = httpClient.SendAsync(request);
            var result = tsk.Result;
            if (result.IsSuccessStatusCode)
            {
                var str = result.Content.ReadAsStreamAsync().Result;

                context.Out.Echoln(" Done(rdc)");
                context.Out.Echo(ANSI.RSTXTA + ANSI.CPL(1) + ANSI.EL(ANSI.ELParameter.p2));     // TODO: add as ANSI combo

                if (str == null)
                    context.Warning("result is empty");
                else
                {
                    var fn = $"{id}.{ver}.nupkg";
                    @out = Path.Combine(@out, fn);
                    using var fstr = new FileStream(@out, FileMode.Create, FileAccess.Write);
                    int b;
                    while ((b = str.ReadByte()) != -1)
                        fstr.WriteByte((byte)b);
                    str.Close();
                    fstr.Close();
                    context.Out.Echoln($"package '{fn}' has been downloaded to: {new FilePath(@out)}");

                    return new CommandResult<string>(@out);
                }
            }
            else
            {
                context.Errorln($"can't get response content: {result.ReasonPhrase}");
                return new CommandResult<string>(ReturnCode.Error);
            }

            return new CommandResult<string>(ReturnCode.Error);
        }

        [Command("push a nuget package")]
        public CommandResult<string> NugetPush(
            CommandEvaluationContext context,
            [Parameter(0,"package (.nuget) file path")] FilePath pkgFile,
            [Parameter(1,"target server api key")] string apiKey,
            [Option("u", "push-url", "nuget server api push service url", true, true)] string url = PushUrl,
            [Option("p", "protocol-version", "nuget thir party client protocol version", true, true)] string protocolVersion = ProtocolVersion
            )
        {
            var @return = "";
            if (pkgFile.CheckExists(context))
            {
                var ext = Path.GetExtension(pkgFile.FullName);
                var atExt = ".nupkg";
                if (ext.ToLower() != atExt )
                    context.Errorln($"bad file extension: '{ext}', should be '{atExt}'");
                else
                {
                    if (string.IsNullOrWhiteSpace(apiKey))
                        context.Errorln($"api key is required and can't be empty");
                    else
                    {
                        using var httpClient = new HttpClient();
                        using var request = new HttpRequestMessage(new HttpMethod("PUT"), url);

                        request.Headers.Add("X-NuGet-ApiKey", apiKey);
                        request.Headers.AcceptEncoding.ParseAdd("gzip,deflate");
                        request.Headers.Add("X-NuGet-Protocol-Version", protocolVersion);
                        //request.Headers.Add("X-NuGet-Client-Version", "5.8.1");
                        //request.Headers.Add("user-agent", "NuGet Command Line / 5.8.1 (Microsoft Windows NT 10.0.19042.0)");

                        var content = new MultipartFormDataContent();

                        var fileContent = new ByteArrayContent(File.ReadAllBytes(pkgFile.FullName));

                        fileContent.Headers.ContentDisposition =
                            new ContentDispositionHeaderValue(
                                "form-data"
                            )
                            {
                                FileName = "package.nupkg"
                            };

                        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                        content.Add(fileContent);
                        request.Content = content;


                        context.Out.Echo(context.ShellEnv.Colors.Log + $"PUT {PushUrl} ... ");

                        var tsk = httpClient.SendAsync(request);

                        var result = tsk.Result;
                        if (result.IsSuccessStatusCode)
                        {
                            var res = result.Content.ReadAsStringAsync().Result;

                            context.Out.Echoln(" Done(rdc)");
                            context.Out.Echo(ANSI.RSTXTA + ANSI.CPL(1) + ANSI.EL(ANSI.ELParameter.p2));     // TODO: add as ANSI combo

                            @return = res;

                            if (res != null)
                            { if (!string.IsNullOrWhiteSpace(res)) context.Out.Echoln(res); }
                            else
                                context.Warningln("quey result is empty");
                            context.Warningln("quey result is empty");

                            if (result.StatusCode == HttpStatusCode.Created
                                || result.StatusCode == HttpStatusCode.Accepted)
                                context.Out.Echoln($"package '{Path.GetFileName(pkgFile.FullName)}' has been successfully pushed");
                        }
                        else
                            context.Errorln($"can't get response content: {(int)result.StatusCode} {result.ReasonPhrase}");
                    }
                }
            }
            return new CommandResult<string>(@return);
        }

        [Command("call nuget web query service and output results")]
        public CommandResult<QueryResultRoot> NugetQuery(
            CommandEvaluationContext context,
            [Parameter("the search terms to used to filter packages",true)] string query,
            [Option("s","skip", "the number of results to skip, for pagination",true,true)] int skip=-1,
            [Option("t","take", "the number of results to return, for pagination", true, true)] int take=-1,
            [Option("r","pre-release", "true or false determining whether to include pre-release packages (default no)")] bool preRelease = false,
            [Option("l","sem-ver-level", "a SemVer 1.0.0 version string", true, true)] string semVerLevel = "2.0.0",
            [Option("p","package-type", "the package type to use to filter packages (added in SearchQueryService/3.5.0)",true,true)] string packageType = null,
            [Option("u","query-url","nuget server api query service template url", true, true)] string url = QueryUrl
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

                var obj = JsonConvert.DeserializeObject<QueryResultRoot>(res);
                @return = obj;

                if (obj != null && obj.Data != null)
                    obj.Echo(new EchoEvaluationContext(context.Out, context));
                else
                    context.Errorln("invalid json");
            }
            else
                context.Errorln($"can't get response content: {result.ReasonPhrase}");
            

            return new CommandResult<QueryResultRoot>(@return);
        }

    }
}
