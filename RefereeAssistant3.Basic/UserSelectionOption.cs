using System;

namespace RefereeAssistant3.Basic
{
    public class UserSelectionOption
    {
        public readonly char Character;
        public readonly string Description;
        public readonly Action Function;

        public UserSelectionOption(char character, string description, Action function)
        {
            Character = character;
            Description = description;
            Function = function;
        }
    }
}
