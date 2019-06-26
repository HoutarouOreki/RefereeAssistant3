using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;

namespace RefereeAssistant3.Visual
{
    public class RA3OverlayContainer : FocusedOverlayContainer
    {
        protected override void PopIn()
        {
            Content.FadeIn(100);
            base.PopIn();
        }

        protected override void PopOut()
        {
            Content.FadeOut(100);
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
