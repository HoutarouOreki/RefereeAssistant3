using System;
using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Basic
{
    public static class ConsoleUtilities
    {
        /// <summary>
        /// Displays the provided options in the console and reads user's input.
        /// Then runs the function of the option the user selected, and returns its character.
        /// </summary>
        /// <remarks>
        /// Options are displayed every 5th user's attempt at input.
        /// </remarks>
        /// <param name="options">
        /// Options that will be displayed in the format "character - description".
        /// </param>
        /// <returns>The <see cref="UserSelectionOption.Character"/> the user selected.</returns>
        public static char DisplayOptionsAndExecuteSelected(IEnumerable<UserSelectionOption> options)
        {
            var i = 0;
            while (true)
            {
                if (i % 5 == 0)
                    DisplayOptions(options);
                i++;
                Console.Write('>');
                var inputLine = Console.ReadLine();
                if (inputLine.Length != 1)
                {
                    Console.WriteLine("Input must be a single character. Try again.");
                    continue;
                }
                if (!options.Any(o => o.Character == inputLine[0]))
                    Console.WriteLine("Incorrect option selection. Try again.");
                else
                {
                    options.First(o => o.Character == inputLine[0]).Function();
                    return inputLine[0];
                }
            }
        }

        private static void DisplayOptions(IEnumerable<UserSelectionOption> options)
        {
            foreach (var option in options)
                Console.WriteLine($"{option.Character} - {option.Description}");
        }
    }
}
