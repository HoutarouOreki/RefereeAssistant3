﻿using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using RefereeAssistant3.Visual.UI;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Visual.Overlays
{
    public class SelectionOverlay<T> : RA3OverlayContainer
    {
        private readonly FillFlowContainer<SelectionOverlayButton> selectionButtons;
        public IEnumerable<T> Items;
        public Action<T> Action;

        public SelectionOverlay(IEnumerable<T> items)
        {
            Children = new Drawable[]
            {
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Width = Style.COMPONENTS_WIDTH,
                    Child = selectionButtons = new FillFlowContainer<SelectionOverlayButton>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new osuTK.Vector2(1)
                    }
                },
            };
            Items = items;
        }

        protected override void PopIn()
        {
            selectionButtons.Clear();
            foreach (var item in Items)
            {
                var selectionButton = new SelectionOverlayButton(item)
                {
                    Action = () =>
                    {
                        Action(item);
                        Hide();
                    }
                };
                selectionButtons.Add(selectionButton);
            }
            base.PopIn();
        }

        public class SelectionOverlayButton : ClickableContainer
        {
            public readonly T Item;
            private readonly Box background;

            public SelectionOverlayButton(T item)
            {
                RelativeSizeAxes = Axes.X;
                Height = Style.COMPONENTS_HEIGHT;
                Item = item;
                Children = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = FrameworkColour.YellowGreenDark
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(6),
                        Child = new SpriteText
                        {
                            Text = item.ToString(),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft
                        }
                    }
                };
            }

            protected override bool OnHover(HoverEvent e)
            {
                background.FadeColour(FrameworkColour.YellowGreen, 200);
                return base.OnHover(e);
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                background.FadeColour(FrameworkColour.YellowGreenDark, 200);
                base.OnHoverLost(e);
            }
        }
    }
}
