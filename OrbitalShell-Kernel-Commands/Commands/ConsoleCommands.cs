using OrbitalShell.Component.CommandLine.CommandModel;
using OrbitalShell.Component.CommandLine.Processor;
using OrbitalShell.Component.CommandLine.Variable;
using OrbitalShell.Console;
using OrbitalShell.Lib;
using System;
using System.Collections.Generic;
using System.Text;
using cons = OrbitalShell.DotNetConsole;
using static OrbitalShell.Component.EchoDirective.Shortcuts;
using OrbitalShell.Component.EchoDirective;

namespace OrbitalShell.Component.Commands
{
    [Commands("commands of the console")]
    public class ConsoleCommands : ICommandsDeclaringType
    {
        const string _printDocText =
@"(rdc)text can contains (uon)echo directives(uoff) that changes the echo behavior. 
the echo directive syntax is formed according to these pattern:

(f=darkyellow)(printDirective) or (printDirective=printDirectiveValue)(rdc)

- multiple echo directives can be separated by a (f=darkyellow),(rdc) to be grouped in a single text in parentheses: (f=darkyellow)(echoDirective1,echoDirective2=..,echoDirective3)(rdc)
- an echo directive value can be written inside a 'code' text block, depending on each echo directive, with the syntax: (f=darkyellow)[[...]](rdc)
- symbols of this grammar can be configured throught the class (uon)DotNetConsole(tdoff)

current print directives are:

    (1) (uon)colorization:(tdoff)

    (f=yellow)f=(f=darkyellow)ConsoleColor(rdc)      : set foreground color
    (f=yellow)f8=(f=darkyellow)Int32(rdc)            : set foreground 8bit color index, where 0 <= index <= 255 
    (f=yellow)f24=(f=darkyellow)Int32:Int32:Int32(rdc) : set foreground 24bit color red:green:blue, where 0 <= red,green,blue <= 255 
    (f=yellow)f=(f=darkyellow)ConsoleColor(rdc)      : set foreground color
    (f=yellow)b=(f=darkyellow)ConsoleColor(rdc)      : set background color
    (f=yellow)b8=(f=darkyellow)Int32(rdc)            : set background 8bit color index, where 0 <= index <= 255
    (f=yellow)b24=(f=darkyellow)Int32:Int32:Int32(rdc) : set background 24bit color red:green:blue, where 0 <= red,green,blue <= 255 
    (f=yellow)df=(f=darkyellow)ConsoleColor(rdc)     : set default foreground
    (f=yellow)db=(f=darkyellow)ConsoleColor(rdc)     : set default background
    (f=yellow)bkf(rdc)                 : backup foreground color
    (f=yellow)bkb(rdc)                 : backup background color
    (f=yellow)rsf(rdc)                 : restore foreground color
    (f=yellow)rsb(rdc)                 : restore background color
    (f=yellow)rdc(rdc)                 : restore default colors
    
    (2) (uon)text decoration (vt100):(tdoff)

    (f=yellow)uon(rdc)                 : underline on
    (f=yellow)invon(rdc)               : inverted colors on
    (f=yellow)tdoff(rdc)               : text decoration off and reset default colors

    (3) (uon)echo flow control:(tdoff)

    (f=yellow)cls(rdc)                 : clear screen
    (f=yellow)br(rdc)                  : jump begin of next line (line break)   
    (f=yellow)bkcr(rdc)                : backup cursor position
    (f=yellow)rscr(rdc)                : restore cursor position
    (f=yellow)crx=(f=darkyellow)Int32(rdc)           : set cursor x ((f=cyan)0<=x<=WindowWidth(rdc))
    (f=yellow)cry=(f=darkyellow)Int32(rdc)           : set cursor y ((f=cyan)0<=y<=WindowHeight(rdc))
    (f=yellow)cleft(rdc)               : move cursor left
    (f=yellow)cright(rdc)              : move cursor right
    (f=yellow)cup(rdc)                 : move cursor up
    (f=yellow)cdown(rdc)               : move cursor down
    (f=yellow)cnleft=(f=darkyellow)Int32(rdc)        : move cursor n characters left
    (f=yellow)cnright=(f=darkyellow)Int32(rdc)       : move cursor n characters right
    (f=yellow)cnup=(f=darkyellow)Int32(rdc)          : move cursor n lines up
    (f=yellow)cndown=(f=darkyellow)Int32(rdc)        : move cursor n lines down
    (f=yellow)cl(rdc)                  : clear line
    (f=yellow)clleft(rdc)              : clear line from cursor left
    (f=yellow)clright(rdc)             : clear line from cursor right
    (f=yellow)chome(rdc)               : move cursor to upper left corner

    (4) (uon)script engine:(tdoff)

    (f=yellow)exec=(f=darkyellow)CodeBlock|[[CodeBlock]](rdc) : executes and echo result of a C# code block

    (5) (uon)application control:(tdoff)

    (f=yellow)exit(rdc)                : exit the current process

    (f=darkyellow)ConsoleColor := darkblue|darkgreen|darkcyan|darkred|darkmagenta|darkyellow|gray|darkgray|blue|green|cyan|red|magenta|yellow|white(rdc) (not case sensitive)
";

        [Command("write text to the output stream followed by a line break", null, _printDocText)]
        public CommandVoidResult Echo(
            CommandEvaluationContext context,
            [Parameter("text or other (value of type object) to be writen to output", true, "")] object obj,
            [Option("n","no line break: do not add a line break after output")] bool avoidLineBreak = false,
            [Option("r","raw mode - echo directives and ansi sequences are replaces by readable text")] bool raw = false
            )
        {
            lock (context.Out.Lock)
            {
                /* // sample: capture the echo output
                context.Out.EchoOn();
                context.Out.Echo(obj,!avoidLineBreak);
                var str = context.Out.EchoOff();
                return new CommandResult<string>(str);*/

                if (obj != null)
                {
                    if (obj is string s)
                        context.Out.Echo(s, !avoidLineBreak, raw);
                    else
                    {
                        obj.Echo(
                            new EchoEvaluationContext(
                                context.Out, 
                                context, 
                                new FormattingOptions(!avoidLineBreak,raw)
                            ));
                        if (!avoidLineBreak) context.Out.Echo("",true);
                    }
                }
                return CommandVoidResult.Instance;
            }
        }

        [Command("get/set console output encoding")]
        public CommandResult<Encoding> Encoding(
            CommandEvaluationContext context,
            [Parameter(0,"encoding name",true)] string encodingName
            )
        {
            var setEncoding = !string.IsNullOrWhiteSpace(encodingName);
            var e = System.Console.OutputEncoding;
            Encoding ret = e;

            var @out = context.Out;
            void echokv(string name, object value)
            {
                new KeyValuePair<string, object>(name, value).Echo(new EchoEvaluationContext(@out,context));
            };

            if (setEncoding)
            {
                try
                {
                    var ne = System.Text.Encoding.GetEncoding(encodingName);
                    System.Console.OutputEncoding = ne;
                    ret = ne;
                } catch (ArgumentException)
                {
                    context.Errorln($"encoding not found: '{encodingName}'");
                    setEncoding = true;
                }
            } else
            {
                echokv("name", e.EncodingName);
                echokv(" code page", e.CodePage);
            }

            if (!setEncoding)
            {
                @out.Echoln();
                @out.Echoln($"{Br}{Uon}available encodings are:{Tdoff}{Br}");

                var lst = new List<object>(System.Text.Encoding.GetEncodings());
                foreach (var o in lst )
                {
                    if (o is EncodingInfo encoding)
                    {
                        echokv("name", encoding.Name);
                        echokv(" code page", encoding.CodePage);
                        @out.Echoln();
                    }
                }
            }
            return new CommandResult<Encoding>(ret);
        }

        [Command("outputs the table of characters")]
        public CommandVoidResult CharTable(
            CommandEvaluationContext context,
            [Parameter( 0, "start char index",true, 32)] int startIndex,
            [OptionRequireParameter("startIndex")]
            [Parameter( 1, "end char index",true,255)] int endIndex,
            [Option("f","turns output to a flat list instead of a table view")] bool flatList
            )
        {            
            int nbCols = 8;
            int col = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                if (!context.CommandLineProcessor.CancellationTokenSource.IsCancellationRequested)
                {
                    var c = Convert.ToChar(i);  
                    if (!flatList)
                        context.Out.Echo($"{context.ShellEnv.Colors.Numeric}{i,4}{Rdc}   {c,-2} {context.ShellEnv.Colors.Symbol}| {Rdc}");
                    else
                        context.Out.Echo($"{context.ShellEnv.Colors.Numeric}{i,-8}{Rdc}{c,-2}", true);
                    if ((col++) > nbCols)
                    {
                        col = 0;
                        var ln = context.Out.CursorLeft;
                        if (!flatList)
                        {
                            context.Out.Echo("", true);
                            context.Out.Echo(context.ShellEnv.Colors.Symbol + "".PadLeft(ln, '-'), true);
                        }
                    }
                }
            }
            context.Out.Echo("", true);
            return CommandVoidResult.Instance;
        }

        [Command("clear console screen")]
        public CommandVoidResult Cls(CommandEvaluationContext context)
        {
            context.Out.ClearScreen();            
            if (context.ShellEnv.IsOptionSetted(ShellEnvironmentVar.settings_console_enableCompatibilityMode) )
                context.CommandLineProcessor.Eval(context,"enableconsolecompatibilitymode",0);
            return CommandVoidResult.Instance;
        }

        [Command("hide cursor")]
        public CommandVoidResult HideCursor(CommandEvaluationContext context)
        {
            context.Out.HideCur();
            return CommandVoidResult.Instance;
        }

        [Command("show cursor")]
        public CommandVoidResult ShowCursor(CommandEvaluationContext context)
        {
            context.Out.ShowCur();
            return CommandVoidResult.Instance;
        }
    }
}
