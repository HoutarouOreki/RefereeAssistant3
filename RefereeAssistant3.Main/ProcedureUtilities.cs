using System.Collections.Generic;

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

            { MatchProcedure.Playing, $"Playing {match.SelectedMap?.MapCode}: {match.SelectedMap?.DisplayName}" },

            { MatchProcedure.FreePoint1, $"{match.Team1} receives a free point" },
            { MatchProcedure.FreePoint2, $"{match.Team2} receives a free point" },
            { MatchProcedure.FreePointRollWinner, $"{rollWinner} receives a free point" },
            { MatchProcedure.FreePointRollLoser, $"{rollLoser} receives a free point" }
        };

        public string GetProcedureDescription(MatchProcedure procedure) => readableMatchStateDictionary.GetValueOrDefault(procedure) ?? procedure.ToString();
    }

    public enum MatchProcedure
    {
        SettingUp = 0,
        WarmUp1 = 1,
        WarmUp2 = 2,
        Rolling = 3,
        Banning1 = 4,
        Banning2 = 5,
        Picking1 = 6,
        Picking2 = 7,
        GettingReady = 8,
        TieBreaker = 9,
        Playing = 10,
        FreePoint1 = 11,
        FreePoint2 = 12,
        BanningRollWinner = 13,
        BanningRollLoser = 14,
        PickingRollWinner = 15,
        PickingRollLoser = 16,
        WarmUpRollWinner = 17,
        WarmUpRollLoser = 18,
        FreePointRollWinner = 19,
        FreePointRollLoser = 20
    }

    public enum ProcedureTeam
    {
        Team1 = 0,
        Team2 = 1,
        RollWinner = 2,
        RollLoser = 3
    }
}
