using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;

namespace RefereeAssistant3.Visual
{
    public class Alert : RA3OverlayContainer
    {
        public override bool RemoveWhenNotAlive => true;

        public Alert(string message)
        {
            Child = new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(Style.SPACING),
                Children = new Drawable[]
                {
                    new TextFlowContainer
                    {
                        Text = message,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        TextAnchor = Anchor.TopCentre
                        //MaximumSize = new Vector2(500, 0)
                    },
                    new RA3Button
                    {
                        Text = "Ok",
                        BackgroundColour = FrameworkColour.Green,
                        Width = Style.COMPONENTS_WIDTH,
                        Height = Style.COMPONENTS_HEIGHT,
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Action = Hide
                    }
                }
            };
        }
    }
}
