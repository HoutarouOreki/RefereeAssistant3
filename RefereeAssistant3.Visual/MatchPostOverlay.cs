﻿using System;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using RefereeAssistant3.Main;
using RefereeAssistant3.Main.Online.APIRequests;

namespace RefereeAssistant3.Visual
{
    public class MatchPostOverlay : RA3OverlayContainer
    {
        private readonly BasicTextBox matchCodeTextBox;
        private readonly Core core;
        private readonly RA3Button button;

        public MatchPostOverlay(Core core)
        {
            this.core = core;
            Children = new Drawable[]
            {
                new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        matchCodeTextBox = new BasicTextBox
                        {
                            PlaceholderText = "Match code",
                            Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT)
                        },
                        button = new RA3Button
                        {
                            Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT)
                        }
                    }
                }
            };
        }

        protected override void PopIn()
        {
            matchCodeTextBox.Text = core.SelectedMatch.Code;
            button.Action = null;
            button.Text = "";
            if (core.SelectedMatch.Id == -1)
            {
                button.Action = PostMatch;
                button.Text = "Submit match";
            }
            base.PopIn();
        }

        private void PostMatch()
        {
            core.SelectedMatch.Code = matchCodeTextBox.Text;
            core.SelectedMatch.Id = -2;
            button.Action = null;
            button.Text = "";
            var req = core.PostMatchAsync();
            req.ContinueWith(res => Schedule(Hide));
        }
    }
}
