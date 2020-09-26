using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Component.CommandLine.Variable;
using DotNetConsoleAppToolkit.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using static DotNetConsoleAppToolkit.DotNetConsole;
using static DotNetConsoleAppToolkit.Lib.Str;

namespace DotNetConsoleAppToolkit.Console
{
    public static partial class EchoPrimitives
    {
        static Table GetVarsDataTable(
            List<IDataObject> values,
            TableFormattingOptions options)
        {
            var table = new Table();
            table.AddColumns("name", "type", "value");
            table.SetFormat("name", ColorSettings.DarkLabel + "{0}" + Rsf);
            table.SetFormat("type", ColorSettings.Label + "{0}" + Tab + Rsf);
            table.SetHeaderFormat("type", "{0}" + Tab);
            foreach (var value in values)
            {
                AddIDataObjectToTable(table, value, options);
            }
            return table;
        }

        static void AddIDataObjectToTable(
            Table table,
            IDataObject value,
            TableFormattingOptions options,
            int level = 0
            )
        {
            if (value == null)
            {
                table.Rows.Add(DumpAsText(null), DumpAsText(null), DumpAsText(null));
            }
            else
            {
                var tab = "".PadLeft((level * 2), ' ');
                var dv = value as DataValue;
                var valueType = (dv != null) ? dv.ValueType.Name : value.GetType().Name;
                var val = (dv != null) ? DumpAsText(dv.Value, false) : string.Empty;
                var valnprefix = (dv == null) ? (ColorSettings.Highlight + "[+] ") : "    ";
                var valnostfix = (dv == null) ? "" : "";

                table.Rows.Add(
                    valnprefix + tab + value.Name + (value.IsReadOnly ? "(r)" : "") + valnostfix,
                    valueType,
                    DumpAsText(val, false));

                if ((value is DataObject dao) && options.UnfoldAll)
                {
                    foreach ( var attr in dao.GetAttributes() )
                        AddIDataObjectToTable(table, attr, options, level+1);
                }
            }
        }

        public static void Echo(
            this IDataObject dataObject,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null
            )
        {
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableSettings);
            options = new TableFormattingOptions(options) { PadLastColumn = false };
            var attrs = dataObject.GetAttributes();
            attrs.Sort((x, y) => x.Name.CompareTo(y.Name));

            var dt = GetVarsDataTable(attrs,options);
            dt.Echo( @out, context, options);
        }

        public static void Echo(
            this Variables variables,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null
            )
        {
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableSettings);
            var values = variables.GetDataValues();
            values.Sort((x, y) => x.Name.CompareTo(y.Name));
            var dt = GetVarsDataTable(values,options);
            dt.Echo( @out, context, options );
        }

        public static void Echo(
            this DataTable table,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null)
        {
            _Echo(table, @out, context, options);
        }

        public static void Echo(
            this Table table,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null)
        {
            _Echo(table, @out, context, options);
        }

        static void _Echo(
            this DataTable table,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null)
        {
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableSettings);
            @out.EnableFillLineFromCursor = false;
            @out.HideCur();
            var colLengths = new int[table.Columns.Count];
            foreach (var rw in table.Rows)
            {
                var cols = ((DataRow)rw).ItemArray;
                for (int i = 0; i < cols.Length; i++)
                {
                    string s = null, s2 = null;
                    if (table is Table t)
                    {
                        s = @out.GetPrint(t.GetFormatedValue(table.Columns[i].ColumnName, cols[i]?.ToString())) ?? "";
                        colLengths[i] = Math.Max(s.Length, colLengths[i]);
                        s2 = @out.GetPrint(t.GetFormatedHeader(table.Columns[i].ColumnName)) ?? "";
                        colLengths[i] = Math.Max(s2.Length, colLengths[i]);
                    }
                    else
                    {
                        s = @out.GetPrint(cols[i]?.ToString()) ?? "";
                        colLengths[i] = Math.Max(s.Length, colLengths[i]);
                        colLengths[i] = Math.Max(table.Columns[i].ColumnName.Length, colLengths[i]);
                    }
                }
            }
            var colsep = options.NoBorders ? " " : (ColorSettings.TableBorder + " | " + ColorSettings.Default);
            var colseplength = options.NoBorders ? 0 : 3;
            var tablewidth = options.NoBorders ? 0 : 3;
            for (int i = 0; i < table.Columns.Count; i++)
                tablewidth += table.Columns[i].ColumnName.PadRight(colLengths[i], ' ').Length + colseplength;
            var line = options.NoBorders ? "" : (ColorSettings.TableBorder + "".PadRight(tablewidth, '-'));

            if (!options.NoBorders) @out.Echoln(line);
            string fxh(string header) => (table is Table t) ? t.GetFormatedHeader(header) : header;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i == 0) @out.Echo(colsep);
                var col = table.Columns[i];
                var colName = (i == table.Columns.Count - 1 && !options.PadLastColumn) ?
                    /*table.GetFormatedHeader*/fxh(col.ColumnName) 
                    : /*table.GetFormatedHeader*/fxh(col.ColumnName).PadRight(colLengths[i], ' ');
                var prfx = (options.NoBorders) ? Uon : "";
                var pofx = (options.NoBorders) ? Tdoff : "";
                @out.Echo(ColorSettings.TableColumnName + prfx + colName + colsep + pofx);
            }
            @out.Echoln();
            if (!options.NoBorders) @out.Echoln(line);

            string fhv(string header, string value) => (table is Table t) ? t.GetFormatedValue(header, value) : value;
            foreach (var rw in table.Rows)
            {
                if (context.CommandLineProcessor.CancellationTokenSource.IsCancellationRequested)
                {
                    @out.EnableFillLineFromCursor = true;
                    @out.ShowCur();
                    @out.Echoln(ColorSettings.Default + "");
                    return;
                }
                var row = (DataRow)rw;
                var arr = row.ItemArray;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i == 0) Out.Echo(colsep);
                    var txt = (arr[i] == null) ? "" : arr[i].ToString();
                    var fvalue = /*table.GetFormatedValue*/fhv(table.Columns[i].ColumnName, txt);
                    var l = Out.GetPrint(fvalue).Length;
                    var spc = (i == arr.Length - 1 && !options.PadLastColumn) ? "" : ("".PadRight(Math.Max(0, colLengths[i] - l), ' '));
                    @out.Echo(ColorSettings.Default + fvalue + spc + colsep);
                }
                @out.Echoln();
            }
            @out.Echoln(line + ColorSettings.Default.ToString());
            @out.ShowCur();
            @out.EnableFillLineFromCursor = true;
        }
    }
}
