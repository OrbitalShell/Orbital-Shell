using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Data;
using static OrbitalShell.Component.Console.EchoPrimitives;
using OrbitalShell.Component.CommandLine.Processor;

namespace OrbitalShell.Component.Shell.Data
{
    public class Table : DataTable
    {
        public Table() : base() { }
        public Table(string tableName) : base(tableName) { }
        public Table(string tableName, string tableNamespace) : base(tableName, tableNamespace) { }
        public Table(SerializationInfo serializationInfo, StreamingContext context) : base(serializationInfo, context) { }

        Dictionary<string, string> _columnHeadersTextFormats;
        public IReadOnlyDictionary<string, string> ColumnsHeadersTextFormats
        {
            get
            {
                if (_columnHeadersTextFormats == null)
                {
                    _columnHeadersTextFormats = new Dictionary<string, string>();
                    foreach (DataColumn column in Columns)
                        _columnHeadersTextFormats.Add(column.ColumnName, null);
                }
                return _columnHeadersTextFormats;
            }
        }

        Dictionary<string, string> _columnsTextFormats;
        public IReadOnlyDictionary<string, string> ColumnsTextFormats
        {
            get
            {
                if (_columnsTextFormats == null)
                {
                    _columnsTextFormats = new Dictionary<string, string>();
                    foreach (DataColumn column in Columns)
                        _columnsTextFormats.Add(column.ColumnName, null);
                }
                return _columnsTextFormats;
            }
        }

        public void SetFormat(string columnName, string format)
        {
            _ = ColumnsTextFormats;
            _columnsTextFormats[columnName] = format;
        }

        public void SetHeaderFormat(string columnName, string format)
        {
            _ = ColumnsHeadersTextFormats;
            _columnHeadersTextFormats[columnName] = format;
        }

        public string GetFormatedValue(CommandEvaluationContext context, string columnName, object value)
        {
            var textFormat = ColumnsTextFormats[columnName];
            return
                (textFormat == null) ?
                    DumpAsText(context, value, false)
                    : string.Format(textFormat, DumpAsText(context, value, false));
        }

        public string GetFormatedHeader(string columnName)
        {
            var textFormat = ColumnsHeadersTextFormats[columnName];
            return
                (textFormat == null) ?
                    columnName
                    : string.Format(textFormat, columnName);
        }
    }
}
