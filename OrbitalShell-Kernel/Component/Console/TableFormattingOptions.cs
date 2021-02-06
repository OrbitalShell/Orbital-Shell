namespace OrbitalShell.Component.Console
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
        public int ColumnLeftMargin = 0;
        public int ColumnRightMargin = 4;

        public TableFormattingOptions() { }

        public TableFormattingOptions(TableFormattingOptions o)
        {
            base.InitFrom(o);
            NoBorders = o.NoBorders;
            PadLastColumn = o.PadLastColumn;
            Layout = o.Layout;
            UnfoldCategories = o.UnfoldCategories;
            UnfoldItems = o.UnfoldItems;
            ColumnLeftMargin = o.ColumnLeftMargin;
            ColumnRightMargin = o.ColumnRightMargin;
        }

        public TableFormattingOptions(
            bool noBorders = true,
            bool padLastColumn = true,
            TableLayout layout = TableLayout.HeaderHorizontalSeparator,
            bool unfoldCategories = true,
            bool unfoldItems = true,
            int columnLeftMargin = 0,
            int columnRightMargin = 4)
        {
            NoBorders = noBorders;
            PadLastColumn = padLastColumn;
            Layout = layout;
            UnfoldCategories = unfoldCategories;
            UnfoldItems = unfoldItems;
            ColumnLeftMargin = columnLeftMargin;
            ColumnRightMargin = columnRightMargin;
        }
    }
}
