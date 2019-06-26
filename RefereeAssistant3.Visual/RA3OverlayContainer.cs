using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace RefereeAssistant3.Visual
{
    public class RA3OverlayContainer : FocusedOverlayContainer
    {
        public RA3OverlayContainer()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        protected override void PopIn()
        {
            Content.Scale = new osuTK.Vector2(1.2f);
            Content.FadeIn(100, Easing.OutCubic).ScaleTo(1, 100, Easing.OutCubic);
            base.PopIn();
        }

        protected override void PopOut()
        {
            Content.FadeOut(120, Easing.OutCubic).ScaleTo(1.06f, 120, Easing.OutCubic);
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
