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
        #region echo tanble builders

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
                var attrs = GetMemberConstraintsText(context,inf);
                if (value == null)
                {
                    table.Rows.Add(tab + prfx + name + attrs + Rdc, inf.GetMemberValueType().Name, DumpAsText(context,null));
                }
                else
                {
                    table.Rows.Add(tab + prfx + name + attrs + Rdc, inf.GetMemberValueType().Name, value);
                }
            }
        }

        static string GetMemberConstraintsText(
            CommandEvaluationContext context,
            MemberInfo mi)
        {
            string r = null;
            bool isStatic = false;
            bool readOnly = false;
            bool canRead = true;
            if (mi is FieldInfo f)
            {
                isStatic = f.IsStatic;
                readOnly = f.IsInitOnly;
                canRead = f.IsPublic;
            }
            if (mi is PropertyInfo p)
            {
                isStatic = p.GetGetMethod().IsStatic;
                readOnly = !p.CanWrite;
                canRead = p.CanRead;
            }
            if (isStatic) r += "s";
            if (readOnly) r += "r";
            if (!canRead) r += "-";
            if (r != null)
                r = context.ShellEnv.Colors.Symbol.ToString() + " " + r + Rdc;
            return r;
        }

        static string GetIDataOjbectContraintsText(
            CommandEvaluationContext context,
            IDataObject obj)
        {
            string r = null;

            bool isStatic = false;
            bool readOnly = obj.IsReadOnly;
            bool canRead = true;
            
            if (isStatic) r += "s";
            if (readOnly) r += "r";
            if (!canRead) r += "-";
            if (r != null)
                r = context.ShellEnv.Colors.Symbol.ToString() + " " + r + Rdc;
            return r;
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
                var foldSymbol = options.UnfoldCategories ? "[-]" : "[+]";
                var valnprefix = (dv == null) ? (context.ShellEnv.Colors.Highlight + foldSymbol + " ") : "";
                var attrs = GetIDataOjbectContraintsText(context,value);

                var str = tab + valnprefix + value.Name + attrs;

                if (val == null)
                {
                    table.Rows.Add(
                        str,
                        valueType,
                        ""
                        );
                }
                else
                {
                    table.Rows.Add(
                        str,
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
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Boolean}");
            @out.Echo(obj.ToString().ToLower());
            @out.Echo(Rdc);
        }

        public static void Echo(
            int obj,
            EchoEvaluationContext ctx)
    {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Integer}");
            @out.Echo(obj.ToString());
            @out.Echo(Rdc);
        }

        public static void Echo(
            double obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Double}");
            @out.Echo(obj.ToString());
            @out.Echo(Rdc);
        }

        public static void Echo(
            float obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Float}");
            @out.Echo(obj.ToString());
            @out.Echo(Rdc);
        }

        public static void Echo(
            decimal obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Decimal}");
            @out.Echo(obj);
            @out.Echo(Rdc);
        }

        public static void Echo(
            char obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

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
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj,ctx)) return;

            @out.Echo($"{obj.Key}{context.ShellEnv.Colors.HighlightSymbol}={context.ShellEnv.Colors.Value}");
            Echo(obj.Value, ctx);
            @out.Echo(Rdc);
        }

        public static void InvokeEcho(
            object obj,
            EchoEvaluationContext ctx
            )
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            // TODO map invoke
            MethodInfo mi;
            if ((mi = obj.GetEchoMethod()) != null)
                mi.InvokeEcho(obj, ctx);
            else
            {
                var str = obj == null ? DumpAsText(context, obj) : obj.ToString();
                @out.Echo(str, (options != null) ? options.LineBreak : false);
            }
        }

        // -------------------------------------------------------------------------------------------------

        /// <summary>
        /// echo fallback method
        /// </summary>
        /// <param name="obj">any object</param>
        /// <param name="ctx">echo evaluation context</param>
        public static void Echo(
            this object obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            // mandatory for having both :
            // 1. static extension methods to nicely write Echo calls from the code (eg. DataTable.Echo , object.Echo ...)
            // 2. Echo calls that can be remapped to others methods than the class one's (extension or owned method)
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            MethodInfo mi;
            if ((mi = obj.GetEchoMethod()) != null)
                mi.InvokeEcho(obj, ctx);
            else
                @out.Echo(obj.ToString(), (options != null) ? options.LineBreak : false);
        }

        // -------------------------------------------------------------------------------------------------

        public static void Echo(
            this TextColor obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

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
            this ConsoleColor obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, _) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            var smbcol = context.ShellEnv.Colors.HighlightSymbol;
            var str = $" {smbcol}{GetCmd(EchoDirectives.b + "", obj.ToString().ToLower())}  {context.ShellEnv.Colors.Default}";

            @out.Echo(smbcol + "{");
            @out.Echo(context.ShellEnv.Colors.Default + obj.ToString() + " " + context.ShellEnv.Colors.Name + str);
            @out.Echo(smbcol + "}");
        }

        public static void Echo(
            this ColorSettings obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            ShellObject.Instance.EchoObj(obj, ctx);
        }

        #region variables & objects

        public static void DumpObject(
            object obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, opts) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            var options = opts as TableFormattingOptions;
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableFormattingOptions);
            options = new TableFormattingOptions(options) { PadLastColumn = false };
            var dt = GetVarsDataTable(context, obj,new List<IDataObject>(), options);
            dt.Echo( new EchoEvaluationContext(@out,context,options));
        }

        public static void Echo(
            this IDataObject dataObject,
            EchoEvaluationContext ctx)
        {
            var (@out, context, opts) = ctx;
            if (context.EchoMap.MappedCall(dataObject, ctx)) return;

            var options = opts as TableFormattingOptions;
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
            dt.Echo( new EchoEvaluationContext(@out,context,options));
        }

        public static void Echo(
            this Variables variables,
            EchoEvaluationContext ctx)
        {
            var (@out, context, opts) = ctx;
            if (context.EchoMap.MappedCall(variables, ctx)) return;

            var options = opts as TableFormattingOptions;
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.Display_TableFormattingOptions);
            var values = variables.GetDataValues();
            values.Sort((x, y) => x.Name.CompareTo(y.Name));
            var dt = GetVarsDataTable(context,null,values,options);
            dt.Echo( new EchoEvaluationContext(@out,context,options));
        }

        #endregion

        #region tables

        public static void Echo(
            this DataTable table,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(table, ctx)) return;
            _Echo(table, @out, context, (TableFormattingOptions)options);
        }

        public static void Echo(
            this Table table,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(table, ctx)) return;
            _Echo(table, @out, context, (TableFormattingOptions)options);
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
                        mi.InvokeEcho(o, new EchoEvaluationContext(@out, context, null));
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
