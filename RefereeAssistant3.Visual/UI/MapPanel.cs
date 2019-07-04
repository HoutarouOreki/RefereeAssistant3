using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osuTK.Graphics;
using RefereeAssistant3.Main;
using System;
using System.Threading.Tasks;

namespace RefereeAssistant3.Visual.UI
{
    public class MapPanel : ClickableContainer
    {
        private readonly Box background;
        private readonly Sprite coverImage;
        private readonly SpriteText artistText;
        private readonly SpriteText difficultyText;
        private readonly SpriteText titleText;
        private TextureStore textures;

        public Map Map { get; }

        public new Action<Map> Action { get; set; }

        private Color4 backgroundColour => Color4.Black;

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
                    Colour = backgroundColour
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Colour = Color4.DarkGray,
                    Alpha = 0.5f,
                    Children = new Drawable[]
                    {
                        coverImage = new Sprite
                        {
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fill,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre
                        }
                    }
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
                        artistText = new SpriteText
                        {
                            Colour = Color4.LightGray,
                            Font = new FontUsage("OpenSans-Bold", 16)
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Horizontal,
                            Spacing = new osuTK.Vector2(7),
                            Children = new Drawable[]
                            {
                                titleText = new SpriteText { Font = new FontUsage("OpenSans-Bold") },
                                difficultyText = new SpriteText
                                {
                                    Font = new FontUsage(null, 16),
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft
                                }
                            }
                        }
                    }
                },
                new SpriteText
                {
                    Font = new FontUsage("OpenSans-Bold", 24),
                    Text = Map.MapCode,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Margin = new MarginPadding { Right = 14 }
                }
            };
        }

        private async void DownloadAndSetData(bool setAvatarAfterDownload)
        {
            await Map.DownloadDataAsync().ContinueWith(t => Schedule(() =>
            {
                artistText.Text = Map.Artist;
                titleText.Text = Map.Title;
                difficultyText.Text = Map.DifficultyName;
                if (setAvatarAfterDownload)
                    SetCover();
            }));    
        }

        protected override bool OnHover(HoverEvent e)
        {
            background.FadeColour(new Color4(100, 100, 100, 255), 100, Easing.OutCubic);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e) => background.FadeColour(backgroundColour, 100);

        protected override bool OnClick(ClickEvent e)
        {
            background.FlashColour(Color4.White, 500, Easing.OutQuart);
            Action(Map);
            return true;
        }

        [BackgroundDependencyLoader]
        private void Load(TextureStore textures)
        {
            this.textures = textures;
            DownloadAndSetData(!Map.MapsetId.HasValue);
            if (Map.MapsetId != null)
                SetCover();
        }

        private void SetCover() => Task.Run(() => Map.DownloadCover(textures)).ContinueWith(t => coverImage.Texture = Map.Cover);
    }
}
