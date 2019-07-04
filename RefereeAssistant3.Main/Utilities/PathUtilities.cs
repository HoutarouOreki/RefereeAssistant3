using System;
using System.IO;

namespace RefereeAssistant3.Main.Utilities
{
    public static class PathUtilities
    {
        public static readonly DirectoryInfo RootProgramDirectory = new DirectoryInfo($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/RefereeAssistant3");

        public static readonly DirectoryInfo PlayersCacheDirectory = new DirectoryInfo($"{RootProgramDirectory.FullName}/cache/players");

        public static readonly DirectoryInfo MapsCacheDirectory = new DirectoryInfo($"{RootProgramDirectory.FullName}/cache/maps");

        public static readonly DirectoryInfo TournamentsDirectory = new DirectoryInfo($"{RootProgramDirectory.FullName}/tournaments");

        public static readonly DirectoryInfo SavedMatchesDirectory = new DirectoryInfo($"{RootProgramDirectory.FullName}/savedMatches");
    }
}
