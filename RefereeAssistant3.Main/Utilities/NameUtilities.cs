using System.Collections.Generic;
using System.Linq;

namespace RefereeAssistant3.Main.Utilities
{
    public static class NameUtilities
    {
        /// <summary>
        /// Returns a unique name. If it already exists, appends (2),
        /// and begins incrementing the number until the name is unique.
        /// </summary>
        /// <param name="newSomethingString">The name of the thing, for example "New product".</param>
        /// <param name="otherNames">The collection of already existing names to check against.</param>
        /// <returns>A unique name.</returns>
        public static string GetUniqueNewName(string newSomethingString, IEnumerable<string> otherNames)
        {
            if (!otherNames.Any(otherName => otherName == newSomethingString))
                return newSomethingString;

            var value = newSomethingString;
            var i = 2;
            while (otherNames.Any(otherName => otherName == value))
            {
                i++;
                value = newSomethingString + $" ({i})";
            }

            return value;
        }
    }
}
