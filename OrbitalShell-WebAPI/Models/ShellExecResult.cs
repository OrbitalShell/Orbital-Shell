using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrbitalShell_WebAPI.Models
{
    public class ShellExecResult
    {
        static ShellExecResult _EmptyShellExecResult = null;
        public static ShellExecResult EmptyShellExecResult
        {
            get
            {
                if (_EmptyShellExecResult == null)
                    _EmptyShellExecResult = new ShellExecResult();
                return _EmptyShellExecResult;
            }
        }

        public string StdOut { get; set; } = String.Empty;

        public string StdErr { get; set; } = String.Empty;

        public int ReturnCode { get; set; } = 0;
    }
}
