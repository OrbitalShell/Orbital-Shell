using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Component.Shell;
using OrbitalShell.Component.Console;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using static OrbitalShell.Component.Console.EchoPrimitives;
using static OrbitalShell.Lib.Str;
using OrbitalShell.Component.CommandLine;

namespace OrbitalShell.Commands
{
    [Commands("system commands")]
    [CommandsNamespace(CommandNamespace.sys)]
    public class SystemCommands : ICommandsDeclaringType
    {
        [Command("print a report of current processes")]
        public CommandResult<List<Process>> Ps(
            CommandEvaluationContext context,
            [Option("b", "borders", "if set add table borders")] bool borders,
            [Option("s", "sid", "filter by session id", true, true)] int fsid = -1,
            [Option("p", "pid", "filter by process id", true, true)] int fpid = -1
            )
        {
            var r = new List<Process>();
            var maxnamel = 40;
            var processes = Process.GetProcesses();
            var table = new DataTable();
            var cpid = table.Columns.Add("pid");
            var sid = table.Columns.Add("sid");
            var cname = table.Columns.Add("name");
            var cbp = table.Columns.Add("pri");
            var cws = table.Columns.Add("ws");
            //var cprms = table.Columns.Add("prms");
            var cpgms = table.Columns.Add("pgms");
            //var ctim = table.Columns.Add("time");
            //var ctpt = table.Columns.Add("tpt");
            //var cvms = table.Columns.Add("vms");
            //var ctitle = table.Columns.Add("window title");
            //var cmname = table.Columns.Add("Module name");
            foreach (var process in processes)
            {
                var select = true;
                select &= (fsid == -1 || process.SessionId == fsid);
                select &= (fpid == -1 || process.Id == fpid);

                var row = table.NewRow();
                row[cpid] = process.Id;
                row[sid] = process.SessionId;
                var n = process.ProcessName;
                if (n.Length > maxnamel) n = n.Substring(0, maxnamel) + "...";
                row[cname] = n;
                //row[cmname] = process.MainModule.FileName;
                //row[cvms] = HumanFormatOfSize(process.VirtualMemorySize64, 2);
                row[cpgms] = HumanFormatOfSize(process.PagedMemorySize64, 2);
                row[cws] = HumanFormatOfSize(process.WorkingSet64, 2);
                //row[cprms] = HumanFormatOfSize(process.PrivateMemorySize64, 2);
                //var tim = DateTime.Now - process.StartTime;
                //row[ctim] = $"{tim.Hours.ToString().PadLeft(2, '0')}:{tim.Minutes.ToString().PadLeft(2, '0')}:{tim.Seconds.ToString().PadLeft(2,'0')}";
                row[cbp] = process.BasePriority;
                //row[ctpt] = process.TotalProcessorTime+" %";
                //row[ctitle] = process.MainWindowTitle;

                if (select)
                {
                    table.Rows.Add(row);
                    r.Add(process);
                }
            }

            table.Echo(
                new EchoEvaluationContext(context.Out, context,
                    new TableFormattingOptions(context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.display_tableFormattingOptions))
                    { NoBorders = !borders }));

            return new CommandResult<List<Process>>(r);
        }

        [Command("get information about current user")]
        public CommandResult<(string userName, string userDomainName)> Whoami(
            CommandEvaluationContext context
            )
        {
            context.Out.Echoln($"{Environment.UserName} [{context.ShellEnv.Colors.Highlight}{Environment.UserDomainName}{context.ShellEnv.Colors.Default}]");
            return new CommandResult<(string, string)>((Environment.UserName, Environment.UserDomainName));
        }
    }
}
