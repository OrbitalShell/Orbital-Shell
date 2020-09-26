namespace DotNetConsoleAppToolkit.Console
{
    public class TableFormattingOptions : IFormattingOptions
    {
        public enum TableLayout
        {
            NoBorders,
            HeaderHorizontalSeparator,
            HeaderHorizontalSeparatorVerticalSeparators
        }

        public TableLayout Layout = TableLayout.HeaderHorizontalSeparator;

        public TableFormattingOptions() { }
    }
}
