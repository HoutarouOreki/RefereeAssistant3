using osu.Framework;
using osu.Framework.Graphics;
using RefereeAssistant3.Main;

namespace RefereeAssistant3.Visual
{
    public class RefereeAssistant3Visual : Game
    {
        private readonly Core core;

        public RefereeAssistant3Visual(Core core) => this.core = core;

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
