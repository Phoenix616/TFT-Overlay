using System.Collections.Generic;

namespace TFT_Overlay.Data
{
    public interface ITftSynergy
    {
        string Id { get; }
        string Name { get; }
        string IconUrl { get; }
        string Description { get; }
        string Effects { get; }
        ICollection<TftChampion> Champions { get; }
    }
}