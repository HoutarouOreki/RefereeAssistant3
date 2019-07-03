using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;

namespace RefereeAssistant3.Visual
{
    public class RA3OverlayContainer : FocusedOverlayContainer
    {
        private readonly Box background;

        protected override Container<Drawable> Content { get; } = new Container<Drawable>
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding(Style.SPACING * 2),
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        };

        public RA3OverlayContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Depth = -10;
            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.GreenDarker,
                    Alpha = 0.9f
                },
                Content
            };
        }

        protected override void PopIn()
        {
            Content.Scale = new osuTK.Vector2(1.2f);
            Content.FadeIn(50, Easing.InExpo).ScaleTo(1, 100, Easing.OutCubic);
            background.FadeTo(0.95f, 50, Easing.InExpo);
            base.PopIn();
        }

        protected override void PopOut()
        {
            const int duration = 120;
            const Easing easing = Easing.OutCubic;
            Content.FadeOut(duration, easing).ScaleTo(1.06f, duration, easing);
            background.FadeOut(duration, easing);
            base.PopOut();
        }

        protected override bool OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == osuTK.Input.Key.Escape)
            {
                Hide();
                return true;
            }
            return base.OnKeyUp(e);
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            if (e.Button == osuTK.Input.MouseButton.Button1)
            {
                Hide();
                return true;
            }
            return base.OnMouseUp(e);
        }
    }
}
