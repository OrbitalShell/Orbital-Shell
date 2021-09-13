using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;

using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.Console.Formats;
using OrbitalShell.Component.EchoDirective;
using OrbitalShell.Component.Shell.Data;
using OrbitalShell.Component.Shell.Module;
using OrbitalShell.Component.Shell.Variable;
using OrbitalShell.Lib;
using OrbitalShell.Lib.Extensions;
using OrbitalShell.Lib.Sys;

using static OrbitalShell.Component.EchoDirective.Shortcuts;

namespace OrbitalShell.Component.Console
{
    /// <summary>
    /// these class concentrates methods that 'echoize' any object depending on object type
    /// </summary>
    public static partial class EchoPrimitives
    {
        #region echo table builders 

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
                AddIDataObjectToTable(
                    context,
                    table,
                    value,
                    (TableFormattingOptions)options
                        .Clone()
                        .AddLevel());
            }
            if (container != null && options.UnfoldItems)
            {
                AddObjectToTable(
                    context,
                    table,
                    container,
                    (TableFormattingOptions)options
                        .Clone()
                        .AddLevel());
            }
            return table;
        }

        private const int TabLength = 4;

        static void AddObjectToTable(
            CommandEvaluationContext context,
            Table table,
            object obj,
            TableFormattingOptions options,
            int level = 0
            )
        {
            var tab = "".PadLeft(level * TabLength, ' ');
            var prfx = context.ShellEnv.Colors.HalfDarkLabel;
            foreach (var (name, value, inf) in obj.GetMemberValues())
            {
                var attrs = GetMemberConstraintsText(context, inf);
                if (value == null)
                {
                    table.Rows.Add(tab + prfx + name + attrs + Rdc, inf.GetMemberValueType().UnmangledName(), DumpAsText(context, null));
                }
                else
                {
                    table.Rows.Add(tab + prfx + name + attrs + Rdc, inf.GetMemberValueType().UnmangledName(), value);
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
                var tab = "".PadLeft((level * TabLength), ' ');
                var dv = value as DataValue;
                var valueType = (dv != null) ? dv.ValueType?.UnmangledName() : value?.GetType().UnmangledName();
                var val = dv?.Value;
                var foldSymbol = options.UnfoldCategories ? "[-]" : "[+]";
                var valnprefix = (dv == null) ? (context.ShellEnv.Colors.Highlight + foldSymbol + " ") : "";
                var attrs = GetIDataOjbectContraintsText(context, value);

                var str = tab + valnprefix + value.Name + attrs;

                if (val == null)
                {
                    table.Rows.Add(
                        str,
                        valueType,
                        new StringWrapper(
                            (dv == null) ? "" : DumpAsText(context, null)
                        ));
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
            DBNull obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo(
                DumpAsText(
                    ctx.CommandEvaluationContext,
                    obj,
                    false)
                , (ctx.Options != null) && options.LineBreak
                , false);
        }

        public static void Echo(
            string obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            if (obj.Length == 0)
            {
                obj = QuotedString(context, obj);
                @out.Echo(obj, (ctx.Options != null) && options.LineBreak, false);
            }
            else
                @out.Echo(obj, (ctx.Options != null) && options.LineBreak, (ctx.Options != null) && ctx.Options.IsRawModeEnabled);
        }

        public static void Echo(
            bool obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo("" + (obj ? context.ShellEnv.Colors.BooleanTrue : context.ShellEnv.Colors.BooleanFalse));
            @out.Echo(obj.ToString().ToLower(), (options != null) && options.LineBreak);
            @out.Echo(Rdc);
        }

        public static void Echo(
            int obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Integer}");
            @out.Echo(obj.ToString(), options != null && options.LineBreak);
            @out.Echo(Rdc);
        }

        public static void Echo(
            double obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Double}");
            @out.Echo(obj.ToString(), options != null && options.LineBreak);
            @out.Echo(Rdc);
        }

        public static void Echo(
            float obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Float}");
            @out.Echo(obj.ToString(), options != null && options.LineBreak);
            @out.Echo(Rdc);
        }

        public static void Echo(
            decimal obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Decimal}");
            @out.Echo(obj.ToString(), options != null && options.LineBreak);
            @out.Echo(Rdc);
        }

        public static void Echo(
            char obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{context.ShellEnv.Colors.Char}");
            @out.Echo(obj, options != null && ctx.Options.LineBreak);
            @out.Echo(Rdc);
        }

        #endregion

        #region -- public util --

        // TODO: from config
        public static string NullText = "{null}";

        public static string DbNullText = "{null}";

        public static string DumpNull(CommandEvaluationContext context) => DumpAsText(context, null);

        public static string DumpAsText(CommandEvaluationContext context, object o, bool quoteStrings = true)
        {
            if (o == null)
                return context.ShellEnv.Colors.Null + NullText + Rdc;
            if (o is DBNull)
                return context.ShellEnv.Colors.Null + DbNullText + Rdc;

            if (o is string s && quoteStrings) return $"\"{s}\"";
            return o.ToString();
        }

        public static string QuotedString(CommandEvaluationContext context, string s)
            => $"{context.ShellEnv.Colors.Quotes}\"{context.ShellEnv.Colors.Default}{s}{context.ShellEnv.Colors.Quotes}\"{context.ShellEnv.Colors.Default}";

        #endregion

        #region compounded & collection types

        public static void Echo(
            this ICollection obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            var i = 1;
            var nb = obj.Count;
            foreach (var o in obj)
            {
                Echo(o,
                    new EchoEvaluationContext(
                        ctx,
                        ctx
                            .Options
                            .Clone()
                            .Apply(o => o.LineBreak = false)
                        ));

                if (i < nb)
                {
                    if (!ctx.Options.LineBreak)
                        /*TODO: currently no way to support option change from any context (see: shell meta-options + output filters )*/
                        @out.Echo(ShellEnvironment.SystemPathSeparator);
                    else
                        @out.Echoln();
                }
                else if (ctx.Options.LineBreak)
                    @out.Echoln();
                i++;
            }
        }

        public static void Echo(
            this List<object> obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            var i = 1;
            var nb = obj.Count;
            foreach (var o in obj)
            {
                Echo(o, new EchoEvaluationContext(ctx, new FormatingOptions(ctx.Options) { LineBreak = false }));
                if (i < nb)
                {
                    if (!ctx.Options.LineBreak)
                        @out.Echo(ShellEnvironment.SystemPathSeparator);
                    else
                        @out.Echoln();
                }
                else if (ctx.Options.LineBreak) @out.Echoln();
                i++;
            }
        }

        public static void Echo(
            this Array obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            var i = 1;
            var nb = obj.Length;
            foreach (var o in obj)
            {
                Echo(o, new EchoEvaluationContext(ctx, new FormatingOptions(ctx.Options) { LineBreak = false }));
                if (i < nb)
                {
                    if (!ctx.Options.LineBreak)
                        @out.Echo(ShellEnvironment.SystemPathSeparator);
                    else
                        @out.Echoln();
                }
                else if (ctx.Options.LineBreak) @out.Echoln();
                i++;
            }
        }

        public static void Echo(
            this object[] obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            var i = 1;
            var nb = obj.Length;
            foreach (var o in obj)
            {
                Echo(o, new EchoEvaluationContext(ctx, new FormatingOptions(ctx.Options) { LineBreak = false }));
                if (i < nb)
                {
                    if (!ctx.Options.LineBreak)
                        @out.Echo(ShellEnvironment.SystemPathSeparator);
                    else
                        @out.Echoln();
                }
                else if (ctx.Options.LineBreak) @out.Echoln();
                i++;
            }
        }

        public static void Echo(
            this string[] obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            var i = 1;
            var nb = obj.Length;
            foreach (var o in obj)
            {
                Echo(o, new EchoEvaluationContext(ctx, new FormatingOptions(ctx.Options) { LineBreak = false }));
                if (i < nb)
                {
                    if (!ctx.Options.LineBreak && i < nb)
                        @out.Echo(ShellEnvironment.SystemPathSeparator);
                    else
                        @out.Echoln();
                }
                else if (ctx.Options.LineBreak) @out.Echoln();
                i++;
            }
        }

        public static void Echo(
            this KeyValuePair<string, object> obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            @out.Echo($"{obj.Key}{context.ShellEnv.Colors.HighlightSymbol}={context.ShellEnv.Colors.Value}");
            Echo(obj.Value, ctx);
            @out.Echo(Rdc);     // TODO: check this
        }

        #endregion

        // TODO: check: EchoEvaluationContext.LineBreak seems to generally never been used

        #region system library types

        public static void Echo(
            this Exception obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, _) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;

            var m = obj.ToString();
            var i = m.IndexOf(':');
            var textCol = ctx.CommandEvaluationContext.ShellEnv.Colors.ExceptionText;
            var pfxCol = ctx.CommandEvaluationContext.ShellEnv.Colors.ExceptionName;
            m = (i > -1) ? pfxCol + m.Substring(0, i + 1) + ANSI.RSTXTA + " " + textCol + ANSI.SGR_Underline + m.Substring(i + 2).Replace(ANSI.CRLF, ANSI.CRLF + ANSI.SGR_UnderlineOff + textCol) : textCol + m;

            @out.Echo(m + ANSI.RSTXTA);
        }

        #endregion

        #region library types

        public static void Echo(
            this ModuleVersion obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, _) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;
            @out.Echo($"{context.ShellEnv.Colors.Integer}{obj}(rdc)");
        }

        public static void Echo(
            this StringWrapper obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, _) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;
            @out.Echo(obj.ToString());
        }

        public static void Echo(
            this ReturnCode obj,
            EchoEvaluationContext ctx)
        {
            var (@out, context, _) = ctx;
            if (context.EchoMap.MappedCall(obj, ctx)) return;
            var r = "";
            switch (obj)
            {
                case ReturnCode.Error:
                    r += context.ShellEnv.Colors.BoxError;
                    break;
                case ReturnCode.OK:
                    r += context.ShellEnv.Colors.BoxOk;
                    break;
                case ReturnCode.Unknown:
                    r += context.ShellEnv.Colors.BoxUnknown;
                    break;
                case ReturnCode.NotIdentified:
                    r += context.ShellEnv.Colors.BoxNotIdentified;
                    break;
            }
            @out.Echo(r + obj + ANSI.RSTXTA);
        }

        #endregion

        // 🚩 -------------------------------------------------------------------------------------------------

        #region object type (bridges)

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

        /// <summary>
        /// echo object fallback method
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

            // handle interfaces types : TODO: should be handlable throught EchoMap
            if (obj is ICollection collection)
            {
                collection.Echo(ctx);
                return;
            }

            MethodInfo mi;
            if ((mi = obj.GetEchoMethod()) != null)
                mi.InvokeEcho(obj, ctx);
            else
            {
                // object fallback print before to string
                if (!ctx.Options.IsObjectDumpEnabled
                    || obj.GetType().GetMethod("ToString", BindingFlags.DeclaredOnly) != null)
                    // if no dump allowed or owned by type: ToString method is called
                    @out.Echo(obj.ToString(), options != null && options.LineBreak);
                else
                    EchoLowLevelObjectDump(obj, ctx);
            }
        }

        #endregion

        // 🚩 -------------------------------------------------------------------------------------------------

        #region colors types

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

            ShellObject.EchoObj(obj, ctx);
        }

        #endregion

        #region variables & collections objects

        private static void EchoLowLevelObjectDump(
            object obj,
            EchoEvaluationContext echoEvaluationContext)
        {
            var (@out, context, options) = echoEvaluationContext;
            //@out.Echo(obj.ToString(), options != null && options.LineBreak);
            ShellObject.EchoObj(obj, echoEvaluationContext);
        }

        public static void DumpObject(
            string name,
            object obj,
            CommandEvaluationContext ctx,
            EchoEvaluationContext echoEvaluationContext)
        {
            var (@out, context, opts) = echoEvaluationContext;
            if (context.EchoMap.MappedCall(obj, echoEvaluationContext)) return;

            var options = opts as TableFormattingOptions;
            options ??=
                context.ShellEnv.GetValue<TableFormattingOptions>(
                    ShellEnvironmentVar.display_tableFormattingOptions)
                .Clone();
            options = new TableFormattingOptions(options) { PadLastColumn = false };

            var dt = GetVarsDataTable(context, obj, new List<IDataObject>(), options);

            dt.AddNamePrefix("".PadLeft(TabLength));

            dt.InsertRow(
                0
                , name
                , obj.GetType().UnmangledName()
                , obj
                );

            dt.Echo(new EchoEvaluationContext(@out, context, options));
        }

        public static void Echo(
            this DataValue dataValue,
            EchoEvaluationContext ctx
            ) => Echo(dataValue?.Value, ctx);

        public static void Echo(
            this DataObject dataObject,
            EchoEvaluationContext ctx
            ) => Echo((IDataObject)dataObject, ctx);

        public static void Echo(
            this IDataObject dataObject,
            EchoEvaluationContext ctx)
        {
            var (@out, context, opts) = ctx;
            if (context.EchoMap.MappedCall(dataObject, ctx)) return;

            var options = opts as TableFormattingOptions;
            options ??= (TableFormattingOptions)
                context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.display_tableFormattingOptions)
                .InitFrom(opts);
            options = new TableFormattingOptions(options)
            {
                PadLastColumn = false
            };

            if (!options.IsObjectDumpEnabled)
            {
                @out.Echo(dataObject?.ToString());
                return;
            }

            var attrs = dataObject.GetAttributes();
            attrs.Sort((x, y) => x.Name.CompareTo(y.Name));

            object container = null;
            if (dataObject is DataValue dataValue
                    && !(dataValue.Value is IDataObject))
            {
                container = dataValue.Value;
            }

            var dt = GetVarsDataTable(context, container, attrs, options);
            dt.Echo(new EchoEvaluationContext(@out, context, options));
        }

        public static void Echo(
            this Variables variables,
            EchoEvaluationContext ctx)
        {
            var (@out, context, opts) = ctx;
            if (context.EchoMap.MappedCall(variables, ctx)) return;

            var options = opts as TableFormattingOptions;
            options ??= (TableFormattingOptions)context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.display_tableFormattingOptions)
                .InitFrom(opts);
            var values = variables.GetDataValues();
            values.Sort((x, y) => x.Name.CompareTo(y.Name));
            var dt = GetVarsDataTable(context, null, values, options);
            dt.Echo(new EchoEvaluationContext(@out, context, options));
        }

        #endregion

        #region tables

        public static void Echo(
            this DataTable table,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(table, ctx)) return;
            _Echo(table, @out, context, options as TableFormattingOptions);
        }

        public static void Echo(
            this Table table,
            EchoEvaluationContext ctx)
        {
            var (@out, context, options) = ctx;
            if (context.EchoMap.MappedCall(table, ctx)) return;
            _Echo(table, @out, context, options as TableFormattingOptions);
        }

        static void _Echo(
            this DataTable table,
            ConsoleTextWriterWrapper @out,
            CommandEvaluationContext context,
            TableFormattingOptions options = null)
        {
            options ??= context.ShellEnv.GetValue<TableFormattingOptions>(ShellEnvironmentVar.display_tableFormattingOptions);
            @out.EnableFillLineFromCursor = false;
            @out.HideCur();
            var colLengths = new int[table.Columns.Count];

            foreach (var rw in table.Rows)
            {
                var cols = ((DataRow)rw).ItemArray;
                for (int i = 0; i < cols.Length; i++)
                {
                    string s, s2;
                    if (table is Table t)
                    {
                        s = @out.GetPrint(t.GetFormatedValue(context, table.Columns[i].ColumnName, cols[i])) ?? "";
                        var length = s.Length;
                        colLengths[i] = Math.Max(length, colLengths[i]);
                        s2 = @out.GetPrint(t.GetFormatedHeader(table.Columns[i].ColumnName)) ?? "";
                        colLengths[i] = Math.Max(s2.Length, colLengths[i]);
                        if (i == cols.Length - 1) colLengths[i] = length + 2;
                    }
                    else
                    {
                        s = @out.GetPrint(cols[i]?.ToString()) ?? "";
                        colLengths[i] = Math.Max(s.Length, colLengths[i]);
                        colLengths[i] = Math.Max(table.Columns[i].ColumnName.Length, colLengths[i]);
                        if (i == cols.Length - 1) colLengths[i] = 1;
                        if (i == cols.Length - 1) colLengths[i] = s.Length + 2;
                    }
                }
            }
            var colsep = options.NoBorders ?
                " ".PadLeft(Math.Max(1, options.ColumnRightMargin))
                : (context.ShellEnv.Colors.TableBorder + " | " + context.ShellEnv.Colors.Default);


            var colseplength = options.NoBorders ? 0 : 3;
            var tablewidth = options.NoBorders ? 0 : 3;

            for (int i = 0; i < table.Columns.Count; i++)
                tablewidth += table.Columns[i].ColumnName.PadRight(colLengths[i], ' ').Length + colseplength;

            var line = options.NoBorders ? "" : (context.ShellEnv.Colors.TableBorder + "".PadRight(tablewidth, '-'));

            if (!options.NoBorders) @out.Echoln(line);
            string fxh(string header) => (table is Table t) ? t.GetFormatedHeader(header) : header;

            // headers

            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (!options.NoBorders && i == 0) @out.Echo(colsep);

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

            // rows

            string fhv(string header, object value) =>
                (table is Table t) ?
                    t.GetFormatedValue(context, header, value is DBNull ? null : value)
                    : DumpAsText(context, value is DBNull ? null : value, false);

            foreach (var rw in table.Rows)
            {
                if (context.CommandLineProcessor.CancellationTokenSource?.IsCancellationRequested is bool b && b)
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
                    if (!options.NoBorders && i == 0) @out.Echo(colsep);

                    var fvalue = fhv(table.Columns[i].ColumnName, arr[i]);
                    var o = arr[i];

                    MethodInfo mi = null;
                    if (((!(o is string)) || table.Columns[i].DataType == typeof(object))
                            && o != null && (mi = o.GetEchoMethod()) != null)
                    {
                        // value dump via Echo primitive
                        @out.Echo("" + context.ShellEnv.Colors.Default);
                        var p0 = @out.CursorPos;
                        mi.InvokeEcho(o,
                            new EchoEvaluationContext(
                                @out,
                                context,
                                // ⚠️⚠️ do not downcast options type when transferring to descendants ⚠️⚠️
                                new TableFormattingOptions(options)
                                {
                                    LineBreak = false
                                }));

                        var p1 = @out.CursorPos;
                        if (p1.Y == p0.Y)
                        {
                            var l = p1.X - p0.X;
                            var spc = (i == arr.Length - 1 && !options.PadLastColumn) ? "" : ("".PadRight(Math.Max(0, colLengths[i] - l), ' '));
                            @out.Echo(spc);
                        }
                        @out.Echo(colsep);
                    }
                    else
                    {
                        // value dump by ToString
                        var l = @out.GetPrint(fvalue).Length;
                        var spc = (i == arr.Length - 1 && !options.PadLastColumn) ? "" : ("".PadRight(Math.Max(0, colLengths[i] - l), ' '));
                        @out.Echo("" + context.ShellEnv.Colors.Default);

                        if (o is string)
                        {
                            Echo(fvalue,
                                new EchoEvaluationContext(
                                    @out,
                                    context,
                                    new TableFormattingOptions(options)
                                    {
                                        LineBreak = false,
                                        IsRawModeEnabled = false
                                    }));
                        }
                        else
                            Echo(o,
                                new EchoEvaluationContext(
                                    @out,
                                    context,
                                    new TableFormattingOptions(options)
                                    {
                                        LineBreak = false
                                    }));
                        @out.Echo(spc + colsep);
                    }
                }
                @out.Echoln();
            }
            @out.Echo(line + context.ShellEnv.Colors.Default.ToString());
            @out.ShowCur();
            @out.EnableFillLineFromCursor = true;
        }

        #endregion
    }
}
