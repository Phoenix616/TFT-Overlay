using System.Collections.Generic;

namespace TFT_Overlay.Data
{
    public class TftClass : ITftSynergy
    {
        public string Id { get; }
        public string Name { get; }
        public string IconUrl { get; }
        public string Description { get; }
        public string Effects { get; }
        public ICollection<TftChampion> Champions { get; } = new List<TftChampion>();
        
        public TftClass(string id, string name, string iconUrl, string description, string effect)
        {
            Id = id;
            Name = name;
            IconUrl = iconUrl;
            Description = description;
            Effects = effect;
        }
    }
}