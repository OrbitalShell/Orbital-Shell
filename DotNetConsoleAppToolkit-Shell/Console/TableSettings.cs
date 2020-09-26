using System;
using System.Collections.Generic;
using System.Text;

namespace DotNetConsoleAppToolkit.Console
{
    public class TableSettings
    {
        public enum TableLayout
        {
            NoBorders,
            HeaderHorizontalSeparator,
            HeaderHorizontalSeparatorVerticalSeparators
        }

        public TableLayout Layout = TableLayout.HeaderHorizontalSeparator;
    }
}
