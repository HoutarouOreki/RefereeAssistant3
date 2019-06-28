using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using RefereeAssistant3.Main;
using System;

namespace RefereeAssistant3.Visual
{
    public class MapPanel : ClickableContainer
    {
        private readonly Box background;

        public Map Map { get; }

        new public Action<Map> Action { get; set; }

        public MapPanel(Map map)
        {
            Map = map;
            AutoSizeAxes = Axes.Y;
            Width = 400;
            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.Green,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Vertical = 4, Horizontal = 8 },
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = Map.Artist,
                            Colour = Color4.LightGray,
                            Font = new FontUsage(null, 16)
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new osuTK.Vector2(7),
                            Children = new Drawable[]
                            {
                                new SpriteText
                                {
                                    Text = Map.Title,
                                },
                                new SpriteText
                                {
                                    Text = $"[{Map.DifficultyName}]",
                                    Font = new FontUsage(null, 18),
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(FrameworkColour.Green.Lighten(0.5f), 100, Easing.OutCubic);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e) => background.FadeColour(FrameworkColour.Green, 100);

        protected override bool OnClick(ClickEvent e)
        {
            background.FlashColour(Color4.White, 500, Easing.OutQuart);
            Action(Map);
            return true;
        }
    }
}
