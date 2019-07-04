using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osuTK;
using RefereeAssistant3.Main;
using RefereeAssistant3.Main.APIModels;
using RefereeAssistant3.Main.Online.APIRequests;
using RefereeAssistant3.Visual.UI;

namespace RefereeAssistant3.Visual.Overlays
{
    public class MapFinderOverlay : RA3OverlayContainer
    {
        private readonly BasicTextBox textBox;
        private readonly Core core;
        private readonly FillFlowContainer mapFlow;
        private readonly TextFlowContainer textFlow;

        public MapFinderOverlay(Core core)
        {
            this.core = core;
            Child = new FillFlowContainer
            {
                Spacing = new Vector2(Style.SPACING),
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    textBox = new BasicTextBox
                    {
                        Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT),
                        PlaceholderText = "Beatmap difficulty ID",
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre
                    },
                    new RA3Button
                    {
                        Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT),
                        Text = "Find",
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Action = Find
                    },
                    new RA3Button
                    {
                        Size = new Vector2(Style.COMPONENTS_WIDTH, Style.COMPONENTS_HEIGHT),
                        Text = "Use placeholder map",
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        BackgroundColour = FrameworkColour.YellowGreen,
                        Action = () => MapSelected(new Map
                        {
                            Artist = "Artist",
                            DifficultyId = -1,
                            DifficultyName = "Difficulty",
                            MapCode = "Code",
                            MapsetId = -1,
                            Title = "Title"
                        })
                    },
                    mapFlow = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre
                    },
                    textFlow = new TextFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        TextAnchor = Anchor.TopCentre
                    }
                }
            };
        }

        private void Find()
        {
            mapFlow.Clear();
            if (!int.TryParse(textBox.Text, out var id))
            {
                textFlow.Text = "Parsing the id failed. Make sure you entered a number.";
                return;
            }
            textFlow.Text = "Map request in progress.";
            var req = new GetMap(id);
            Add(req);
            req.Success += OnSearchSuccess;
            req.Fail += OnSearchFail;
            req.Run();
        }

        private void OnSearchFail(object obj, System.Net.HttpStatusCode code)
        {
            textFlow.Text = $"Search request failed: {code}";
            textFlow.AddParagraph(obj.ToString());
        }

        private void OnSearchSuccess(APIMap[] maps, System.Net.HttpStatusCode code)
        {
            if (maps.Length == 0)
            {
                textFlow.Text = "No maps found. Make sure you entered the correct ";
                textFlow.AddText("difficulty", t => t.Font = new FontUsage("OpenSans-Bold"));
                textFlow.AddText(" ID.");
                return;
            }
            textFlow.Text = "";
            foreach (var map in maps)
            {
                var mapPanel = new MapPanel(new Map(map))
                {
                    Action = MapSelected
                };
                mapFlow.Add(mapPanel);
            }
        }

        private void MapSelected(Map obj)
        {
            core.SelectedMatch.Value.SelectedMap = obj;
            Hide();
        }
    }
}
