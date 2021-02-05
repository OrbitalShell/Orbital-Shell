namespace OrbitalShell.Component.Console
{
    /// <summary>
    /// 4 bits colors map - see https://en.wikipedia.org/wiki/ANSI_escape_code#3-bit_and_4-bit
    /// </summary>
    public enum Color4BitMap
    {
        // 3 bits

        darkgray = 0b0,
        gray = 0b1111,
        red =     0b1,
        green =   0b10,
        yellow =  0b11,
        blue =    0b100,
        magenta = 0b101,
        cyan =    0b110,
        white =   0b111,

        // 4 bits

        black =      0b1000,
        darkred =       0b1001,
        darkgreen =     0b1010,
        darkyellow =    0b1011,
        darkblue =      0b1100,
        darkmagenta =   0b1101,
        darkcyan =      0b1110,
    }
}
