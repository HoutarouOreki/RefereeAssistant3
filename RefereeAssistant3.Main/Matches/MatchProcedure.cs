namespace RefereeAssistant3.Main.Matches
{
    public class MatchProcedure<TParticipant> where TParticipant : MatchParticipant
    {
        public MatchProcedure(MatchProcedureTypes procedureType, TParticipant participant)
        {
            ProcedureType = procedureType;
            Participant = participant;
        }

        public MatchProcedure(MatchProcedureTypes procedureType) => ProcedureType = procedureType;

        public MatchProcedureTypes ProcedureType { get; private set; }
        public TParticipant Participant { get; private set; }

        public string Name => $"{ProcedureType}{(Participant != null ? $" - {Participant}" : string.Empty)}";
    }

    public enum MatchProcedureTypes
    {
        SettingUp = 0,
        WarmUp = 10,
        Rolling = 20,
        Banning = 30,
        Picking = 40,
        GettingReady = 50,
        TieBreaker = 60,
        Playing = 70,
        PlayingWarmUp = 71,
        FreePoint = 80
    }
}
