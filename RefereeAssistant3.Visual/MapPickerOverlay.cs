using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class MapPickerOverlay : RA3OverlayContainer
    {
        private readonly Core core;
        private readonly FillFlowContainer mapFlowContainer;

        private Mappool mappool => core.SelectedMatch.TournamentStage.Mappool;

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
                        Origin = Anchor.TopCentre
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
            mapFlowContainer.Add(new SpriteText { Text = "No mod", Font = font });
            foreach (var map in mappool.NoMod)
                mapFlowContainer.Add(new SpriteText { Text = map.ToString() });
            mapFlowContainer.Add(new SpriteText { Text = "Hidden", Font = font });
            foreach (var map in mappool.Hidden)
                mapFlowContainer.Add(new SpriteText { Text = map.ToString() });
            mapFlowContainer.Add(new SpriteText { Text = "Hard rock", Font = font });
            foreach (var map in mappool.HardRock)
                mapFlowContainer.Add(new SpriteText { Text = map.ToString() });
            mapFlowContainer.Add(new SpriteText { Text = "Double time", Font = font });
            foreach (var map in mappool.DoubleTime)
                mapFlowContainer.Add(new SpriteText { Text = map.ToString() });
            mapFlowContainer.Add(new SpriteText { Text = "Free mod", Font = font });
            foreach (var map in mappool.FreeMod)
                mapFlowContainer.Add(new SpriteText { Text = map.ToString() });
        }
    }
}
