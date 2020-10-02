using DotNetConsoleAppToolkit.Component.CommandLine;
using DotNetConsoleAppToolkit.Component.CommandLine.CommandModel;
using DotNetConsoleAppToolkit.Component.CommandLine.Processor;
using DotNetConsoleAppToolkit.Console;
using DotNetConsoleAppToolkit.Lib;
using System.ComponentModel;
using System.Reflection;
using static DotNetConsoleAppToolkit.DotNetConsole;

namespace DotNetConsoleAppToolkit.Shell.Commands
{
    [Commands("commands of the console")]
    public class ConsoleCommands : ICommandsDeclaringType
    {
        const string _printDocText =
@"text can contains (uon)echo directives(tdoff) that changes the echo behavior. 
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
            [Option("n","no line break: do not add a line break after output")] bool avoidLineBreak = false
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
                        context.Out.Echo(s, !avoidLineBreak);
                    else
                    {
                        obj.Echo(context.Out, context, null);
                        if (!avoidLineBreak) context.Out.Echo("",true);
                    }
                }
                return CommandVoidResult.Instance;
            }
        }

        [Command("outputs the table of characters")]
        public CommandVoidResult CharTable(
            CommandEvaluationContext context,
            [Parameter( 0, "start char index",true, 32)] int startIndex,
            [OptionRequireParameter("startIndex")]
            [Parameter( 1, "end char index",true,255)] int endIndex
            )
        {
            int nbCols = 8;
            int col = 0;
            for (int i = startIndex; i <= endIndex; i++)
            {
                context.Out.Echo($"{context.ShellEnv.Colors.Numeric}{i,4}{Rdc}   {(char)i,-2} {context.ShellEnv.Colors.Symbol}| {Rdc}");
                if ((col++) > nbCols)
                {
                    col = 0;
                    var ln = context.Out.CursorLeft;
                    context.Out.Echo("", true);
                    context.Out.Echo(context.ShellEnv.Colors.Symbol + "".PadLeft(ln, '-'), true);
                }
            }
            context.Out.Echo("", true);
            return CommandVoidResult.Instance;
        }

        [Command("clear console screen")]
        public CommandVoidResult Cls(CommandEvaluationContext context)
        {
            context.Out.ClearScreen();
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
