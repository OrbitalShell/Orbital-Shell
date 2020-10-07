using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Lib;
using System.Reflection;
using System.Text;
using static DotNetConsoleAppToolkit.Console.EchoPrimitives;

namespace DotNetConsoleAppToolkit.Console
{
    /// <summary>
    /// dynamic outputable & convertible object that integrates with the shell
    /// </summary>
    public class ShellObject : IShellObject
    {
        public static ShellObject _instance;
        public static ShellObject Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ShellObject();
                return _instance;
            }
        }

        /// <summary>
        /// Echo method
        /// </summary>
        /// <param name="out"></param>
        /// <param name="context"></param>
        /// <param name="options"></param>
        public void Echo(
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null
            ) => EchoObj(this, @out, context, options);

        /// <summary>
        /// Echo method for any object
        /// </summary>
        /// <param name="o"></param>
        /// <param name="out"></param>
        /// <param name="context"></param>
        /// <param name="options"></param>
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
                }
                else
                {
                    @out.Echo(DumpAsText(context, m.Item2));
                }
            }
            @out.Echo(smbcol + "}");
        }

        /// <summary>
        /// returns the object as text representation of its value (the returned value might be convertible to a native value)
        /// <para>this method must be in place of ToString() to provide a string value to describe the object. ToString must be reserved for debug purpose only</para>
        /// </summary>
        /// <returns>readable value representing the object</returns>
        public string AsText(CommandEvaluationContext context)
        {
            var sb = new StringBuilder();
            sb.Append('{');
            bool firstElement = true;
            foreach (var m in this.GetMemberValues())
            {
                if (!firstElement) sb.Append(','); else firstElement = false;
                sb.Append($"{m.Item1}={DumpAsText(context, m.Item2)}");
            }
            sb.Append('}');
            return sb.ToString();
        }

        /// <summary>
        /// debug print
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append('{');
            bool firstElement = true;
            foreach (var m in this.GetMemberValues())
            {
                if (!firstElement) sb.Append(','); else firstElement = false;
                sb.Append($"{m.Item1}={Str.DumpAsText( m.Item2 )}");
            }
            sb.Append('}');
            return sb.ToString();
        }
    }
}
