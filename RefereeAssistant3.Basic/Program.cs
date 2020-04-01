using RefereeAssistant3.Basic.MatchRefereeing;
using RefereeAssistant3.Basic.TournamentConfiguration;
using RefereeAssistant3.Main;
using System;
using System.Collections.Generic;

namespace RefereeAssistant3.Basic
{
    public class Program
    {
        public static int Main()
        {
            Console.WriteLine("Welcome to ra3's console client.");
            Console.WriteLine();
            var core = new Core();
            while (true)
                MainMenu(core);
        }

        private static void MainMenu(Core core)
        {
            Console.WriteLine("Main menu:");
            var optionSelection = ConsoleUtilities.DisplayOptionsAndExecuteSelected(new List<UserSelectionOption>
            {
                new UserSelectionOption('n', "ref a new match", () => MatchManagement.CreateNewMatch(core)),
                new UserSelectionOption('c', "configure tournaments", () => TournamentManagement.TournamentConfigurationMenu(core)),
                new UserSelectionOption('e', "exit", () => Environment.Exit(0)),
            });
        }
    }
}
