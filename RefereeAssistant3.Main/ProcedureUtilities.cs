using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main
{
    public class ProcedureUtilities
    {
        private readonly Match match;

        public ProcedureUtilities(Match match) => this.match = match;

        private string rollWinner => match.RollWinner?.TeamName ?? "roll winner";
        private string rollLoser => match.RollLoser?.TeamName ?? "roll loser";

        private Dictionary<MatchProcedure, string> readableMatchStateDictionary => new Dictionary<MatchProcedure, string>
        {
            { MatchProcedure.SettingUp, "Setting up the match" },

            { MatchProcedure.WarmUp1, $"Warmup by {match.Team1}" },
            { MatchProcedure.WarmUp2, $"Warmup by {match.Team2}" },

            { MatchProcedure.WarmUpRollWinner, $"Warmup by {rollWinner}" },
            { MatchProcedure.WarmUpRollLoser, $"Warmup by {rollLoser}" },

            { MatchProcedure.Rolling, $"Teams rolling" },

            { MatchProcedure.Banning1, $"{match.Team1} banning" },
            { MatchProcedure.Banning2, $"{match.Team2} banning" },
            { MatchProcedure.BanningRollWinner, $"{rollWinner} banning" },
            { MatchProcedure.BanningRollLoser, $"{rollLoser} banning" },

            { MatchProcedure.Picking1, $"{match.Team1} picking" },
            { MatchProcedure.Picking2, $"{match.Team2} picking" },
            { MatchProcedure.PickingRollWinner, $"{rollWinner} picking" },
            { MatchProcedure.PickingRollLoser, $"{rollLoser} picking" },

            { MatchProcedure.GettingReady, "Players getting ready" },

            { MatchProcedure.TieBreaker, "Tiebreaker!" },

            { MatchProcedure.Playing, $"Playing" + (match.SelectedMap != null ? $" {match.SelectedMap?.MapCode}: {match.SelectedMap}" : "") },

            { MatchProcedure.FreePoint1, $"{match.Team1} receives a free point" },
            { MatchProcedure.FreePoint2, $"{match.Team2} receives a free point" },
            { MatchProcedure.FreePointRollWinner, $"{rollWinner} receives a free point" },
            { MatchProcedure.FreePointRollLoser, $"{rollLoser} receives a free point" }
        };

        private static readonly IReadOnlyList<MatchProcedure> procedures_requiring_selected_map = new List<MatchProcedure>
        {
            MatchProcedure.WarmUp1,
            MatchProcedure.WarmUp2,
            MatchProcedure.WarmUpRollWinner,
            MatchProcedure.WarmUpRollLoser,
            MatchProcedure.Banning1,
            MatchProcedure.Banning2,
            MatchProcedure.BanningRollLoser,
            MatchProcedure.BanningRollWinner,
            MatchProcedure.GettingReady,
            MatchProcedure.Picking1,
            MatchProcedure.Picking2,
            MatchProcedure.PickingRollLoser,
            MatchProcedure.PickingRollWinner,
            MatchProcedure.Playing,
            MatchProcedure.TieBreaker
        };

        public bool CurrentProcedureRequireSelectedMap() => procedures_requiring_selected_map.Contains(match.CurrentProcedure);

        public string GetProcedureDescription(MatchProcedure procedure) => readableMatchStateDictionary.GetValueOrDefault(procedure) ?? procedure.ToString();
    }

    public enum MatchProcedure
    {
        SettingUp = 0,

        WarmUp1 = 11,
        WarmUp2 = 12,
        WarmUpRollWinner = 13,
        WarmUpRollLoser = 14,

        Rolling = 20,

        Banning1 = 31,
        Banning2 = 32,
        BanningRollWinner = 33,
        BanningRollLoser = 34,

        Picking1 = 41,
        Picking2 = 42,
        PickingRollWinner = 43,
        PickingRollLoser = 44,

        GettingReady = 50,

        TieBreaker = 60,

        Playing = 70,

        FreePoint1 = 81,
        FreePoint2 = 82,
        FreePointRollWinner = 83,
        FreePointRollLoser = 84
    }

    public enum ProcedureTeam
    {
        Team1 = 0,
        Team2 = 1,
        RollWinner = 2,
        RollLoser = 3
    }
}
