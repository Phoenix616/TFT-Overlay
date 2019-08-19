using System.Collections.Generic;

namespace TFT_Overlay.Data
{
    public class TftOrigin : ITftSynergy
    {
        public string Id { get; }
        public string Name { get; }
        public string IconUrl { get; }
        public string Description { get; }
        public string Effects { get; }
        public ICollection<TftChampion> Champions { get; } = new List<TftChampion>();
        
        public TftOrigin(string id, string name, string iconUrl, string description, string effects)
        {
            Id = id;
            Name = name;
            IconUrl = iconUrl;
            Description = description;
            Effects = effects;
        }
    }
}