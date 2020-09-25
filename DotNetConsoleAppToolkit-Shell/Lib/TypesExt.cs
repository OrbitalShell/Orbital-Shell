using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Console;
using System.Data;
using System.Threading;
using prim = DotNetConsoleAppToolkit.Console.EchoPrimitives;

namespace DotNetConsoleAppToolkit.Lib
{
    public static partial class TypesExt
    {
        public static void Echo(this DataTable x, ConsoleTextWriterWrapper @out, CancellationTokenSource cancellationTokenSource, bool noBorders = false, bool padLastColumn = true) => prim.Print(@out, cancellationTokenSource, x, noBorders, padLastColumn);
        public static void Echo(this Table x, ConsoleTextWriterWrapper @out, CancellationTokenSource cancellationTokenSource, bool noBorders = false,bool padLastColumn = true) => prim.Print(@out, cancellationTokenSource, x, noBorders,padLastColumn);
    }
}
