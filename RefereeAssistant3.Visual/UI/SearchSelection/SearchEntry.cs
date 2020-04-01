using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using System;

namespace RefereeAssistant3.Visual.UI.SearchSelection
{
    public class SearchEntry<T> : Container where T : class
    {
        private readonly Bindable<T> selectedEntryBindable;
        private readonly Func<T, string> getTNameFunc;
        private readonly SpriteText nameText;
        private readonly Box background;

        private static readonly ColourInfo deselected_colour = FrameworkColour.Blue;

        private static readonly ColourInfo selected_colour = FrameworkColour.Green;

        public T Value { get; }

        public SearchEntry(T value, Bindable<T> selectedEntryBindable, Func<T, string> getTNameFunc)
        {
            Value = value;
            this.selectedEntryBindable = selectedEntryBindable;
            this.getTNameFunc = getTNameFunc;
            Size = Style.COMPONENTS_SIZE;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(5),
                    Child = nameText = new SpriteText
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft
                    }
                }
            };
            UpdateNameText();
            selectedEntryBindable.BindValueChanged(OnSelectedEntryChanged, true);
        }

        private void OnSelectedEntryChanged(ValueChangedEvent<T> selectionChange)
        {
            if (selectionChange.NewValue == Value)
                OnSelected();
            else
                OnDeselected();
        }

        protected override bool OnClick(ClickEvent e)
        {
            selectedEntryBindable.Value = Value;
            return true;
        }

        private void OnDeselected() => background.FadeColour(deselected_colour);

        private void OnSelected() => background.FadeColour(selected_colour);

        private void UpdateNameText() => nameText.Text = getTNameFunc(Value);
    }
}
