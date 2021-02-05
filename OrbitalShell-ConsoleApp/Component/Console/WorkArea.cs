using System.Drawing;

namespace OrbitalShell.Component.Console
{
    public class WorkArea
    {
        public readonly string Id;
        public readonly Rectangle Rect = Rectangle.Empty;

        public WorkArea() { }

        public WorkArea(string id,int x,int y,int width,int height)
        {
            Id = id;
            Rect = new Rectangle(x, y, width, height);
        }

        public WorkArea(WorkArea workArea)
        {
            Id = workArea.Id;
            Rect = new Rectangle(workArea.Rect.X, workArea.Rect.Y, workArea.Rect.Width, workArea.Rect.Height);
        }

        public bool IsEmpty => Rect.IsEmpty;
    }
}
