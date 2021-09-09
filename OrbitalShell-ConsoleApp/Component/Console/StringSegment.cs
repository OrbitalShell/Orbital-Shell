using System.Collections.Generic;
using System.Diagnostics;

using OrbitalShell.Lib;

namespace OrbitalShell.Component.Console
{
    [DebuggerDisplay("{Text}          {X}..{Y} ({ Y - X + 1 })")]
    public class StringSegment
    {
        public string Text { get; protected set; }

        public int X { get; protected set; }

        public int Y { get; protected set; }

        public int Length { get; protected set; }

        public bool IsEmpty { get; set; }

        public Dictionary<string, object> Map;

        public string FromArray(ref char[] t)
            => new(t[X..(Y + 1)]);

        public void SetText(string text, bool updateCoords = false)
        {
            Text = text;
            if (updateCoords)
            {
                Length = text.Length;
                Y = X + Length;
            }
        }

        public StringSegment(string text, int x, int y, int length)
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
            Length = y - x + 1;
        }

        public StringSegment(string text, int x, int y, int length, Dictionary<string, object> map)
        {
            Text = text;
            X = x;
            Y = y;
            Length = length;
            if (map != null && map.Count > 0)
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

        /// <summary>
        /// warn: this is intensively used in error messages...
        /// </summary>
        /// <returns>text representation of a StringSegment</returns>
        public override string ToString() => Text;
    }
}