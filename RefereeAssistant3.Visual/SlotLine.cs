using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osuTK;
using osuTK.Graphics;
using RefereeAssistant3.Main;
using System;

namespace RefereeAssistant3.Visual
{
    public class SlotLine : AvatarUsernameLine
    {
        private readonly int slot;

        public SlotLine(int slot, Player player, Action<AvatarUsernameLine, Player> onDownloadComplete = null) : base(player, true, onDownloadComplete) =>  this.slot = slot;

        protected override void CreateContent()
        {
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(4),
                    Children = new Drawable[]
                    {
                        AvatarContainer,
                        new Box
                        {
                            BypassAutoSizeAxes = Axes.Y,
                            RelativeSizeAxes = Axes.Y,
                            Width = 4,
                            Colour = Player == null ? Color4.Black : Player.SelectedTeam == TeamColour.Red ? Style.Red : Style.Blue
                        },
                        UsernameText
                    }
                }
            };
            AvatarContainer.Add(new Box { RelativeSizeAxes = Axes.Both, Colour = Color4.Black, Alpha = 0.2f, Depth = -1 });
            AvatarContainer.Add(new SpriteText { Anchor = Anchor.Centre, Origin = Anchor.Centre, Text = $"{slot}", Depth = -2, Font = new FontUsage("OpenSans-Bold", 22), Colour = Color4.White, Shadow = true, ShadowColour = Color4.Black });
        }
    }
}
