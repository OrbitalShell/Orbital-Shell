namespace DotNetConsoleAppToolkit.Console
{
    public enum Color3BitMap
    {
        // 1

        darkgray = 0b0,
        gray = 0b1111,          // white on windows
        red =     0b1,
        green =   0b10,
        yellow =  0b11,
        blue =    0b100,
        magenta = 0b101,
        cyan =    0b110,
        white =   0b111,

        // 4

        black =      0b1000,
        darkred =       0b1001,
        darkgreen =     0b1010,
        darkyellow =    0b1011,     // yellow on windows
        darkblue =      0b1100,
        darkmagenta =   0b1101,
        darkcyan =      0b1110,
    }
}
