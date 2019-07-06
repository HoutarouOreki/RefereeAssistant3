namespace RefereeAssistant3.Main.IRC.Events
{
    public class PlayerFinishedEventArgs
    {
        public MpRoomIrcChannel Channel;
        public string Username;
        public int Score;
        public bool Passed;

        public PlayerFinishedEventArgs(MpRoomIrcChannel channel, string username, int score, bool passed)
        {
            Channel = channel;
            Username = username;
            Score = score;
            Passed = passed;
        }
    }
}
