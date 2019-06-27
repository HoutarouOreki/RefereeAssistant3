using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class Alert : RA3OverlayContainer
    {
        public override bool RemoveWhenNotAlive => true;

        public Alert(string message)
        {
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.GreenDarker,
                    Alpha = 0.9f
                },
                new FillFlowContainer
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
                            Action = PopOut
                        }
                    }
                }
            };
        }
    }
}
