using Newtonsoft.Json;
using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using RefereeAssistant3.Main.Online.APIRequests;
using RefereeAssistant3.Main.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main.Matches
{
    /// <summary>
    /// This class should be used on a per-one-match basis, since it contains members that break if the instance is in more than one match, such as <see cref="SelectedMods"/> and <see cref="SelectedTeam"/>.
    /// </summary>
    public class Player : MatchParticipant
    {
        public int? PlayerId;
        public string Username;
        /// <summary>
        /// If username is unavailable, returns #id
        /// </summary>
        public string IRCUsername => string.IsNullOrEmpty(Username) ?
            PlayerId.HasValue ? $"#{PlayerId.Value}" : null :
            Username?.Replace(' ', '_');

        public override string Name => IRCUsername;

        public Texture Avatar;

        public IEnumerable<Mods> SelectedMods;

        public TeamColour SelectedTeam;

        public List<PlayerMapResult> MapResults = new List<PlayerMapResult>();

        private string avatarCachePath => $"{PathUtilities.PlayersCacheDirectory}/{PlayerId}.png";

        public Player(int id) => PlayerId = id;

        public Player() { }

        public Player(string username) => Username = username;

        public async void DownloadFullDataAsync(TextureStore textures, Action<Player> OnLoaded, Scheduler scheduler)
        {
            if (PlayerId.HasValue && string.IsNullOrEmpty(Username))
                await Task.WhenAll(DownloadAvatar(textures), DownloadMetadata());
            else if (!PlayerId.HasValue || string.IsNullOrEmpty(Username))
            {
                await DownloadMetadata();
                await DownloadAvatar(textures);
            }
            else if (Avatar == null)
                await DownloadAvatar(textures);
            scheduler.Add(() => OnLoaded?.Invoke(this));
        }

        public async Task DownloadMetadata()
        {
            var req = await new GetUsers(PlayerId, Username).RunTask();
            if (req.Response.IsSuccessful && req.Object?.Length > 0)
            {
                PlayerId = req.Object[0].Id;
                Username = req.Object[0].Username;
            }
        }

        private async Task DownloadAvatar(TextureStore textures)
        {
            if (Avatar == null && PlayerId != null)
            {
                if (!File.Exists(avatarCachePath) || (DateTime.UtcNow - File.GetLastWriteTimeUtc(avatarCachePath)).TotalDays > 2)
                {
                    await Task.Run(() =>
                    {
                        try
                        {
                            using (var fileStream = new FileStream(avatarCachePath, FileMode.OpenOrCreate, FileAccess.Write))
                            {
                                var avatarOnlineStream = textures.GetStream($"https://a.ppy.sh/{PlayerId}");
                                var img = SixLabors.ImageSharp.Image.Load(avatarOnlineStream);
                                SixLabors.ImageSharp.ImageExtensions.SaveAsPng(img, fileStream);
                            }
                        }
                        catch { }
                    });
                }
                try
                {
                    using (var s = new FileStream(avatarCachePath, FileMode.Open, FileAccess.Read))
                    {
                        await Task.Run(() => Avatar = Texture.FromStream(s));
                    }
                }
                catch { }
            }
        }

        public override bool Equals(object obj)
        {
            Player other = null;
            string s = null;
            if (obj is Player player)
                other = player;
            if (obj is string username)
                s = username;
            if (other == null && s == null)
                return false;
            if ((!string.IsNullOrEmpty(other?.IRCUsername) && other.IRCUsername == IRCUsername) || (!string.IsNullOrEmpty(s) && s == IRCUsername))
                return true;
            if ((!string.IsNullOrEmpty(other?.Username) && other.Username == Username) || (!string.IsNullOrEmpty(s) && s == Username))
                return true;
            if ((PlayerId.HasValue && other?.PlayerId == PlayerId) || (int.TryParse(s?.Trim('#'), out var parsedId) && parsedId == PlayerId))
                return true;
            return false;
        }

        public override int GetHashCode() => IRCUsername.GetHashCode() * 2;
    }
}
