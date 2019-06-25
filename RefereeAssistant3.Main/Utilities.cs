using System;
using System.IO;

namespace RefereeAssistant3.Main
{
    public static class Utilities
    {
        public static DirectoryInfo GetBaseDirectory()
        {
            var info = new DirectoryInfo($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}/RefereeAssistant3");
            if (!info.Exists)
                Directory.CreateDirectory(info.FullName);
            return info;
        }
    }
}
