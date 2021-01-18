using System;
using static DotNetConsoleAppToolkit.DotNetConsole;

namespace DotNetConsoleAppToolkit.Console
{
    public class ColorSettings 
    {        
        public TextColor Default => new TextColor(DefaultForeground, DefaultBackground,ANSI.RSTXTA);
        public TextColor Inverted => new TextColor(DefaultBackground, DefaultForeground);

        public TextColor Log = new TextColor(ConsoleColor.Green, null);
        public TextColor Error = new TextColor(ConsoleColor.Red, null);
        public TextColor Success = new TextColor(ConsoleColor.Green, null);
        public TextColor Warning = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor Debug = new TextColor(ConsoleColor.Green, null);

        // DotNetConsoleAppToolkit-UI

        public TextColor TitleBar = new TextColor(ConsoleColor.White, ConsoleColor.DarkBlue);
        public TextColor TitleDarkText = new TextColor(ConsoleColor.Gray,ConsoleColor.DarkBlue);
        public TextColor InteractionBar = new TextColor(ConsoleColor.White, ConsoleColor.DarkGreen);

        // DotNetConsoleAppToolkit-Shell

        //      values types
        public TextColor Numeric = new TextColor(ConsoleColor.Cyan, null);
        public TextColor Boolean = new TextColor(ConsoleColor.Gray, null);
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
        public TextColor TableBorder = new TextColor(ConsoleColor.Cyan,null);
        public TextColor TableColumnName = new TextColor(ConsoleColor.Yellow,null);
        //      syntax
        public TextColor ParameterName = new TextColor(ConsoleColor.Yellow, null);
        public TextColor KeyWord = new TextColor(ConsoleColor.Yellow, null);
        public TextColor ParameterValueType = new TextColor(ConsoleColor.DarkYellow, null);
        public TextColor OptionPrefix = new TextColor(ConsoleColor.Yellow, null);
        public TextColor OptionName = new TextColor(ConsoleColor.Yellow, null);
        public TextColor SyntaxSymbol = new TextColor(ConsoleColor.Cyan, null);
    }
}
