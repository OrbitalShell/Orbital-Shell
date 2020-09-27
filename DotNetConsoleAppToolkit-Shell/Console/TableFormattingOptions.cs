namespace DotNetConsoleAppToolkit.Console
{
    public class TableFormattingOptions : FormattingOptions
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
        public bool UnfoldAll = false;

        public TableFormattingOptions() { }

        public TableFormattingOptions(TableFormattingOptions o)
        {
            NoBorders = o.NoBorders;
            PadLastColumn = o.PadLastColumn;
            Layout = o.Layout;
            UnfoldAll = o.UnfoldAll;
        }

        public TableFormattingOptions(
            bool noBorders = true, 
            bool padLastColumn = true, 
            TableLayout layout = TableLayout.HeaderHorizontalSeparator,
            bool unfoldAll = false)
        {
            NoBorders = noBorders;
            PadLastColumn = padLastColumn;
            Layout = layout;
            UnfoldAll = unfoldAll;
        }

        public override string ToString()
        {
            return $"{{NoBorders={NoBorders},layout={Layout},UnfoldAll={UnfoldAll},PadLastColumn={PadLastColumn},LineBreak={LineBreak}}}";
        }
    }
}
