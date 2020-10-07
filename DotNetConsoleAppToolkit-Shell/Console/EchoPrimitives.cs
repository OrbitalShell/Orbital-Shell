using DotNetConsoleAppToolkit.Component.CommandLine.Data;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Component.CommandLine.Variable;
using DotNetConsoleAppToolkit.Lib;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using static DotNetConsoleAppToolkit.DotNetConsole;

namespace DotNetConsoleAppToolkit.Console
{
    public static partial class EchoPrimitives
    {
        #region private

        static Table GetVarsDataTable(
            CommandEvaluationContext context,
            object container,
            List<IDataObject> values,
            TableFormattingOptions options)
        {
            var table = new Table();
            table.AddColumns("name", "type");
            table.Columns.Add("value", typeof(object));
            table.SetFormat("name", context.ShellEnv.Colors.Label + "{0}" + Rsf);
            table.SetFormat("type", context.ShellEnv.Colors.MediumDarkLabel + "{0}" + Tab + Rsf);
            table.SetHeaderFormat("type", "{0}" + Tab);
            foreach (var value in values)
            {
                AddIDataObjectToTable(context, table, value, options);
            }
            if (container != null && options.UnfoldItems)
            {
                AddObjectToTable(context, table, container, options);
            }
            return table;
        }

        static void AddObjectToTable(
            CommandEvaluationContext context,
            Table table,
            object obj,
            TableFormattingOptions options,
            int level = 0
            )
        {
            var tab = "".PadLeft((level * 4), ' ');
            var prfx = context.ShellEnv.Colors.HalfDarkLabel;
            foreach (var (name, value, inf) in obj.GetMemberValues())
            {
                if (value == null)
                {
                    table.Rows.Add(tab + prfx + name + Rsf, inf.GetMemberValueType().Name, DumpAsText(context,null));
                }
                else
                {
                    table.Rows.Add(tab + prfx + name + Rsf, inf.GetMemberValueType().Name, value);
                }
            }
        }

        static void AddIDataObjectToTable(
            CommandEvaluationContext context,
            Table table,
            IDataObject value,
            TableFormattingOptions options,
            int level = 0
            )
        {
            if (value == null)
            {
                table.Rows.Add(DumpAsText(context, null), DumpAsText(context, null), DumpAsText(context, null));
            }
            else
            {
                var tab = "".PadLeft((level * 4), ' ');
                var dv = value as DataValue;
                var valueType = (dv != null) ? dv.ValueType?.Name : value?.GetType().Name;
                var val = dv?.Value;
                var valnprefix = (dv == null) ? (context.ShellEnv.Colors.Highlight + "[+] ") : ""/*"    "*/;
                var valnostfix = (dv == null) ? "" : "";

                if (val == null)
                {
                    table.Rows.Add(
                        tab + valnprefix + value.Name + (value.IsReadOnly ? $"{context.ShellEnv.Colors.Symbol} r{Rdc}" : "") + valnostfix,
                        valueType,
                        ""
                        );
                }
                else
                {
                    table.Rows.Add(
                        tab + valnprefix + value.Name + (value.IsReadOnly ? $"{context.ShellEnv.Colors.Symbol} r{Rdc}" : "") + valnostfix,
                        valueType,
                        val
                        );

                    if (options.UnfoldItems && val != null)
                    {
                        AddObjectToTable(context, table, val, options, level + 1);
                    }
                }

                if ((value is DataObject dao) && options.UnfoldCategories)
                {
                    foreach (var attr in dao.GetAttributes())
                        AddIDataObjectToTable(context, table, attr, options, level + 1);
                }
            }
        }

        #endregion

        #region natives types

        public static void Echo(
            bool obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            @out.Echo($"{context.ShellEnv.Colors.Boolean}");
            @out.Echo(obj.ToString().ToLower());
            @out.Echo(Rdc);
        }

        public static void Echo(
            int obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            @out.Echo($"{context.ShellEnv.Colors.Integer}");
            @out.Echo(obj.ToString());
            @out.Echo(Rdc);
        }

        public static void Echo(
            double obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            @out.Echo($"{context.ShellEnv.Colors.Double}");
            @out.Echo(obj.ToString());
            @out.Echo(Rdc);
        }

        public static void Echo(
            float obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            @out.Echo($"{context.ShellEnv.Colors.Float}");
            @out.Echo(obj.ToString());
            @out.Echo(Rdc);
        }

        public static void Echo(
            decimal obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            @out.Echo($"{context.ShellEnv.Colors.Decimal}");
            @out.Echo(obj);
            @out.Echo(Rdc);
        }

        public static void Echo(
            char obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            @out.Echo($"{context.ShellEnv.Colors.Char}");
            @out.Echo(obj);
            @out.Echo(Rdc);
        }

        #endregion

        public static string NUllText = "{null}";

        public static string DumpAsText(CommandEvaluationContext context,object o, bool quoteStrings = true)
        {
            if (o == null) return context.ShellEnv.Colors.Debug+NUllText+Rdc ?? null;
            if (o is string s && quoteStrings) return $"\"{s}\"";
            return o.ToString();
        }

        public static void Echo(
            this KeyValuePair<string, object> obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            @out.Echo($"{obj.Key}{context.ShellEnv.Colors.HighlightSymbol}={context.ShellEnv.Colors.Value}");
            Echo(obj.Value, @out, context, options);
            @out.Echo(Rdc);
        }

        public static void InvokeEcho(
            object obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            MethodInfo mi;
            if ((mi = obj.GetEchoMethod()) != null)
                mi.InvokeEcho(obj, @out, context, options);
            else
            {
                var str = obj == null ? DumpAsText(context, obj) : obj.ToString();
                @out.Echo(str, (options != null) ? options.LineBreak : false);
            }
        }

        public static void Echo(
            this object obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            MethodInfo mi;
            if ((mi = obj.GetEchoMethod()) != null)
                mi.InvokeEcho(obj, @out, context, options);
            else
                @out.Echo(obj.ToString(), (options != null) ? options.LineBreak : false);
        }

        public static void Echo(
            this TextColor obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            FormattingOptions options = null)
        {
            var smbcol = context.ShellEnv.Colors.HighlightSymbol;
            var foregroundCol = (obj.Foreground.HasValue) ? (obj.Foreground.ToString() + $" {smbcol}{GetCmd(EchoDirectives.b + "", obj.Foreground.Value.ToString().ToLower())}  {context.ShellEnv.Colors.Default}") : "";
            var backgroundCol = (obj.Background.HasValue) ? (obj.Background.ToString() + $" {smbcol}{GetCmd(EchoDirectives.b + "", obj.Background.Value.ToString().ToLower())}  {context.ShellEnv.Colors.Default}") : "";
            var twice = !string.IsNullOrWhiteSpace(foregroundCol) && !string.IsNullOrWhiteSpace(backgroundCol);

            if (twice) @out.Echo(smbcol + "{");
            if (!string.IsNullOrWhiteSpace(foregroundCol)) @out.Echo(context.ShellEnv.Colors.Default + "f" + smbcol + "=" + context.ShellEnv.Colors.Name + foregroundCol);
            if (twice) @out.Echo(smbcol + ",");
            if (!string.IsNullOrWhiteSpace(backgroundCol)) @out.Echo(context.ShellEnv.Colors.Default + "g" + smbcol + "=" + context.ShellEnv.Colors.Name + backgroundCol);
            if (twice) @out.Echo(smbcol + "}");
        }

        public static void Echo(
            this ColorSettings obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null
            ) => ShellObject.Instance.EchoObj(obj, @out, context, options);

        #region variables & objects

        public static void DumpObject(
            object obj,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null
            )
        {
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableFormattingOptions);
            options = new TableFormattingOptions(options) { PadLastColumn = false };
            var dt = GetVarsDataTable(context, obj,new List<IDataObject>(), options);
            dt.Echo(@out, context, options);
        }

        public static void Echo(
            this IDataObject dataObject,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null
            )
        {
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableFormattingOptions);
            options = new TableFormattingOptions(options) { PadLastColumn = false };
            var attrs = dataObject.GetAttributes();
            attrs.Sort((x, y) => x.Name.CompareTo(y.Name));

            object container = null;
            if (dataObject is DataValue dataValue
                    && !(dataValue.Value is IDataObject))
            {
                container = dataValue.Value;
            } 

            var dt = GetVarsDataTable(context,container,attrs,options);
            dt.Echo( @out, context, options);
        }

        public static void Echo(
            this Variables variables,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null
            )
        {
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableFormattingOptions);
            var values = variables.GetDataValues();
            values.Sort((x, y) => x.Name.CompareTo(y.Name));
            var dt = GetVarsDataTable(context,null,values,options);
            dt.Echo( @out, context, options );
        }

        #endregion

        #region tables

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
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableFormattingOptions);
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
                        s = @out.GetPrint(t.GetFormatedValue(context,table.Columns[i].ColumnName, cols[i]?.ToString())) ?? "";
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
            var colsep = options.NoBorders ? " " : (context.ShellEnv.Colors.TableBorder + " | " + context.ShellEnv.Colors.Default);
            var colseplength = options.NoBorders ? 0 : 3;
            var tablewidth = options.NoBorders ? 0 : 3;
            for (int i = 0; i < table.Columns.Count; i++)
                tablewidth += table.Columns[i].ColumnName.PadRight(colLengths[i], ' ').Length + colseplength;
            var line = options.NoBorders ? "" : (context.ShellEnv.Colors.TableBorder + "".PadRight(tablewidth, '-'));

            if (!options.NoBorders) @out.Echoln(line);
            string fxh(string header) => (table is Table t) ? t.GetFormatedHeader(header) : header;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (i == 0) @out.Echo(colsep);
                var col = table.Columns[i];
                var colName = (i == table.Columns.Count - 1 && !options.PadLastColumn) ?
                    fxh(col.ColumnName) 
                    : fxh(col.ColumnName).PadRight(colLengths[i], ' ');
                var prfx = (options.NoBorders) ? Uon : "";
                var pofx = (options.NoBorders) ? Tdoff : "";
                @out.Echo(context.ShellEnv.Colors.TableColumnName + prfx + colName + colsep + pofx);
            }
            @out.Echoln();
            if (!options.NoBorders) @out.Echoln(line);

            string fhv(string header, string value) => (table is Table t) ? t.GetFormatedValue(context,header, value) : value;
            foreach (var rw in table.Rows)
            {
                if (context.CommandLineProcessor.CancellationTokenSource.IsCancellationRequested)
                {
                    @out.EnableFillLineFromCursor = true;
                    @out.ShowCur();
                    @out.Echoln(context.ShellEnv.Colors.Default + "");
                    return;
                }
                var row = (DataRow)rw;
                var arr = row.ItemArray;
                for (int i = 0; i < arr.Length; i++)
                {
                    if (i == 0) Out.Echo(colsep);
                    var txt = (arr[i] == null) ? "" : arr[i].ToString();
                    var fvalue = fhv(table.Columns[i].ColumnName, txt);
                    var o = arr[i];

                    MethodInfo mi = null;
                    if (o != null && (mi=o.GetEchoMethod())!=null)
                    {
                        // value dump via Echo primitive
                        @out.Echo(context.ShellEnv.Colors.Default);
                        /*if (mi.GetParameters().Length == 3)
                            mi.Invoke(o, new object[] { @out, context, null });
                        else
                            mi.Invoke(o, new object[] { o, @out, context, null });*/
                        mi.InvokeEcho(o, @out, context, null);
                        @out.Echo(colsep);
                    }
                    else
                    {
                        // value dump by ToString
                        var l = @out.GetPrint(fvalue).Length;
                        var spc = (i == arr.Length - 1 && !options.PadLastColumn) ? "" : ("".PadRight(Math.Max(0, colLengths[i] - l), ' '));
                        @out.Echo(context.ShellEnv.Colors.Default + fvalue + spc + colsep);
                    }
                }
                @out.Echoln();
            }
            @out.Echoln(line + context.ShellEnv.Colors.Default.ToString());
            @out.ShowCur();
            @out.EnableFillLineFromCursor = true;
        }

        #endregion
    }
}
