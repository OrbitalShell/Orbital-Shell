using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data;
using static OrbitalShell.Component.Console.EchoPrimitives;
using OrbitalShell.Component.CommandLine.Processor;
using System;

namespace OrbitalShell.Component.Shell.Data
{
    public class Table : DataTable
    {
        #region init        

        public Table() : base() { }
        public Table(string tableName) : base(tableName) { }
        public Table(string tableName, string tableNamespace) : base(tableName, tableNamespace) { }
        public Table(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context) { }

        public Table(params string[] columnNames) : base() => AddColumns(columnNames);
        public Table(params (string name, Type type)[] columns) : base() => AddColumns(columns);
        public Table(params (string name, Type type, string format)[] columns) : base() => AddColumns(columns);

        #endregion

        #region attributs

        public Dictionary<string, string> ColumnsHeadersTextFormats = new Dictionary<string, string>();

        public Dictionary<string, string> ColumnsTextFormats = new Dictionary<string, string>();

        #endregion

        #region fluent modifiers

        public Table AddRow(params object[] objects)
        {
            var tr = base.NewRow();
            int i = 0;
            foreach (DataColumn col in Columns)
                tr[col.ColumnName] = objects[i++];
            this.Rows.Add(tr);
            return this;
        }

        public Table AddColumns(params string[] columnNames)
        {
            foreach (var colName in columnNames)
            {
                Columns.Add(colName);
            }
            return this;
        }

        public Table AddColumns(params (string name, Type type)[] columns)
        {
            foreach (var col in columns)
            {
                Columns.Add(col.name, col.type);
            }
            return this;
        }

        public Table AddColumns(params (string name, Type type, string format)[] columns)
        {
            foreach (var col in columns)
            {
                Columns.Add(col.name, col.type);
                ColumnsTextFormats[col.name] = col.format;
            }
            return this;
        }

        public Table AddColumns(params (string name, Type type, string format, string headerFormat)[] columns)
        {
            foreach (var col in columns)
            {
                Columns.Add(col.name, col.type);
                ColumnsTextFormats[col.name] = col.format;
                ColumnsHeadersTextFormats[col.name] = col.headerFormat;
            }
            return this;
        }

        #endregion

        #region fluent setters

        public Table SetFormat(string columnName, string format)
        {
            ColumnsTextFormats[columnName] = format;
            return this;
        }

        public Table SetHeaderFormat(string columnName, string format)
        {
            ColumnsHeadersTextFormats[columnName] = format;
            return this;
        }

        #endregion

        #region getters

        public string GetFormatedValue(CommandEvaluationContext context, string columnName, object value)
        {
            if (ColumnsTextFormats.TryGetValue(columnName,out var textFormat))
                return string.Format(textFormat, DumpAsText(context, value, false));
            else
                return DumpAsText(context, value, false);
        }

        public string GetFormatedHeader(string columnName)
        {
            if (ColumnsHeadersTextFormats.TryGetValue(columnName, out var textFormat))
                return string.Format(textFormat, columnName);
            else
                return columnName;                   
        }

        #endregion
    }
}
