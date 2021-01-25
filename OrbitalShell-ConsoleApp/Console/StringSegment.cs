using OrbitalShell.Lib;
using System.Collections.Generic;

namespace OrbitalShell.Console
{
    public class StringSegment
    {
        public readonly string Text;
        public readonly int X;
        public readonly int Y;
        public readonly int Length;

        public Dictionary<string, object> Map;

        public StringSegment(string text,int x,int y,int length)
        {
            Text = text;
            X = x;
            Y = y;
            Length = length;
        }

        public StringSegment(string text, int x, int y)
        {
            Text = text;
            X = x;
            Y = y;
            Length = y-x+1;
        }

        public StringSegment(string text, int x, int y, int length, Dictionary<string, object> map)
        {
            Text = text;
            X = x;
            Y = y;
            Length = length;
            if (map!=null && map.Count>0)
                Map = new Dictionary<string, object> { map };
        }

        public StringSegment(string text, int x, int y, Dictionary<string, object> map)
        {
            Text = text;
            X = x;
            Y = y;
            Length = y - x + 1;
            if (map != null && map.Count > 0)
                Map = new Dictionary<string, object> { map };
        }

        public override string ToString()
        {
            return $"pos={X},{Y} l={Length} Text={Text}";
        }
    }
}
