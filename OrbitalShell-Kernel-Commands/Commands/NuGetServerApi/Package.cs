using System;
using System.Linq;

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
        public PackageVersion[] Versions;

        public override string ToString()
        {
            string r = Environment.NewLine;
            string sep = "".PadLeft(30, '-') + r;
            return $"Id={Id}{r}{sep}Registration={Registration}{r}Version={Version}{r}Description={Description}"
                + $"{r}Summary={Summary}{r}Title={Title}{r}IconUrl={IconUrl}{r}LicenseUrl={LicenseUrl}{r}ProjectUrl={ProjectUrl}"
                + $"{r}TotalDownloads={TotalDownloads}{r}Verified={Verified}{r}Tags={string.Join(";", Tags)}"
                + $"{r}Authors={string.Join(";", Authors)}{r}PackagesTypes={string.Join(";", PackageTypes.Select(x => x.ToString()))}"
                + $"{r}Versions={Environment.NewLine+"  "+string.Join( Environment.NewLine + "  ", Versions.Select(x => x.ToString()))}{r}";
        }
    }
}
