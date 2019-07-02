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
    public class AvatarUsernameLine : FillFlowContainer
    {
        private readonly Player player;
        private readonly Action<AvatarUsernameLine, Player> onDownloadComplete;
        private readonly Container avatarContainer;
        public readonly SpriteText UsernameText;
        private readonly SpriteText idText;

        public AvatarUsernameLine(Player player, bool avatarOnLeft, Action<AvatarUsernameLine, Player> onDownloadComplete = null)
        {
            this.player = player;
            this.onDownloadComplete = onDownloadComplete;
            Anchor = avatarOnLeft ? Anchor.TopLeft : Anchor.TopRight;
            Origin = avatarOnLeft ? Anchor.TopLeft : Anchor.TopRight;
            Spacing = new Vector2(6);
            AutoSizeAxes = Axes.Both;
            Direction = FillDirection.Horizontal;
            Children = new Drawable[]
            {
                avatarContainer = new Container
                {
                    Anchor = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                    Origin = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                    Size = new Vector2(24)
                },
                UsernameText = new SpriteText
                {
                    Anchor = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                    Origin = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                    Text = player.Username
                },
                idText = new SpriteText
                {
                    Anchor = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                    Origin = avatarOnLeft ? Anchor.CentreLeft : Anchor.CentreRight,
                    Text = player.Id.ToString()
                }
            };
        }

        [BackgroundDependencyLoader]
        private void Load(TextureStore textures)
        {
            player.DownloadDataAsync(textures, p =>
            {
                UsernameText.Text = p.Username;
                avatarContainer.Child = new Sprite { RelativeSizeAxes = Axes.Both, Texture = p.Avatar };
                idText.Text = p.Id.ToString();
                onDownloadComplete?.Invoke(this, player);
            }, Scheduler);
        }
    }
}
