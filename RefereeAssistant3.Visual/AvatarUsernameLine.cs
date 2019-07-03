using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osuTK;
using RefereeAssistant3.Main;
using System;

namespace RefereeAssistant3.Visual
{
    public class AvatarUsernameLine : Container
    {
        protected Player Player { get; private set; }
        private readonly Action<AvatarUsernameLine, Player> onDownloadComplete;
        protected Container AvatarContainer { get; private set; }
        public SpriteText UsernameText { get; private set; }
        protected SpriteText IdText { get; private set; }

        public AvatarUsernameLine(Player player, bool avatarOnLeft, Action<AvatarUsernameLine, Player> onDownloadComplete = null)
        {
            Player = player;
            this.onDownloadComplete = onDownloadComplete;
            Anchor = avatarOnLeft ? Anchor.TopLeft : Anchor.TopRight;
            Origin = avatarOnLeft ? Anchor.TopLeft : Anchor.TopRight;
            AutoSizeAxes = Axes.Both;
            AvatarContainer = new Container
            {
                Anchor = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                Origin = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                Size = new Vector2(24)
            };
            UsernameText = new SpriteText
            {
                Anchor = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                Origin = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                Text = Player?.Username
            };
            IdText = new SpriteText
            {
                Anchor = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                Origin = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                Text = Player?.Id.ToString()
            };
        }

        protected virtual void CreateContent()
        {
            Child = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Direction = FillDirection.Horizontal,
                Spacing = new Vector2(4),
                Children = new Drawable[]
                {
                    AvatarContainer,
                    UsernameText,
                    IdText
                }
            };
        }

        protected override void LoadComplete()
        {
            CreateContent();
            base.LoadComplete();
        }

        [BackgroundDependencyLoader]
        private void Load(TextureStore textures)
        {
            Player?.DownloadDataAsync(textures, p =>
            {
                UsernameText.Text = p.Username;
                AvatarContainer.Add(new Sprite { RelativeSizeAxes = Axes.Both, Texture = p.Avatar });
                IdText.Text = p.Id.ToString();
                onDownloadComplete?.Invoke(this, Player);
            }, Scheduler);
        }
    }
}
