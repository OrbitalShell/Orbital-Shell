using System;

namespace OrbitalShell.Component.UI
{
    public class WorkAreaScrollEventArgs
        : EventArgs
    {
        public readonly int DeltaX;
        public readonly int DeltaY;

        public WorkAreaScrollEventArgs(int deltaX,int deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }
    }
}
