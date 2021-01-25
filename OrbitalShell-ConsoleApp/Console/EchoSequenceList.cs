using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace OrbitalShell.Console
{
    public class EchoSequenceList : IEnumerable<EchoSequence>
    {
        public readonly List<EchoSequence> List
            = new List<EchoSequence>();

        public void Add(EchoSequence printSequence) => List.Add(printSequence);

        public override string ToString()
        {
            var r = new StringBuilder();
            foreach (var printSequence in List)
                r.AppendLine(printSequence.ToString());
            return r.ToString();
        }

        public string ToStringPattern()
        {
            var r = new StringBuilder();
            foreach (var printSequence in List)
                r.Append(printSequence.ToStringPattern());
            return r.ToString();
        }

        public IEnumerator<EchoSequence> GetEnumerator()
        {
            return List.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return List.GetEnumerator();
        }

        public int TextLength
        {
            get
            {
                int n = 0;
                foreach (var seq in List)
                    if (seq.IsText) n += seq.Length;
                return n;
            }
        }
    }
}
