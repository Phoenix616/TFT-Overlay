using System.Collections.Generic;

namespace TFT_Overlay.Data
{
    public class TftItem
    {
        public string Id { get; }
        public string Name { get; }
        public string IconUrl { get; }
        public string Description { get; }
        public ICollection<TftItem> Ingredient { get; } = new List<TftItem>();
        public ICollection<TftItem> Ingredients { get; } = new List<TftItem>();
        public ICollection<TftChampion> Champions { get; } = new List<TftChampion>();

        public TftItem(string id, string name, string iconUrl, string description)
        {
            Id = id;
            Name = name;
            IconUrl = iconUrl;
            Description = description;
        }
    }
}