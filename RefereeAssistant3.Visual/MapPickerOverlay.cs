using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using RefereeAssistant3.Main;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Visual
{
    public class MapPickerOverlay : RA3OverlayContainer
    {
        private readonly Core core;
        private readonly FillFlowContainer mapFlowContainer;

        private Mappool mappool => core.SelectedMatch.TournamentStage.Mappool;
        private IEnumerable<Map> noMod => mappool.NoMod.Except(core.SelectedMatch.UsedMaps);
        private IEnumerable<Map> hidden => mappool.Hidden.Except(core.SelectedMatch.UsedMaps);
        private IEnumerable<Map> hardRock => mappool.HardRock.Except(core.SelectedMatch.UsedMaps);
        private IEnumerable<Map> doubleTime => mappool.DoubleTime.Except(core.SelectedMatch.UsedMaps);
        private IEnumerable<Map> freeMod => mappool.FreeMod.Except(core.SelectedMatch.UsedMaps);

        public MapPickerOverlay(Core core)
        {
            this.core = core;
            RelativeSizeAxes = Axes.Both;
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = FrameworkColour.GreenDarker,
                    Alpha = 0.9f
                },
                new BasicScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Padding = new MarginPadding(Style.SPACING),
                    Child = mapFlowContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Spacing = new osuTK.Vector2(4)
                    }
                }
            };
        }

        protected override void PopIn()
        {
            GenerateLayout();
            base.PopIn();
        }

        private void GenerateLayout()
        {
            mapFlowContainer.Clear();
            var font = new FontUsage(null, 28);

            if (noMod.Any())
            {
                mapFlowContainer.Add(new SpriteText
                    { Text = "No mod", Font = font, Margin = new MarginPadding { Top = 8 } });
                AddMap(noMod);
            }

            if (hidden.Any())
            {
                mapFlowContainer.Add(new SpriteText
                    { Text = "Hidden", Font = font, Margin = new MarginPadding { Top = 8 } });
                AddMap(hidden);
            }

            if (hardRock.Any())
            {
                mapFlowContainer.Add(new SpriteText
                    { Text = "Hard rock", Font = font, Margin = new MarginPadding { Top = 8 } });
                AddMap(hardRock);
            }

            if (doubleTime.Any())
            {
                mapFlowContainer.Add(new SpriteText
                    { Text = "Double time", Font = font, Margin = new MarginPadding { Top = 8 } });
                AddMap(doubleTime);
            }

            if (freeMod.Any())
            {
                mapFlowContainer.Add(new SpriteText
                    { Text = "Free mod", Font = font, Margin = new MarginPadding { Top = 8 } });
                AddMap(freeMod);
            }
        }

        private void AddMap(IEnumerable<Map> maps)
        {
            foreach (var map in maps)
            {
                var panel = new MapPanel(map);
                mapFlowContainer.Add(panel);
                panel.Action = PanelClicked;
            }
        }

        private void PanelClicked(Map map)
        {
            Hide();
            core.SelectedMatch.SelectedMap = map;
        }
    }
}
