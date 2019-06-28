using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RefereeAssistant3.Main
{
    public class Mappool
    {
        public readonly List<Map> NoMod = new List<Map>();
        public readonly List<Map> Hidden = new List<Map>();
        public readonly List<Map> HardRock = new List<Map>();
        public readonly List<Map> DoubleTime = new List<Map>();
        public readonly List<Map> FreeMod = new List<Map>();

        public async void DownloadMappoolAsync(Core core)
        {
            var tasks = new List<Task>();
            tasks.AddRange(NoMod.Select(m => m.DownloadDataAsync(core)));
            tasks.AddRange(Hidden.Select(m => m.DownloadDataAsync(core)));
            tasks.AddRange(HardRock.Select(m => m.DownloadDataAsync(core)));
            tasks.AddRange(DoubleTime.Select(m => m.DownloadDataAsync(core)));
            tasks.AddRange(FreeMod.Select(m => m.DownloadDataAsync(core)));
            await Task.WhenAll(tasks);
        }
    }
}
