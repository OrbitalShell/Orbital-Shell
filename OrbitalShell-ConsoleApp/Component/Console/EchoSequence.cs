using System;
using OrbitalShell.Component.EchoDirective;

namespace OrbitalShell.Component.Console
{
    public class EchoSequence
    {
        public readonly EchoDirectives? PrintDirective;
        public readonly int FirstIndex;
        public readonly int LastIndex;
        public readonly string Value;
        public readonly string Text;
        public int Length => LastIndex - FirstIndex + 1;

        IConsole Console;

        public EchoSequence(
            IConsole console,
            EchoDirectives? printDirective,
            int firstIndex,
            int lastIndex,
            string value,
            string text,
            int relIndex=0)
        {
            Console = console;
            PrintDirective = printDirective;
            FirstIndex = firstIndex + relIndex;
            LastIndex = lastIndex + relIndex;
            Value = value;
            Text = text;
        }

        public EchoSequence(
            IConsole console,
            string printDirective,
            int firstIndex,
            int lastIndex,
            string value,
            string text,
            int relIndex=0)
        {
            Console = console;
            if (printDirective != null)
                if (Enum.TryParse<EchoDirectives>(printDirective, out var pr))
                    PrintDirective = pr;

            FirstIndex = firstIndex+relIndex;
            LastIndex = lastIndex + relIndex;
            Value = value;
            Text = text;
        }

        public string ToText()
        {
            var s = "";
            if (PrintDirective.HasValue && Value == null)
                s += $"{Console.CommandBlockBeginChar}{PrintDirective}{Console.CommandBlockEndChar}";
            else
            {
                if (PrintDirective.HasValue && Value != null)
                    s += $"{Console.CommandBlockBeginChar}{PrintDirective}{Console.CommandValueAssignationChar}{Value}{Console.CommandBlockEndChar}";
                else
                    s += Text;
            }
            return s;
        }

        public override string ToString()
        {
            var s = $"{FirstIndex}..{LastIndex}({Length})  ";
            if (PrintDirective.HasValue && Value == null)
                s += $"{PrintDirective}";
            else
            {
                if (PrintDirective.HasValue && Value != null)
                    s += $"{PrintDirective}={Value}";
                else
                    s += Text;
            }
            return s;
        }

        public string ToStringPattern()
        {
            var c = "-";
            var s = "";
            if (PrintDirective.HasValue && Value == null)
                s += $"{c}{PrintDirective}{c}";
            else
            {
                if (PrintDirective.HasValue && Value != null)
                    s += $"{c}{PrintDirective}={Value}{c}";
                else
                    s += Text;
            }
            return s;
        }

        public bool IsText => !PrintDirective.HasValue;
    }
}
