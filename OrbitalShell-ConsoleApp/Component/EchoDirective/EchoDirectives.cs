namespace OrbitalShell.Component.EchoDirective
{
    /// <summary>
    /// <para>
    /// naming conventions:<br/>
    /// &#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;<br/>
    /// &#9889; echo directives names are in lower case<br/>
    /// &#9889; excepted ANSI sequences directives names that are in upper case<br/>
    /// &#9889; excepted Unicode characters names that are in camel case<br/>
    /// </para>
    /// <para>
    /// &#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;<br/>
    /// &#128309; constants by category:<br/>
    /// &#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;<br/>
    /// &#128313; Specials caracters:<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473; <br/>
    /// 
    ///     ESC<br/>
    ///     CRLF<br/>
    ///     CSI<br/>
    /// 
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// &#128313; ANSI sequences<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// 
    /// DECSC<br/>
    /// DECRC<br/>
    /// RIS<br/>
    /// RSTXTA<br/>
    /// RSCOLDEF<br/>
    /// CUU<br/>
    /// CUD<br/>
    /// CUF<br/>
    /// CUB<br/>
    /// CNL<br/>
    /// CPL<br/>
    /// CHA<br/>
    /// CUP<br/>
    /// SU<br/>
    /// SD<br/>
    /// DSR<br/>
    /// ED<br/>
    /// EL<br/>
    /// SGR_Reset<br/>
    /// SGR_IncreasedIntensity<br/>
    /// SGR_DecreaseIntensity<br/>
    /// SGR_Italic<br/>
    /// SGR_Underline<br/>
    /// SGR_SlowBlink<br/>
    /// SGR_RapidBlink<br/>
    /// SGR_ReverseVideo<br/>
    /// SGR_ItalicOff<br/>
    /// SGR_UnderlineOff<br/>
    /// SGR_BlinkOff<br/>
    /// SGR_ReverseOff<br/>
    /// SGR_NotCrossedOut<br/>
    /// SGR_CrossedOut<br/>
    /// SGR_DoubleUnderline<br/>
    /// SGR_NormalIntensity<br/>
    /// SGRF<br/>
    /// SGRF8<br/>
    /// SGRF24<br/>
    /// SGRB<br/>
    /// SGRB8<br/>
    /// SGRB24<br/>
    /// 
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// &#128313; colors:<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// 
    ///       f=consoleColor:   set foreground<br/>
    ///       b=consoleColor:   set background<br/>
    ///       df=consoleColor:  set default foreground<br/>
    ///       db=consoleColor:  set default background<br/>
    ///       bkf:              backup foreground<br/>
    ///       bkb:              backup background<br/>
    ///       rsf:              restore foreground<br/>
    ///       rsb:              restore background<br/>
    ///       rdc:              set colors to defaults<br/>
    ///       consoleColor (case insensitive) := black | darkblue | darkgreen | darkcyan | darkred  | darkmagenta | darkyellow | gray | darkgray  | blue | green | cyan  | red  | magenta  | yellow  | white<br/>
    ///
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// &#128313; print flow control:<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    ///
    ///       cl:       clear console<br/>
    ///       br:       line break<br/>
    ///       bkcr:     backup cursor pos<br/>
    ///       rscr:     restore cursor pos<br/>
    ///       crx=:     set cursor left<br/>
    ///       cry=:     set cursor top<br/>
    /// 
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// &#128313; app control:<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// 
    ///       exit: exit<br/>
    /// 
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// &#128313; scripts engines:<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// 
    ///       exec: exec csharp from text<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// &#128313; text decoration (vt100):<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// 
    ///       uon:      underline on<br/>
    ///       invon:    inverted text on<br/>
    ///       tdoff:    text decoration off<br/>
    ///       lion:     low intensity<br/>
    ///       blon:     blink on<br/>
    ///       bon:      bold on<br/>
    /// 
    /// </para>
    /// <para>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>
    /// &#128313; Unicode characters:<br/>
    /// &#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;&#9473;<br/>    
    /// Lire<br/>
    /// Yen<br/>
    /// None<br/>
    /// ARet<br/>
    /// Demi<br/>
    /// Quar<br/>
    /// ThreeQuar<br/>
    /// DoubleExclam<br/>
    /// Exp1<br/>
    /// Exp2<br/>
    /// Exp3<br/>
    /// ExpRelease<br/>
    /// Copyright<br/>
    /// AE<br/>
    /// AESmall<br/>
    /// Bull<br/>
    /// ArrowThickUp<br/>
    /// ArrowThickDown<br/>
    /// ArrowThickLeft<br/>
    /// ArrowThickRight<br/>
    /// ArrowUp<br/>
    /// ArrowRight<br/>
    /// ArrowDown<br/>
    /// ArrowLeftRight<br/>
    /// ArrowUpDown<br/>
    /// ArrowUpDownUnderline<br/>
    /// MoreOrLess<br/>
    /// CornerBottomLeft<br/>
    /// BarSmallDottedVertical<br/>
    /// LeftChevron<br/>
    /// RightChevron<br/>
    /// EdgeFlatTopRight<br/>
    /// BarHorizontal<br/>
    /// BarVertical<br/>
    /// EdgeTopLeft<br/>
    /// EdgeTopRight<br/>
    /// EdgeBottomLeft<br/>
    /// EdgeBottomRight<br/>
    /// EdgeRowLeft<br/>
    /// EdgeRowRight<br/>
    /// EdgeColTop<br/>
    /// EdgeColBottom<br/>
    /// EdgeRowColCross<br/>
    /// BarDoubleThickHorizontal<br/>
    /// BarDoubleHorizontal<br/>
    /// BarDoubleVertical<br/>
    /// EdgeDoubleTopLeft<br/>
    /// EdgeDoubleTopRight<br/>
    /// EdgeDoubleBottomLeft<br/>
    /// EdgeDoubleBottomRight<br/>
    /// EdgeDoubleRowLeft<br/>
    /// EdgeDoubleRowRight<br/>
    /// EdgeDoubleColTop<br/>
    /// EdgeDoubleColBottom<br/>
    /// EdgeDoubleRowColCross<br/>
    /// BoxHalfBottom<br/>
    /// BoxHalfTop<br/>
    /// Box<br/>
    /// BoxQuarLight<br/>
    /// BoxTierLight<br/>
    /// BoxHalfLight<br/>
    /// CardPic<br/>
    /// CardTrefl<br/>
    /// CardArt<br/>
    /// CardCarro<br/>
    /// </para>
    /// <para>
    /// &#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;<br/>
    /// &#128309; echo directives global syntax:<br/>
    /// &#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;&#175;<br/>
    ///     commandBlockBegin command commandValueAssignationChar value (commandSeparatorChar command commandValueAssignationChar value) commandBlockEnd<br/>
    ///     commandBlockBegin := (<br/>
    ///     commandBlockEnd := )<br/>
    ///     commandValueAssignationChar := =<br/>
    ///     commandSeparatorChar := ,<br/>
    ///     value := string_without_CommandBlockBegin_and_CommandBlockEnd) | ( codeBlockBegin any codeBlockEnd )<br/>
    ///     any := string<br/>
    ///     odeBlockBegin ::= [[<br/>
    ///     codeBlockEnd ::= ]]<br/>
    ///     syntactic elements can be changed for convenience &amp; personal preference<br/>
    /// </para>
    /// </summary>
    public enum EchoDirectives
    {
        // @TODO : nommages à revoir

        #region ANSI_Sequences

        /// <summary>
        /// ESC (char 27d)
        /// </summary>
        ESC,

        /// <summary>
        /// new line (depends on environment)
        /// </summary>
        CRLF,

        /// <summary>
        /// Control Sequence Introducer
        /// </summary>
        CSI,

        /// <summary>
        /// backup cursor position
        /// </summary>
        DECSC,

        /// <summary>
        /// restore cursor position
        /// </summary>
        DECRC,

        /// <summary>
        /// Triggers a full reset of the terminal
        /// </summary>
        RIS,

        /// <summary>
        /// RESET TEXT ATTRIBUTES (hack)
        /// </summary>
        RSTXTA,

        /// <summary>
        /// reset terminal colors
        /// </summary>
        RSCOLDEF,

        /// <summary>
        /// Cursor up
        /// </summary>        
        CUU,

        /// <summary>
        /// Cursor Down
        /// </summary>      
        CUD,

        /// <summary>
        /// Cursor Forward
        /// </summary>
        CUF,

        /// <summary>
        /// Cursor Back
        /// </summary>
        CUB,

        /// <summary>
        /// Cursor Next Line
        /// </summary>
        CNL,

        /// <summary>
        /// Cursor Previous Line
        /// </summary>
        CPL,

        /// <summary>
        /// Cursor Horizontal Absolute
        /// </summary>
        CHA,

        /// <summary>
        /// Cursor Position
        /// </summary>
        CUP,

        /// <summary>
        /// Scroll Up
        /// </summary>
        SU,

        /// <summary>
        /// Scroll Down
        /// </summary>
        SD,

        /// <summary>
        /// Device Status Report - Reports the cursor position
        /// </summary>
        DSR,

        /// <summary>
        /// Erases part of the screen
        /// </summary>
        ED,

        /// <summary>
        /// Erases part of the line
        /// </summary>
        EL,

        /// <summary>
        /// Reset / Normal - All attributes off
        /// </summary>
        SGR_Reset,

        /// <summary>
        /// Bold or increased intensity
        /// </summary>
        SGR_IncreasedIntensity,

        /// <summary>
        /// Faint or decreased intensity
        /// </summary>
        SGR_DecreaseIntensity,

        /// <summary>
        /// Italic
        /// </summary>
        SGR_Italic,

        /// <summary>
        /// Underline
        /// </summary>
        SGR_Underline,

        /// <summary>
        /// Slow Blink
        /// </summary>
        SGR_SlowBlink,

        /// <summary>
        /// Rapid Blink
        /// </summary>
        SGR_RapidBlink,

        /// <summary>
        /// Reverse video
        /// </summary>
        SGR_ReverseVideo,

        /// <summary>
        /// Italic off
        /// </summary>
        SGR_ItalicOff,

        /// <summary>
        ///Underline off
        /// </summary>
        SGR_UnderlineOff,

        /// <summary>
        /// BlinkOff
        /// </summary>
        SGR_BlinkOff,

        /// <summary>
        /// ReverseOff
        /// </summary>
        SGR_ReverseOff,

        /// <summary>
        /// NotCrossedOut
        /// </summary>
        SGR_NotCrossedOut,

        /// <summary>
        /// CrossedOut
        /// </summary>
        SGR_CrossedOut,

        /// <summary>
        /// DoubleUnderline
        /// </summary>
        SGR_DoubleUnderline,

        /// <summary>
        /// NormalIntensity
        /// </summary>
        SGR_NormalIntensity,

        /// <summary>
        /// Set foreground color - 3/4 bits palette mode : SGRF=0&lt;=n&lt;=7[,bright]
        /// </summary>
        SGRF,

        /// <summary>
        /// set foreground color - 8 bits palette (256 colors) : SGRF8=0&lt;=n&lt;=255
        /// </summary>
        SGRF8,

        /// <summary>
        /// set foreground color - 24 bits 'true color' : SGRF24=0&lt;=n&lt;=255,0&lt;=n&lt;=255,0&lt;=n&lt;=255 (r,g,b)
        /// </summary>
        SGRF24,

        /// <summary>
        /// Set background color - 3/4 bits palette mode : SGRF=0&lt;=n&lt;=7[,bright]
        /// </summary>
        SGRB,

        /// <summary>
        /// set background color - 8 bits palette (256 colors) : SGRF8=0&lt;=n&lt;=255
        /// </summary>
        SGRB8,

        /// <summary>
        /// set background color - 24 bits 'true color' : SGRF24=0&lt;=n&lt;=255,0&lt;=n&lt;=255,0&lt;=n&lt;=255 (r,g,b)
        /// </summary>
        SGRB24,

        #endregion

        #region colors management        

        /// <summary>
        /// backup foreground in the shell backup memory
        /// </summary>
        bkf,

        /// <summary>
        /// backup background in the shell backup memory
        /// </summary>
        bkb,

        /// <summary>
        /// restore foreground from the shell default foreground setting
        /// </summary>
        rsf,

        /// <summary>
        /// restore background from shell default background setting
        /// </summary>
        rsb,

        /// <summary>
        /// set colors to defaults from shell  default foreground and background colors
        /// </summary>
        rdc,

        /// <summary>
        /// set foreground from 4 bits palette : f=red (from ConsoleColor not case sensitive values)
        /// </summary>
        f,

        /// <summary>
        /// set foreground from 8 bits palette : f8=0&lt;=n&lt;=255
        /// </summary>
        f8,

        /// <summary>
        /// set foreground from 24 bits color : f24=r:g:b with 0&lt;=r,g,b&lt;=255
        /// </summary>
        f24,

        /// <summary>
        /// set background from 4 bits palette  : b=red (from ConsoleColor not case sensitive values)
        /// </summary>
        b,

        /// <summary>
        /// set background from 8 bits palette : b8=0&lt;=n&lt;=255
        /// </summary>
        b8,

        /// <summary>
        /// set background from 24 bits color : b24=r:g:b with 0&lt;=r,g,b&lt;=255
        /// </summary>
        b24,

        /// <summary>
        /// set default foreground : assign the shell default foreground memory
        ///<para>set the system console foreground</para>
        /// </summary>
        df,

        /// <summary>
        /// set default background : assign the shell default background memory
        /// <para>set the system console background</para>
        /// </summary>
        db,

        #endregion

        #region flow control

        /// <summary>
        /// line break - @Uses ConsoleTextWrapper.LineBreak
        /// </summary>
        br,

        /// <summary>
        /// clear screen - @Uses ConsoleTextWriterWrapper
        /// </summary>
        cls,

        /// <summary>
        /// fill current line to the right - shell impl. (@TODO: remove)
        /// </summary>
        fillright,

        /// <summary>
        /// clear line from cursor right - @[K
        /// </summary>
        clright,

        /// <summary>
        /// clear line from cursor left - @[1K
        /// </summary>
        clleft,

        /// <summary>
        /// clear entire line - @[2K
        /// </summary>
        cl,

        #endregion

        #region process control & information

        /// <summary>
        /// infos about console
        /// </summary>
        inf,

        /// <summary>
        /// exit the current process
        /// </summary>
        exit,

        /// <summary>
        /// exec csharp code from text
        /// </summary>
        exec,

        #endregion

        #region cursor management

        /// <summary>
        /// backup cursor pos in shell memory - @Uses system console cursor position
        /// </summary>
        bkcr,

        /// <summary>
        /// restore cursor pos from shell memory - @Uses @2J@[{top+1};{left+1})H
        /// </summary>
        rscr,

        /// <summary>
        /// hide cursor - @Uses system console
        /// </summary>
        crh,

        /// <summary>
        /// show cursor - @Uses system console
        /// </summary>
        crs,

        /// <summary>
        /// set cursor left (x) - @Uses @[{x+1}G (get @uses system console)
        /// </summary>
        crx,

        /// <summary>
        /// set cursor top (y) - @Uses @2J@[{top+1};{left+1})H [@TODO: avoid also set X !!!!] (get @uses system console)
        /// </summary>
        cry,

        /// <summary>
        /// cursor up - @[1A
        /// </summary>
        cup,

        /// <summary>
        /// cursor down - @[1B
        /// </summary>
        cdown,

        /// <summary>
        /// cursor left - @[1D
        /// </summary>
        cleft,

        /// <summary>
        /// cursor right- @[1C
        /// </summary>
        cright,

        /// <summary>
        /// cursor n lines up - @[1{n}A 
        /// </summary>
        cnup,

        /// <summary>
        /// cursor n lines down - @[{n}B
        /// </summary>
        cndown,

        /// <summary>
        /// cursor n cells left - @[{n}D
        /// </summary>
        cnleft,

        /// <summary>
        /// cursor n cells right- @[{n}C
        /// </summary>
        cnright,

        /// <summary>
        /// cursor home (top left 0,0) - @[H
        /// </summary>
        chome,

        #endregion

        #region text decoration

        /// <summary>
        /// underline on - sgr
        /// </summary>
        uon,

        /// <summary>
        /// bold on - sgr
        /// </summary>
        bon,

        /// <summary>
        /// blink on - sgr
        /// </summary>
        blon,

        /// <summary>
        /// invert/reverse on - sgr
        /// </summary>
        invon,

        /// <summary>
        /// light colors - sgr
        /// </summary>
        lion,

        /// <summary>
        /// text decoration off - sgr
        /// </summary>
        tdoff,

        #endregion

        #region unicode characters

        Lire,
        Yen,
        None,
        ARet,
        Demi,
        Quar,
        ThreeQuar,
        DoubleExclam,
        Exp1,
        Exp2,
        Exp3,
        ExpRelease,
        Copyright,
        AE,
        AESmall,
        Bull,
        ArrowThickUp,
        ArrowThickDown,
        ArrowThickLeft,
        ArrowThickRight,
        ArrowUp,
        ArrowRight,
        ArrowDown,
        ArrowLeftRight,
        ArrowUpDown,
        ArrowUpDownUnderline,
        MoreOrLess,
        CornerBottomLeft,
        BarSmallDottedVertical,
        LeftChevron,
        RightChevron,
        EdgeFlatTopRight,
        BarHorizontal,
        BarVertical,
        EdgeTopLeft,
        EdgeTopRight,
        EdgeBottomLeft,
        EdgeBottomRight,
        EdgeRowLeft,
        EdgeRowRight,
        EdgeColTop,
        EdgeColBottom,
        EdgeRowColCross,
        BarDoubleThickHorizontal,
        BarDoubleHorizontal,
        BarDoubleVertical,
        EdgeDoubleTopLeft,
        EdgeDoubleTopRight,
        EdgeDoubleBottomLeft,
        EdgeDoubleBottomRight,
        EdgeDoubleRowLeft,
        EdgeDoubleRowRight,
        EdgeDoubleColTop,
        EdgeDoubleColBottom,
        EdgeDoubleRowColCross,
        BoxHalfBottom,
        BoxHalfTop,
        Box,
        BoxQuarLight,
        BoxTierLight,
        BoxHalfLight,
        CardPic,
        CardTrefl,
        CardArt,
        CardCarro,

        #endregion
    }
}
