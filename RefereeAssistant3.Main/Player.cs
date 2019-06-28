using osu.Framework.Graphics.Textures;
using osu.Framework.Threading;
using RefereeAssistant3.Main.Online.APIRequests;
using System;

namespace RefereeAssistant3.Main
{
    public class Player
    {
        public int? Id;
        public string Username;
        public Texture Avatar;

        public Player(string username) => Username = username;

        public Player(int id) => Id = id;

        public async void DownloadDataAsync(TextureStore textures, Action<Player> OnLoaded, Scheduler scheduler)
        {
            if (!Id.HasValue || string.IsNullOrEmpty(Username))
            {
                var req = new GetUsers(Id, Username).RunTask();
                await req;
                if (req.Result != null)
                {
                    Id = req.Result[0].Id;
                    Username = req.Result[0].Username;
                }
            }

            if (Avatar == null)
            {
                var avatarReq = textures.GetAsync($"https://a.ppy.sh/{Id}");
                Avatar = await avatarReq;
            }

            scheduler.Add(() => OnLoaded?.Invoke(this));
        }
    }
}
