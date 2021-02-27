using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrbitalShell.Lib
{
    /// <summary>
    /// generic event args + IsCanceled information
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class EventArgs<T> : EventArgs
    {
        public T Value { get; set; }
        
        public bool IsCanceled { get; set; }

        public EventArgs(T val)
        {
            Value = val;            
        }

        public EventArgs() { }

        public void Recycle()
        {
            Value = default;
            IsCanceled = false;
        }
    }
}
