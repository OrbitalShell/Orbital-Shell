using OrbitalShell.Lib.Sys;

namespace OrbitalShell.Component.Console.Formats
{
    public class TableFormattingOptions
        : FormatingOptions,
        IClonable<TableFormattingOptions>
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
        public bool UnfoldSubCategories = true;

        public TableFormattingOptions() { }

        public TableFormattingOptions(FormatingOptions o) => InitFrom(o);

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
            UnfoldSubCategories = o.UnfoldSubCategories;
        }

        public TableFormattingOptions(
            bool noBorders = true,
            bool padLastColumn = true,
            TableLayout layout = TableLayout.HeaderHorizontalSeparator,
            bool unfoldCategories = true,
            bool unfoldItems = true,
            int columnLeftMargin = 0,
            int columnRightMargin = 4,
            bool unfoldSubCategories = true)
        {
            NoBorders = noBorders;
            PadLastColumn = padLastColumn;
            Layout = layout;
            UnfoldCategories = unfoldCategories;
            UnfoldItems = unfoldItems;
            ColumnLeftMargin = columnLeftMargin;
            ColumnRightMargin = columnRightMargin;
            UnfoldSubCategories = unfoldSubCategories;
        }

        public override TableFormattingOptions Clone()
            => new(this);

    }
}
