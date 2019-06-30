using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using RefereeAssistant3.Main.Online.APIRequests;
using System;
using System.IO;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main
{
    public class Player
    {
        public int? Id;
        public string Username;
        public Texture Avatar;
        private string avatarCachePath => $"{Utilities.GetBaseDirectory()}/cache/players/{Id}.png";

        public Player(int id) => Id = id;

        public Player() { }

        public async void DownloadDataAsync(TextureStore textures, Action<Player> OnLoaded, Scheduler scheduler)
        {
            var avatarTask = DownloadAvatar(textures);
            var metadataTask = DownloadMetadata();
            if (Id.HasValue && string.IsNullOrEmpty(Username))
                await Task.WhenAll(avatarTask, metadataTask);
            else if (!Id.HasValue || string.IsNullOrEmpty(Username))
            {
                await metadataTask;
                await avatarTask;
            }
            scheduler.Add(() => OnLoaded?.Invoke(this));
        }

        private async Task DownloadMetadata()
        {
            var req = await new GetUsers(Id, Username).RunTask();
            if (req.Response.IsSuccessful && req.Object?.Length > 0)
            {
                Id = req.Object[0].Id;
                Username = req.Object[0].Username;
            }
        }

        private async Task DownloadAvatar(TextureStore textures)
        {
            if (Avatar == null && Id != null)
            {
                if (!File.Exists(avatarCachePath) || (DateTime.UtcNow - File.GetCreationTimeUtc(avatarCachePath)).TotalDays > 2)
                {
                    await Task.Run(() =>
                    {
                        using (var fileStream = new FileStream(avatarCachePath, FileMode.OpenOrCreate, FileAccess.Write))
                        {
                            var avatarOnlineStream = textures.GetStream($"https://a.ppy.sh/{Id}");
                            var img = SixLabors.ImageSharp.Image.Load(avatarOnlineStream);
                            SixLabors.ImageSharp.ImageExtensions.SaveAsPng(img, fileStream);
                        }
                    });
                }
                using (var s = new FileStream(avatarCachePath, FileMode.Open, FileAccess.Read))
                {
                    await Task.Run(() => Avatar = Texture.FromStream(s));
                }
            }
        }
    }
}
