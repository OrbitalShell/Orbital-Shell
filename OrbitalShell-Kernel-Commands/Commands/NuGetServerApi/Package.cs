using System;
using System.Linq;

using OrbitalShell.Component.Console;
using OrbitalShell.Component.Console.Formats;
using OrbitalShell.Component.Shell.Data;

namespace OrbitalShell.Commands.NuGetServerApi
{
    public class Package
    {
        public string Id;
        public string Registration;
        public string Version;
        public string Description;
        public string Summary;
        public string Title;
        public string IconUrl;
        public string LicenseUrl;
        public string ProjectUrl;
        public string[] Tags;
        public string[] Authors;
        public int TotalDownloads;
        public bool Verified;
        public PackageType[] PackageTypes;
        public PackageVersionInfos[] Versions;

        public override string ToString()
        {
            string r = Environment.NewLine;
            string sep = "".PadLeft(30, '-') + r;
            return $"Id={Id}{r}{sep}Registration={Registration}{r}Version={Version}{r}Description={Description}"
                + $"{r}Summary={Summary}{r}Title={Title}{r}IconUrl={IconUrl}{r}LicenseUrl={LicenseUrl}{r}ProjectUrl={ProjectUrl}"
                + $"{r}TotalDownloads={TotalDownloads}{r}Verified={Verified}{r}Tags={string.Join(";", Tags)}"
                + $"{r}Authors={string.Join(";", Authors)}{r}PackagesTypes={string.Join(";", PackageTypes.Select(x => x.ToString()))}"
                + $"{r}Versions={Environment.NewLine + "  " + string.Join(Environment.NewLine + "  ", Versions.Select(x => x.ToString()))}{r}";
        }

        /// <summary>
        /// Echo method
        /// </summary>
        /// <param name="context">echo context</param>
        public void Echo(EchoEvaluationContext context)
        {
            var options = new TableFormattingOptions(context.CommandEvaluationContext.ShellEnv.TableFormattingOptions)
            {
                UnfoldCategories = false,
                UnfoldItems = false,
                IsRawModeEnabled = true
            };

            var cols = context.CommandEvaluationContext.ShellEnv.Colors;
            var tb = new Table(
                ("property", typeof(string), $"{cols.Label}{{0}}{cols.Default}")
            );

            tb.AddColumns(("value", typeof(object)));

            tb.AddRow("id", Id);
            tb.AddRow("registration", Registration);
            tb.AddRow("version", Version);
            tb.AddRow("description", Description);
            tb.AddRow("summary", Summary);
            tb.AddRow("title", Title);
            tb.AddRow("icon url", IconUrl);
            tb.AddRow("license url", LicenseUrl);
            tb.AddRow("project url", ProjectUrl);
            tb.AddRow("tags", Tags);
            tb.AddRow("authors", Authors);
            tb.AddRow("total downloads", TotalDownloads);
            tb.AddRow("verified", Verified);
            tb.AddRow("packages types", PackageTypes);
            tb.AddRow("versions", Versions);
            tb.Echo(new EchoEvaluationContext(context, options));
        }
    }
}
