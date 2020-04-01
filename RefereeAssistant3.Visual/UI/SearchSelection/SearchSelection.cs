using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Visual.UI.SearchSelection
{
    public class SearchSelection<T> : Container where T : class
    {
        private const float search_height = Style.COMPONENTS_HEIGHT;
        private readonly BasicTextBox searchBox;
        private readonly FillFlowContainer<SearchEntry<T>> entriesFlow;
        private readonly Func<T, string> getTNameFunc;
        private readonly Bindable<T> selectedValueBindable;

        public SearchSelection(Func<T, string> getTNameFunc, IEnumerable<T> items, Bindable<T> selectedValueBindable)
        {
            this.getTNameFunc = getTNameFunc;
            this.selectedValueBindable = selectedValueBindable;
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = search_height,
                    Child = searchBox = new BasicTextBox
                    {
                        RelativeSizeAxes = Axes.Both,
                        PlaceholderText = "Search..."
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = search_height },
                    Child = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Bottom = Style.COMPONENTS_HEIGHT },
                                Child = new BasicScrollContainer
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = entriesFlow = new FillFlowContainer<SearchEntry<T>>
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y
                                    }
                                }
                            },
                            new RA3Button
                            {
                                Text = "Create New",
                                Size = Style.COMPONENTS_SIZE,
                                Anchor = Anchor.BottomCentre,
                                Origin = Anchor.BottomCentre
                            }
                        }
                    }
                }
            };
            searchBox.Current.BindValueChanged(OnSearchChanged, true);
            foreach (var item in items)
                AddEntry(item);
        }

        public void AddEntry(T entry) => entriesFlow.Add(new SearchEntry<T>(entry, selectedValueBindable, getTNameFunc));

        public void NameUpdated(T value)
        {
            foreach (var entry in entriesFlow)
            {
                if (entry.Value == value)
                    entry.UpdateNameText();
            }
        }

        private void OnSearchChanged(ValueChangedEvent<string> searchChange)
        {
            foreach (var entry in entriesFlow)
            {
                if (string.IsNullOrEmpty(searchChange.NewValue) || getTNameFunc(entry.Value).Contains(searchChange.NewValue))
                {
                    entry.Show();
                }
                else
                {
                    entry.Hide();
                }
            }
        }
    }
}
