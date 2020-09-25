using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using System.IO;
using static DotNetConsoleAppToolkit.Lib.Str;
using static DotNetConsoleAppToolkit.DotNetConsole;
using System.Globalization;
using DotNetConsoleAppToolkit.Console;

namespace DotNetConsoleAppToolkit.Lib.FileSystem
{
    [CustomParamaterType]
    public class FileSystemPath
    {
        public static string ErrorColorization = $"{Red}";
        public static string NormalDirectoryColorization = $"{Blue}";
        public static string WritableDirectoryColorization = $"{Bdarkgreen}{White}";
        public static string SystemWritableDirectoryColorization = $"{Bdarkgreen}{Darkred}";
        public static string SystemColorization = $"{Red}";
        public static string FileColorization = $"";
        public static string ReadOnlyFileColorization = $"{Green}";

        public FileSystemInfo FileSystemInfo { get; protected set; }
        public string Name => FileSystemInfo.Name;
        public string FullName => FileSystemInfo.FullName;
        public string PrintableFullName
        {
            get
            {
                var quote = FullName.Contains(' ') ? "\"" : "";
                return $"{quote}{FullName}{quote}";
            }
        }
        public string PrintableName
        {
            get
            {
                var quote = Name.Contains(' ') ? "\"" : "";
                return $"{quote}{Name}{quote}";
            }
        }
        public string GetPrintableName(bool fullname = false) => fullname ? PrintableFullName : PrintableName;

        public string Error;

        public FileSystemPath(string fileSystemPath)
        {
            FileSystemInfo = new DirectoryInfo(fileSystemPath);
            if (!FileSystemInfo.Exists)
            {
                var fi = new FileInfo(fileSystemPath);
                if (fi.Exists) FileSystemInfo = fi;
            }
        }

        public FileSystemPath(FileSystemInfo fileSystemInfo)
        {
            FileSystemInfo = fileSystemInfo;
        }

        public virtual bool CheckExists(bool dumpError = true)
        {
            if (!FileSystemInfo.Exists)
            {
                if (dumpError)
                    Errorln($"path doesn't exists: {this}");
                return false;
            }
            return true;
        }

        public bool IsDirectory => FileSystemInfo.Attributes.HasFlag(FileAttributes.Directory);
        public bool IsFile => !IsDirectory;
        public bool HasError => Error != null;
        public bool IsReadOnly => FileSystemInfo.Attributes.HasFlag(FileAttributes.ReadOnly);
        public bool IsSystem => FileSystemInfo.Attributes.HasFlag(FileAttributes.System);
        public bool IsHidden => FileSystemInfo.Attributes.HasFlag(FileAttributes.Hidden);
        public bool IsArchive => FileSystemInfo.Attributes.HasFlag(FileAttributes.Archive);
        public bool IsCompressed => FileSystemInfo.Attributes.HasFlag(FileAttributes.Compressed);

        public string GetError() => $"{ErrorColorization}{Error}";

        public static FileSystemPath Get(FileSystemInfo fsinf)
        {
            if (fsinf.Attributes.HasFlag(FileAttributes.Directory))
                return new DirectoryPath(fsinf.FullName);
            else
                return new FilePath(fsinf.FullName);
        }

        public void Print(bool printAttributes=false,bool shortPath=false,string prefix="",string postfix="",int paddingRight=-1,string linePrefix="")
        {
            var bg = GetCmd(EchoDirectives.b + "", DefaultBackground.ToString().ToLower());
            var fg = GetCmd(EchoDirectives.f + "", DefaultForeground.ToString().ToLower());
            var color = (IsDirectory) ? NormalDirectoryColorization : FileColorization;
            if (!IsSystem && IsDirectory && !IsReadOnly) color += WritableDirectoryColorization;
            if (IsSystem && !IsDirectory) color += SystemColorization + bg;
            if (IsSystem && IsDirectory && !IsReadOnly) color += SystemWritableDirectoryColorization;
            if (IsFile && IsReadOnly) color += ReadOnlyFileColorization;
            var endcolor = bg + fg;
            var r = "";
            var attr = "";
            string hidden = "";
            if (printAttributes)
            {
                var dir = IsDirectory ? "d" : "-";
                var ro = IsReadOnly ? "r-" : "rw";
                var sys = IsSystem ? "s" : "-";
                var h = IsHidden ? "h" : "-";
                //var c = IsCompressed ? "c" : "-";
                var a = IsArchive ? "a" : "-";
                var size = (IsDirectory) ? "" : HumanFormatOfSize(((FileInfo)FileSystemInfo).Length, 2);
                var moddat = FileSystemInfo.LastWriteTime;
                hidden = IsHidden ? "*" : "";
                var dat = (moddat.Year != System.DateTime.Now.Year) ? moddat.Year+"" : "";
                var smoddat = $"{dat,4} {moddat.ToString("MMM", CultureInfo.InvariantCulture),-3} {moddat.Day,-2} {moddat.Hour.ToString().PadLeft(2,'0')}:{moddat.Minute.ToString().PadLeft(2,'0')}";
                attr = $" {dir}{ro}{sys}{h}{a} {size,10} {smoddat}  ";
            }
            var name = shortPath ? FileSystemInfo.Name : FileSystemInfo.FullName;
            var quote =name.Contains(' ') ? "\"" : "";
            var pdr = paddingRight - name.Length;
            if (!string.IsNullOrWhiteSpace(quote)) pdr -= 2;
            var rightspace = (paddingRight > -1) ? endcolor+"".PadRight(pdr>0?pdr:1, ' ') : "";
            r += $"{linePrefix}{attr}{color}{prefix}{quote}{name}{quote}{hidden}{rightspace}{postfix}";
            Out.Echo(r+ColorSettings.Default);
            if (HasError)
                Out.Echo($" {ErrorColorization}{GetError()}");
        }

        public override string ToString()
        {
            return FileSystemInfo.FullName;
        }
    }
}
