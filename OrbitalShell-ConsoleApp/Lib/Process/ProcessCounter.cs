using System.Threading;

namespace OrbitalShell.Lib.Process
{
    /// <summary>
    /// process counter usable with ProcessWrapper
    /// </summary>
    public class ProcessCounter
    {
        protected int _counter;
        protected object _counterLock = new object();
        protected bool _log = true;

        public int Counter {
            get
            {
                lock (_counterLock)
                {
                    return _counter;
                }
            }
        }

        public int GetCounter()
        {
            return _counter;
        }

        public ProcessCounter()
        {
            this._counter = 0;
        }

        public void Increase() {
            lock (_counterLock)
            {
                _counter++;
                if (_log) System.Diagnostics.Debug.WriteLine("ProcessCounter:Increased = " + _counter);
            }
        }

        public void Decrease() {
            lock (_counterLock)
            {
                _counter--;
                if (_log) System.Diagnostics.Debug.WriteLine("ProcessCounter:Decreased = " + _counter);
            }
        }

        public void WaitForLessThan(int N)
        {
            if (_log) System.Diagnostics.Debug.WriteLine("ProcessCounter:WaitForLessThan " + N);
            Thread t = new Thread(() => { WaitForLessThanNInternal(N); });
            t.Start();
            t.Join();
        }

        protected void WaitForLessThanNInternal(int n)
        {
            bool end = false;
            while (!end)
            {
                int v;
                lock (_counterLock)
                {
                    v = GetCounter();
                    if (v < n)
                        end = true;
                    if (!end)
                    {
                        Thread.Yield();
                        Thread.Sleep(50);
                    }
                }
            }
        }
    }
}
