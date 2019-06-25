using osu.Framework;
using osu.Framework.Graphics;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class RefereeAssistant3Visual : Game
    {
        private readonly MainLoop core;

        public RefereeAssistant3Visual(MainLoop core) => this.core = core;

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Children = new Drawable[]
            {
                new NewMatchOverlay(core)
            };
        }
    }
}
