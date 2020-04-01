using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Screens;
using RefereeAssistant3.Visual.UI.SearchSelection;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Visual.Screens
{
    public abstract class ScreenWithSelection<T> : Screen where T : class
    {
        protected readonly Bindable<T> Current = new Bindable<T>();
        private readonly Box background;
        private const float selection_column_width = 300;

        protected Container<Drawable> Content { get; } = new Container<Drawable>
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Left = selection_column_width },
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        };

        public ScreenWithSelection(Func<T, string> getTNameFunc, IEnumerable<T> items)
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.GreenDarker,
                    Alpha = 0.9f
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = selection_column_width,
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = FrameworkColour.BlueDark },
                        new SearchSelection<T>(getTNameFunc, items, Current)
                    }
                },
                Content
            };
        }

        public override void OnEntering(IScreen last)
        {
            Content.Scale = new osuTK.Vector2(1.2f);
            Content.FadeIn(50, Easing.InExpo).ScaleTo(1, 100, Easing.OutCubic);
            background.FadeTo(0.95f, 50, Easing.InExpo);
            base.OnEntering(last);
        }

        public override bool OnExiting(IScreen next)
        {
            const int duration = 120;
            const Easing easing = Easing.OutCubic;
            Content.FadeOut(duration, easing).ScaleTo(1.06f, duration, easing);
            background.FadeOut(duration, easing);
            return base.OnExiting(next);
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
