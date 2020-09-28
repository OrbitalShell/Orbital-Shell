using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Lib;
using System.Reflection;
using System.Text;

namespace DotNetConsoleAppToolkit.Console
{
    public class DynamicallyEchoable
    {
       public static DynamicallyEchoable _instance;
       public static DynamicallyEchoable Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DynamicallyEchoable();
                return _instance;
            }
        }

        public void Echo(
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null
            ) => EchoObj(this, @out, context, options);

        public void EchoObj(
            object o,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null
            )
        { 
            var smbcol = context.ShellEnv.Colors.Highlight;
            @out.Echo(smbcol + "{");
            bool firstElement = true;
            foreach (var m in o.GetMemberValues())
            {
                if (!firstElement) @out.Echo(smbcol + ",");
                else firstElement = false;
                @out.Echo(
                    context.ShellEnv.Colors.Default + m.Item1 +
                    smbcol + "=" +
                    context.ShellEnv.Colors.Name /*+ Str.DumpAsText(m.Item2)*/
                    );
                MethodInfo mi = null;
                var value = m.Item2;
                if (value != null && (mi = value.GetEchoMethod()) != null)
                {
                    mi.InvokeEcho(value, @out, context, null);
                    @out.Echo(context.ShellEnv.Colors.Default);
                } else
                {
                    @out.Echo(Str.DumpAsText(m.Item2));
                }
            }
            @out.Echo(smbcol + "}");
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('{');
            bool firstElement=true;
            foreach (var m in this.GetMemberValues())
            {
                if (!firstElement) sb.Append(','); else firstElement = false;
                sb.Append($"{m.Item1}={Str.DumpAsText(m.Item2)}");
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
