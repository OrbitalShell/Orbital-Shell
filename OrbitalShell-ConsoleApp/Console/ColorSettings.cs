using System;
using static OrbitalShell.DotNetConsole;

namespace OrbitalShell.Console
{
    public class ColorSettings
    {
        /// <summary>
        /// defaults shell foreground and background - if is setted. designed to preserve console default background transparency
        /// </summary>
        /// <returns></returns>
        public TextColor Default => new TextColor(DefaultForeground, DefaultBackground, ANSI.RSTXTA);

        public TextColor Inverted => new TextColor(DefaultBackground, DefaultForeground);

        // states colors

        public TextColor Log = new TextColor(ConsoleColor.Green, null);
        public TextColor Error = new TextColor(ConsoleColor.Red, null);
        public TextColor Success = new TextColor(ConsoleColor.Green, null);
        public TextColor Warning = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor Debug = new TextColor(ConsoleColor.Green, null);

        // states as text in a box

        public TextColor BoxOk = new TextColor(ConsoleColor.White, ConsoleColor.DarkGreen);
        public TextColor BoxError => new TextColor(ConsoleColor.Yellow, ConsoleColor.Red);
        public TextColor BoxUnknown = new TextColor(ConsoleColor.Green, ConsoleColor.DarkCyan);
        public TextColor BoxNotIdentified = new TextColor(ConsoleColor.Yellow, ConsoleColor.Red);
        public TextColor Information = new TextColor(ConsoleColor.DarkCyan, null);

        // DotNetConsoleAppToolkit-UI

        public TextColor TitleBar = new TextColor(ConsoleColor.White, ConsoleColor.DarkBlue);
        public TextColor TitleDarkText = new TextColor(ConsoleColor.Gray, ConsoleColor.DarkBlue);
        public TextColor InteractionBar = new TextColor(ConsoleColor.White, ConsoleColor.DarkBlue);
        public TextColor InteractionPanel = new TextColor(ConsoleColor.White, ConsoleColor.DarkBlue);
        public TextColor InteractionPanelCmdKeys = new TextColor(ConsoleColor.Black, ConsoleColor.White);
        public TextColor InteractionPanelDisabledCmdKeys = new TextColor(ConsoleColor.DarkGray, ConsoleColor.Black);

        public TextColor InteractionPanelCmdLabel = new TextColor(ConsoleColor.Yellow, ConsoleColor.DarkBlue);
        public TextColor InteractionPanelDisabledCmdLabel = new TextColor(ConsoleColor.DarkGray, ConsoleColor.DarkBlue);

        // system library types

        public TextColor ExceptionText = new TextColor(ConsoleColor.Red, null);
        public TextColor ExceptionName = new TextColor(ConsoleColor.Yellow, ConsoleColor.Red);

        // DotNetConsoleAppToolkit-Shell

        //      values types

        public TextColor Null = new TextColor(ConsoleColor.Green, null);
        public TextColor Quotes = new TextColor(ConsoleColor.Green, null);
        public TextColor Numeric = new TextColor(ConsoleColor.Cyan, null);
        public TextColor Boolean = new TextColor(ConsoleColor.Magenta, null);
        public TextColor BooleanTrue = new TextColor(ConsoleColor.Magenta, null);
        public TextColor BooleanFalse = new TextColor(ConsoleColor.DarkMagenta, null);
        public TextColor Integer = new TextColor(ConsoleColor.Cyan, null);
        public TextColor Double = new TextColor(ConsoleColor.Cyan, null);
        public TextColor Float = new TextColor(ConsoleColor.Cyan, null);
        public TextColor Decimal = new TextColor(ConsoleColor.Cyan, null);
        public TextColor Char = new TextColor(ConsoleColor.Green, null);

        public TextColor Label = new TextColor(ConsoleColor.Cyan, null);
        public TextColor Name = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor HighlightSymbol = new TextColor(ConsoleColor.Yellow, null);
        public TextColor Symbol = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor Value = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor HalfDarkLabel = new TextColor(ConsoleColor.DarkCyan, null);
        public TextColor DarkLabel = new TextColor(ConsoleColor.DarkBlue, null);
        public TextColor MediumDarkLabel = new TextColor(ConsoleColor.Blue, null);
        public TextColor HighlightIdentifier = new TextColor(ConsoleColor.Green, null);

        //      colors by effect

        public TextColor Highlight = new TextColor(ConsoleColor.Yellow, null);
        public TextColor HalfDark = new TextColor(ConsoleColor.Gray, null);
        public TextColor Dark = new TextColor(ConsoleColor.DarkGray, null);

        //      global ? 

        public TextColor TableBorder = new TextColor(ConsoleColor.Cyan, null);
        public TextColor TableColumnName = new TextColor(ConsoleColor.Yellow, null);

        //      syntax

        public TextColor ParameterName = new TextColor(ConsoleColor.Yellow, null);
        public TextColor KeyWord = new TextColor(ConsoleColor.Yellow, null);
        public TextColor ParameterValueType = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor OptionPrefix = new TextColor(ConsoleColor.Yellow, null);
        public TextColor OptionName = new TextColor(ConsoleColor.Yellow, null);
        public TextColor TypeName = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor OptionValue = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor SyntaxSymbol = new TextColor(ConsoleColor.Cyan, null);
    }
}
