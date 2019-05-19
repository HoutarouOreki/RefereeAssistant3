namespace RefereeAssistant3.Main
{
    /// <summary>
    /// A class with undoable actions.
    /// </summary>
    public interface IOperation
    {
        void UndoOperation();
    }
}
