using System;
using System.Diagnostics.CodeAnalysis;

namespace OrbitalShell.Lib.Data
{
    /// <summary>
    /// a version number according to nupkg versions numbers
    /// </summary>
    /// <remarks>
    /// {major}[.{minor}[.{fix}[.{build}|(-{versionType}[-{preVersionNumber})]<br/>
    /// examples:<br/>
    /// 1.0.0.0<br/>
    /// 1.0.0<br/>
    /// 1.0<br/>
    /// 1<br/>
    /// 1.0.0-beta (is lower than 1.0.0)<br/>
    /// 1.0.0-beta-4 (is upper than 1.0.0-beta, lower than 1.0.0)
    /// </remarks>
    public class VersionNumber : IComparable<VersionNumber>
    {
        public readonly int Major = 0;
        public readonly int Minor = 0;
        public readonly int Fix = 0;
        public readonly int Build = 0;
        public readonly string VersionType = null;
        public readonly int PreVersionNumber = 0;

        public const string VersionNumberSyntax = "{major}[.{minor}[.{fix}[.{build}|(-{versionType}[-{preVersionNumber})]";

        public VersionNumber(string s)
        {            
            if (s == null) throw new ArgumentNullException(nameof(s));
            var t = s.Split(".");
            var ats = $" (attempted syntax is: {VersionNumberSyntax})";
            string bsn(string name) => $"bad syntax. {name} version must be an int"+ats;
            string bsvt(string name,string err) => $"bad syntax. {name} {err}" + ats;
            if (t.Length > 4) throw new ArgumentException("bad syntax."+ats, nameof(s));
            if (int.TryParse(t[0], out var major)) Major = major;
            else throw new ArgumentException(bsn("major"));
            if (t.Length > 1)
            {
                if (int.TryParse(t[1], out var minor)) Minor = minor;
                else throw new ArgumentException(bsn("minor"));
            }
            if (t.Length > 2)
            {
                if (int.TryParse(t[2], out var fix)) Fix = fix;
                else throw new ArgumentException(bsn("fix"));
            }
            if (t.Length > 3)
            {
                var n = t[3];
                if (int.TryParse(n, out var build)) Build = build;
                else
                {
                    if (!n.StartsWith("-"))
                        throw new ArgumentException(bsvt("versionType", "must starts with -"));
                    else
                    {
                        n = n.Substring(1);
                        var vt = n.Split("-");
                        if (vt.Length == 1)
                            VersionType = n;
                        else
                        {
                            VersionType = vt[0];
                            if (int.TryParse(vt[1], out var preVersionNumber)) PreVersionNumber = preVersionNumber;
                            else throw new ArgumentException(bsn("preVersionNumber"));
                        }
                    }
                }
            }
        }

        public override bool Equals(object obj) => 
            obj is VersionNumber n 
            && n.Major == Major 
            && n.Minor == Minor 
            && n.Fix == Fix 
            && n.Build == Build 
            && n.VersionType == VersionType 
            && n.PreVersionNumber == PreVersionNumber;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo([AllowNull] VersionNumber other)
        {
            if (other == null) return 1;
            if (other.Major > Major) return -1;
            if (other.Minor > Minor) return -1;
            if (other.Fix > Fix) return -1;
            if (other.Build > Build) return -1;
            if (VersionType == null && other.VersionType != null) return 1;
            if (other.PreVersionNumber > PreVersionNumber) return -1;
            return 0;
        }
    }
}
