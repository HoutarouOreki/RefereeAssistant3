using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
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
        private readonly SpriteText selectedNameText;
        private readonly Func<T, string> getTNameFunc;
        private readonly string typeName;
        private readonly SearchSelection<T> searchSelection;
        private const float selection_column_width = 300;
        private const float selected_name_container_height = 40;

        protected Container<Drawable> Content { get; } = new Container<Drawable>
        {
            RelativeSizeAxes = Axes.Both,
            Padding = new MarginPadding { Left = selection_column_width, Top = selected_name_container_height },
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre
        };

        public ScreenWithSelection(Func<T, string> getTNameFunc, IEnumerable<T> items, string typeName)
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
                        searchSelection = new SearchSelection<T>(getTNameFunc, items, Current)
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = selected_name_container_height,
                    Padding = new MarginPadding { Left = selection_column_width },
                    Children = new Drawable[]
                    {
                        new Box { RelativeSizeAxes = Axes.Both, Colour = FrameworkColour.GreenDark },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Left = 10, Vertical = 5 },
                            Child = selectedNameText = new SpriteText
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Font = new FontUsage(null, selected_name_container_height - 10)
                            }
                        }
                    }
                },
                Content
            };
            this.getTNameFunc = getTNameFunc;
            this.typeName = typeName;
        }

        protected override void LoadComplete()
        {
            Current.BindValueChanged(OnCurrentChangedPrivate, true);
            base.LoadComplete();
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

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Key == osuTK.Input.Key.Escape)
                return true;
            return base.OnKeyDown(e);
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            if (e.Key == osuTK.Input.Key.Escape)
                Hide();
            else
                base.OnKeyUp(e);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == osuTK.Input.MouseButton.Button1)
                return true;
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (e.Button == osuTK.Input.MouseButton.Button1)
                Hide();
            else
                base.OnMouseUp(e);
        }

        protected virtual void OnCurrentChanged(ValueChangedEvent<T> currentChange) { }

        protected void NameUpdated(T value) => searchSelection.NameUpdated(value);

        private void OnCurrentChangedPrivate(ValueChangedEvent<T> currentChange)
        {
            if (currentChange.NewValue == null)
                selectedNameText.Text = typeName;
            else
                selectedNameText.Text = $"{typeName} > {getTNameFunc(currentChange.NewValue)}";
            OnCurrentChanged(currentChange);
        }
    }
}
