using System;
using System.Collections.Generic;
using System.Text;

namespace OrbitalShell.Commands.Tools.Shell
{
    public class ModuleSettings
    {
        /// <summary>
        /// module id (C# case) - auto from command new-module
        /// </summary>
        public string ModuleID;

        /// <summary>
        /// OrbitalShell-Module-{ModulePackageID}
        /// </summary>
        public string ModulePackageID;

        /// <summary>
        /// ModulePascalCaseID ::= moduleId != ModuleId (auto)
        /// </summary>
        public string ModulePascalCaseID;

        /// <summary>
        /// final module id, lower case (OrbitalShell-Module-{ModulePackageID}) (auto)
        /// </summary>
        public string ModuleLowerID;

        /// <summary>
        /// final module namespace for readme (auto == env.com.{ModuleToolNamespace})
        /// </summary>
        public string ModuleNamespace;

        /// <summary>
        /// title for doc/nupkg
        /// </summary>
        public string ModuleTitle;

        /// <summary>
        /// description for doc/nupkg
        /// </summary>
        public string ModuleDescription;

        /// <summary>
        /// module min shell version (auto == OrbitalshellKernelVersion)
        /// </summary>
        public string ModuleShellMinVersion = "1.0.1-beta4";

        /// <summary>
        /// authors
        /// </summary>
        public string ModuleAuthors = "Orbital Shell team";

        /// <summary>
        /// copyright
        /// </summary>
        public string ModuleCopyright = "(c) Orbital Shell 2020";

        /// <summary>
        /// version == major.minor.build
        /// </summary>
        public string ModuleVersion = "1.0.0";

        /// <summary>
        /// product title
        /// </summary>
        public string ModuleProduct;

        /// <summary>
        /// company
        /// </summary>
        public string ModuleCompany = "Orbital Shell";

        /// <summary>
        /// release ntoes
        /// </summary>
        public string ModulePackageReleaseNotes;

        /// <summary>
        /// package web site url
        /// </summary>
        public string ModulePackageUrl = "https://orbitalshell.github.io/Orbital-Shell/";
        
        /// <summary>
        /// package tags
        /// </summary>
        public string ModulePackageTags = "prompt git git-branch git-status bash linux shell interactive csharp netcore5 netstandard21 netcore31 cli command-line-tool command-line-parser command-line-interface";

        /// <summary>
        /// minimum shell version required
        /// </summary>     
        public string OrbitalshellKernelVersion = "1.0.2";
        
        /// <summary>
        /// in: env.tools
        /// </summary>
        public string ModuleToolNamespace = "git";

        /// <summary>
        /// should be ModulePascalCaseID (auto == ModulePascalCaseID)
        /// </summary>
        public string ModuleToolVarSettingsName;

    }
}
