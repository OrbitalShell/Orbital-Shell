using static DotNetConsoleAppToolkit.DotNetConsole;
using DotNetConsoleAppToolkit.Component.EchoDirective;
using System;

namespace DotNetConsoleAppToolkit.Component.EchoDirective
{
    /// <summary>
    /// shortcuts to echo directives commands
    /// <para>
    /// naming conventions:<br/>
    /// &#9889; echo directive commands shortcuts are CamelCase<br/>
    /// </para>
    /// </summary>
    public static class Shortcuts
{       public static string GetCmd(string cmd, string value = null)
        {
            if (value != null)
                return $"{CommandBlockBeginChar}{cmd}{CommandValueAssignationChar}{value}{CommandBlockEndChar}";
            return $"{CommandBlockBeginChar}{cmd}{CommandBlockEndChar}";
        }

        public static string GetCmd(EchoDirectives cmd, string value = null)
        {
            if (value != null)
                return $"{CommandBlockBeginChar}{cmd}{CommandValueAssignationChar}{value}{CommandBlockEndChar}";
            return $"{CommandBlockBeginChar}{cmd}{CommandBlockEndChar}";
        }

        #region commands shortcuts

        public static string Clleft => GetCmd(EchoDirectives.clleft);
        public static string Clright => GetCmd(EchoDirectives.clright);
        public static string Fillright => GetCmd(EchoDirectives.fillright);
        public static string Cl => GetCmd(EchoDirectives.cl);
        public static string Chome => GetCmd(EchoDirectives.chome);

        public static string Lion => GetCmd(EchoDirectives.lion);
        public static string Bon => GetCmd(EchoDirectives.bon);
        public static string Blon => GetCmd(EchoDirectives.blon);

        public static string Cleft => GetCmd(EchoDirectives.cleft);
        public static string Cright => GetCmd(EchoDirectives.cright);
        public static string Cup => GetCmd(EchoDirectives.cup);
        public static string Cdown => GetCmd(EchoDirectives.cdown);
        public static string Cnleft(int n) => GetCmd(EchoDirectives.cleft+"",n+"");
        public static string Cnright(int n) => GetCmd(EchoDirectives.cright + "", n + "");
        public static string Cnup(int n) => GetCmd(EchoDirectives.cup + "", n + "");
        public static string Cndown(int n) => GetCmd(EchoDirectives.cdown + "", n + "");

        public static string Invon => GetCmd(EchoDirectives.invon);
        public static string Uon => GetCmd(EchoDirectives.uon);
        public static string Tdoff => GetCmd(EchoDirectives.tdoff);
        
        public static string DefaultBackgroundCmd => GetCmd(EchoDirectives.b + "", DefaultBackground.ToString().ToLower());
        public static string DefaultForegroundCmd => GetCmd(EchoDirectives.f + "", DefaultForeground.ToString().ToLower());
        public static string Rdc => GetCmd(EchoDirectives.rdc);

        public static string Bblack => GetCmd(EchoDirectives.b+"", "black");
        public static string Bdarkblue => GetCmd(EchoDirectives.b , "darkblue");
        public static string Bdarkgreen => GetCmd(EchoDirectives.b , "darkgreen");
        public static string Bdarkcyan => GetCmd(EchoDirectives.b , "darkcyan");
        public static string Bdarkred => GetCmd(EchoDirectives.b , "darkred");
        public static string Bdarkmagenta => GetCmd(EchoDirectives.b , "darkmagenta");
        public static string Bdarkyellow => GetCmd(EchoDirectives.b , "darkyellow");
        public static string Bgray => GetCmd(EchoDirectives.b , "gray");
        public static string Bdarkgray => GetCmd(EchoDirectives.b , "darkgray");
        public static string Bblue => GetCmd(EchoDirectives.b , "blue");
        public static string Bgreen => GetCmd(EchoDirectives.b , "green");
        public static string Bcyan => GetCmd(EchoDirectives.b , "cyan");
        public static string Bred => GetCmd(EchoDirectives.b , "red");
        public static string Bmagenta => GetCmd(EchoDirectives.b , "magenta");
        public static string Byellow => GetCmd(EchoDirectives.b , "yellow");
        public static string Bwhite => GetCmd(EchoDirectives.b , "white");
        public static string Black => GetCmd(EchoDirectives.f , "black");
        public static string Darkblue => GetCmd(EchoDirectives.f , "darkblue");
        public static string Darkgreen => GetCmd(EchoDirectives.f , "darkgreen");
        public static string Darkcyan => GetCmd(EchoDirectives.f , "darkcyan");
        public static string Darkred => GetCmd(EchoDirectives.f , "darkred");
        public static string Darkmagenta => GetCmd(EchoDirectives.f , "darkmagenta");
        public static string Darkyellow => GetCmd(EchoDirectives.f , "darkyellow");
        public static string Gray => GetCmd(EchoDirectives.f , "gray");
        public static string Darkgray => GetCmd(EchoDirectives.f , "darkgray");
        public static string Blue => GetCmd(EchoDirectives.f , "blue");
        public static string Green => GetCmd(EchoDirectives.f , "green");
        public static string Cyan => GetCmd(EchoDirectives.f , "cyan");
        public static string Red => GetCmd(EchoDirectives.f , "red");
        public static string Magenta => GetCmd(EchoDirectives.f , "magenta");
        public static string Yellow => GetCmd(EchoDirectives.f , "yellow");
        public static string White => GetCmd(EchoDirectives.f , "white");

        public static string Bkf => GetCmd(EchoDirectives.bkf );
        public static string Rsf => GetCmd(EchoDirectives.rsf );
        public static string Bkb => GetCmd(EchoDirectives.bkb );
        public static string Rsb => GetCmd(EchoDirectives.rsb );
        public static string Cls => GetCmd(EchoDirectives.cls );
        public static string Br => GetCmd(EchoDirectives.br );

        public static string B(ConsoleColor c) => GetCmd(EchoDirectives.b , c+"");
        public static string B8(ConsoleColor c) => GetCmd(EchoDirectives.b8 , c+"");
        public static string B24(ConsoleColor c) => GetCmd(EchoDirectives.b24 , c+"");

        public static string F(ConsoleColor c) => GetCmd(EchoDirectives.f , c+"");
        public static string F8(ConsoleColor c) => GetCmd(EchoDirectives.f8 , c+"");
        public static string F24(ConsoleColor c) => GetCmd(EchoDirectives.f24 , c+"");

        public static string Bkcr => GetCmd(EchoDirectives.bkcr );
        public static string Rscr => GetCmd(EchoDirectives.rscr );
        public static string Crx(int x) => GetCmd(EchoDirectives.crx , x +"");
        public static string Cry(int y) => GetCmd(EchoDirectives.cry , y +"");
        public static string Cr(int x, int y) => $"{GetCmd(EchoDirectives.crx , x +"" )}{GetCmd(EchoDirectives.cry , y+"" )}";

        public static string Exec(string csharpText) => GetCmd(EchoDirectives.exec , csharpText);

        public static string Tab => "".PadLeft(TabLength, ' ');

        #endregion
    }
}