using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main.Tournaments
{
    public class Mappool
    {
        public List<Map> NoMod = new List<Map>();
        public List<Map> Hidden = new List<Map>();
        public List<Map> HardRock = new List<Map>();
        public List<Map> DoubleTime = new List<Map>();
        public List<Map> FreeMod = new List<Map>();
        public List<Map> Other = new List<Map>();
        public List<Map> AllMaps => NoMod.Concat(Hidden).Concat(HardRock).Concat(DoubleTime).Concat(FreeMod).Concat(Other).ToList();

        public async void DownloadMappoolAsync()
        {
            var tasks = new List<Task>();
            tasks.AddRange(NoMod.Select(m => m.DownloadDataAsync()));
            tasks.AddRange(Hidden.Select(m => m.DownloadDataAsync()));
            tasks.AddRange(HardRock.Select(m => m.DownloadDataAsync()));
            tasks.AddRange(DoubleTime.Select(m => m.DownloadDataAsync()));
            tasks.AddRange(FreeMod.Select(m => m.DownloadDataAsync()));
            await Task.WhenAll(tasks);
        }
    }
}
