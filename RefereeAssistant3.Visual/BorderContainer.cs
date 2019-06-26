using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace RefereeAssistant3.Visual
{
    public class BorderContainer : Container
    {
        public BorderContainer() : this(1, true)
        { }

        public BorderContainer(float thickness = 1, bool left = true, bool top = true, bool right = true, bool bottom = true)
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Box { RelativeSizeAxes = Axes.X, Height = thickness, Alpha = top ? 1 : 0 },
                new Box { Anchor = Anchor.BottomRight, Origin = Anchor.BottomRight, RelativeSizeAxes = Axes.X, Height = thickness, Alpha = bottom ? 1 : 0 },
                new Box { RelativeSizeAxes = Axes.Y, Width = thickness, Alpha = left ? 1 : 0 },
                new Box { Anchor = Anchor.BottomRight, Origin = Anchor.BottomRight, RelativeSizeAxes = Axes.Y, Width = thickness, Alpha = right ? 1 : 0 }
            };
        }
    }
}
