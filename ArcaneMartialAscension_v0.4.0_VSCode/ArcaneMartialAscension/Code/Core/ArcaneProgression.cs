using System;
using System.Collections.Generic;

namespace ArcaneMartialAscension.Core
{
    // Traditions are not exclusive classes: a character can develop all three.
    public enum ArcaneTradition { Wizard, Warlock, Sorcerer }
    public enum SorcerousOrigin { None, Draconic, Elemental, Fey, Wild }

    public sealed class SpellDefinition
    {
        public readonly string Id;
        public readonly MagicSchool School;
        public readonly int Circle;
        public readonly float ManaCost;
        public SpellDefinition(string id, MagicSchool school, int circle, float manaCost)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("Spell id is required");
            if (circle < 0 || circle > 9) throw new ArgumentOutOfRangeException("circle");
            if (float.IsNaN(manaCost) || float.IsInfinity(manaCost) || manaCost <= 0) throw new ArgumentOutOfRangeException("manaCost");
            Id = id; School = school; Circle = circle; ManaCost = manaCost;
        }
    }

    public sealed class ArcaneProgression
    {
        private readonly Dictionary<ArcaneTradition, int> experience = new Dictionary<ArcaneTradition, int>();
        private readonly HashSet<string> learnedSpells = new HashSet<string>(StringComparer.Ordinal);
        public SorcerousOrigin Origin { get; private set; } = SorcerousOrigin.None;
        public string PatronId { get; private set; }
        public int GetExperience(ArcaneTradition tradition) => experience.TryGetValue(tradition, out int value) ? value : 0;
        // Circle is a capability per tradition, not a permanent character class.
        public int GetCircle(ArcaneTradition tradition)
        {
            int xp = GetExperience(tradition);
            int[] thresholds = { 0, 100, 250, 500, 900, 1500, 2400, 3600, 5200, 7500 };
            int circle = 0;
            for (int i = 1; i < thresholds.Length; i++) if (xp >= thresholds[i]) circle = i;
            return circle;
        }
        public int HighestCircle => Math.Max(GetCircle(ArcaneTradition.Wizard), Math.Max(GetCircle(ArcaneTradition.Warlock), GetCircle(ArcaneTradition.Sorcerer)));
        public void Study(ArcaneTradition tradition, int xp)
        {
            if (xp <= 0) return;
            experience[tradition] = (int)Math.Min(1000000L, (long)GetExperience(tradition) + xp);
        }
        public void SetOrigin(SorcerousOrigin origin) { Origin = origin; }
        public void SetPatron(string patronId) { PatronId = string.IsNullOrWhiteSpace(patronId) ? null : patronId; }
        public bool CanLearn(SpellDefinition spell, ArcaneTradition tradition)
        {
            if (spell == null || GetCircle(tradition) < spell.Circle) return false;
            if (tradition == ArcaneTradition.Warlock && PatronId == null) return false;
            if (tradition == ArcaneTradition.Sorcerer && Origin == SorcerousOrigin.None) return false;
            return true;
        }
        public bool Learn(SpellDefinition spell, ArcaneTradition tradition)
        {
            return CanLearn(spell, tradition) && learnedSpells.Add(spell.Id);
        }
        public bool Knows(string spellId) => spellId != null && learnedSpells.Contains(spellId);
        public bool CanCast(SpellDefinition spell, float mana)
        {
            return spell != null && Knows(spell.Id) && HighestCircle >= spell.Circle && mana >= spell.ManaCost;
        }
    }
}
