using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK.Graphics;

namespace RefereeAssistant3.Visual
{
    public class TeamButton : ClickableContainer
    {
        private readonly Box background;
        private Color4 backgroundColour;
        public Color4 BackgroundColour
        {
            get => backgroundColour;
            set
            {
                backgroundColour = value;
                UpdateLayout();
            }
        }

        public SpriteText Text { get; }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (!Enabled.Value)
                {
                    isSelected = false;
                    return;
                }
                isSelected = value;
                UpdateLayout();
            }
        }

        public TeamButton()
        {
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                Text = new SpriteText
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AllowMultiline = true
                }
            };
            Enabled.BindValueChanged(OnEnabledValueChanged);
        }

        private void OnEnabledValueChanged(ValueChangedEvent<bool> obj)
        {
            if (!obj.NewValue)
                IsSelected = false;
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            var transitionDuration = 200;
            var transitionEasing = Easing.OutCubic;
            if (IsSelected)
                background.FadeColour(BackgroundColour.Lighten(0.5f), transitionDuration, transitionEasing);
            else if (IsHovered && Enabled.Value)
                background.FadeColour(BackgroundColour.Lighten(0.1f), transitionDuration, transitionEasing);
            else
                background.FadeColour(BackgroundColour, transitionDuration, transitionEasing);
            if (Enabled.Value)
                Colour = Color4.White;
            else
                Colour = Color4.Gray;
        }

        protected override bool OnHover(HoverEvent e)
        {
            UpdateLayout();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UpdateLayout();
            base.OnHoverLost(e);
        }
    }
}
