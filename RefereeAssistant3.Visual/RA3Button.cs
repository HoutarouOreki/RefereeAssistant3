using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace RefereeAssistant3.Visual
{
    public class RA3Button : Button
    {
        private readonly Box hoverOverlay;

        public RA3Button()
        {
            Height = Style.COMPONENTS_HEIGHT;
            Add(hoverOverlay = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.White,
                Alpha = 0
            });
        }

        protected override bool OnHover(HoverEvent e)
        {
            hoverOverlay.FadeTo(0.2f, 120, Easing.OutCubic);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverOverlay.FadeOut(200, Easing.OutCubic);
            base.OnHoverLost(e);
        }
    }
}
