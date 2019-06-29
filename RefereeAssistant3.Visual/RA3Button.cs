using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace RefereeAssistant3.Visual
{
    public class RA3Button : ClickableContainer
    {
        private const float hover_alpha = 1f;
        private readonly Box hoverOverlay;
        private readonly Box background;

        public TextFlowContainer TextFlow { get; }
        public string Text { set => TextFlow.Text = value ?? ""; }

        public Color4 BackgroundColour
        {
            get => background.Colour;
            set
            {
                background.Colour = value;
                hoverOverlay.Colour = value.Lighten(0.5f);
            }
        }

        public RA3Button()
        {
            Height = Style.COMPONENTS_HEIGHT;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                hoverOverlay = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0
                },
                TextFlow = new TextFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    TextAnchor = Anchor.TopCentre
                }
            };
            BackgroundColour = FrameworkColour.Green;
            Enabled.BindValueChanged(OnEnabledValueChanged, true);
        }

        private void OnEnabledValueChanged(ValueChangedEvent<bool> obj)
        {
            Colour = Enabled.Value ? Color4.White : Color4.DarkGray;
            if (!Enabled.Value)
                hoverOverlay.FadeOut(200, Easing.OutCubic);
            else if (IsHovered)
                hoverOverlay.FadeTo(hover_alpha, 120, Easing.OutCubic);
        }

        protected override bool OnHover(HoverEvent e)
        {
            if (Enabled.Value)
                hoverOverlay.FadeTo(hover_alpha, 120, Easing.OutCubic);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            hoverOverlay.FadeOut(200, Easing.OutCubic);
            base.OnHoverLost(e);
        }
    }
}
