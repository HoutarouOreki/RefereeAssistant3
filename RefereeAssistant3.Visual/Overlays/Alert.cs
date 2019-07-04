using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osuTK;
using RefereeAssistant3.Visual.UI;
using System;

namespace RefereeAssistant3.Visual.Overlays
{
    public class Alert : RA3OverlayContainer
    {
        private readonly RA3Button okButton;
        private readonly FillFlowContainer flow;

        public override bool RemoveWhenNotAlive => true;

        public Alert(string message)
        {
            Child = flow = new FillFlowContainer
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
                    okButton = new RA3Button
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

        public Alert(string message, string buttonMessage, Action buttonAction) : this(message)
        {
            flow.Remove(okButton);
            okButton.BackgroundColour = FrameworkColour.YellowDark;
            okButton.Text = "Cancel";
            flow.Add(new RA3Button
            {
                BackgroundColour = FrameworkColour.Green,
                Width = Style.COMPONENTS_WIDTH,
                Height = Style.COMPONENTS_HEIGHT,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Text = buttonMessage,
                Action = () =>
                {
                    Hide();
                    buttonAction();
                }
            });
            flow.Add(okButton);
        }
    }
}
