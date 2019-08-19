using System.Collections.Generic;

namespace TFT_Overlay.Data
{
    public class TftChampion
    {
        public string Id { get; }
        public string Name { get; }
        public string IconUrl { get; }
        public ICollection<ITftSynergy> Synergies { get; }
        public string Cost { get; }
        public string Health { get; }
        public string Damage { get; }
        public string DPS { get; }
        public int Range { get; }
        public double Speed { get; }
        public int Armor { get; }
        public int MagicResistance { get; }
        public Spell Spell { get; }
        public bool PBE { get; }
        public ICollection<TftItem> RecommendedItems { get; } = new List<TftItem>();
        
        public TftChampion(string id, string name, string iconUrl, ICollection<ITftSynergy> synergies, string cost, string health, string damage, string dps, int range, double speed, int armor, int magicResistance, Spell spell, bool pbe)
        {
            Id = id;
            Name = name;
            IconUrl = iconUrl;
            Synergies = synergies;
            Cost = cost;
            Health = health;
            Damage = damage;
            DPS = dps;
            Range = range;
            Speed = speed;
            Armor = armor;
            MagicResistance = magicResistance;
            Spell = spell;
            PBE = pbe;
        }
        
    }

    public class Spell
    {
        public string Id { get; }
        public string Name { get; }
        public string IconUrl { get; }
        public string Description { get; }
        public string Effect { get; }
        public string Type { get; }
        public int Mana { get; }
        
        public Spell(string id, string name, string iconUrl, string description, string effect, string type, int mana)
        {
            Id = id;
            Name = name;
            IconUrl = iconUrl;
            Description = description;
            Effect = effect;
            Type = type;
            Mana = mana;
        }
    }
}