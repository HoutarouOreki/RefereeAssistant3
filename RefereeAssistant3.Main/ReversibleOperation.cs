using System;

namespace RefereeAssistant3.Main
{
    public class ReversibleOperation
    {
        public ReversibleOperation(string description, Action reverseAction)
        {
            Description = description;
            ReverseAction = reverseAction;
        }

        public string Description { get; }
        public Action ReverseAction { get; }
    }
}
