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

        public bool NoBorders = true;
        public bool PadLastColumn = true;
        public TableLayout Layout = TableLayout.HeaderHorizontalSeparator;

        public TableFormattingOptions() { }

        public TableFormattingOptions(TableFormattingOptions o)
        {
            NoBorders = o.NoBorders;
            PadLastColumn = o.PadLastColumn;
            Layout = o.Layout;
        }

        public TableFormattingOptions(
            bool noBorders = true, 
            bool padLastColumn = true, 
            TableLayout layout = TableLayout.HeaderHorizontalSeparator)
        {
            NoBorders = noBorders;
            PadLastColumn = padLastColumn;
            Layout = layout;
        }
    }
}
