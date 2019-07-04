using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace RefereeAssistant3.Visual.UI
{
    public class UserAvatar : Container
    {
        private readonly int userId;

        public UserAvatar(int userId) => this.userId = userId;

        [BackgroundDependencyLoader]
        private void Load(TextureStore textures) => Add(new Sprite
        {
            RelativeSizeAxes = Axes.Both,
            Texture = textures.Get($"https://a.ppy.sh/{userId}")
        });
    }
}
