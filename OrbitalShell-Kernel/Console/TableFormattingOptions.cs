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
        public bool UnfoldCategories = true;
        public bool UnfoldItems = true;

        public TableFormattingOptions() { }

        public TableFormattingOptions(TableFormattingOptions o)
        {
            NoBorders = o.NoBorders;
            PadLastColumn = o.PadLastColumn;
            Layout = o.Layout;
            UnfoldCategories = o.UnfoldCategories;
            UnfoldItems = o.UnfoldItems;
        }

        public TableFormattingOptions(
            bool noBorders = true, 
            bool padLastColumn = true, 
            TableLayout layout = TableLayout.HeaderHorizontalSeparator,
            bool unfoldCategories = true,
            bool unfoldItems = true)
        {
            NoBorders = noBorders;
            PadLastColumn = padLastColumn;
            Layout = layout;
            UnfoldCategories = unfoldCategories;
            UnfoldItems = unfoldItems;
        }
    }
}
