using System;
using System.Collections.Generic;
using ArcaneMartialAscension.Learning;

namespace ArcaneMartialAscension.Core
{
    public enum MagicSchool { Abjuration, Conjuration, Divination, Enchantment, Evocation, Illusion, Necromancy, Transmutation }
    public enum WeaponType { Sword, Spear, Bow, Axe }

    public sealed class CharacterProgression
    {
        private readonly Dictionary<MagicSchool, int> magicExperience = new Dictionary<MagicSchool, int>();
        private readonly Dictionary<WeaponType, int> weaponExperience = new Dictionary<WeaponType, int>();

        public ArcaneProgression Arcane { get; } = new ArcaneProgression();
        public LearningProgress Learning {get;}=new LearningProgress();
        private readonly Dictionary<string,KnowledgeOrigin> origins=new Dictionary<string,KnowledgeOrigin>(StringComparer.Ordinal);
        public KnowledgeOrigin GetOrigin(string id)
        {KnowledgeOrigin origin;return id!=null && origins.TryGetValue(id,out origin)?origin:null;}
        public void RecordOrigin(string id,KnowledgeOrigin origin)
        {if(!string.IsNullOrEmpty(id) && origin!=null && !origins.ContainsKey(id))origins.Add(id,origin);}

        public float Mana { get; private set; } = 100f;
        public float MaxMana { get; private set; } = 100f;
        public float Aura { get; private set; } = 0f;
        public float MaxAura { get; private set; } = 0f;
        public bool HasAwakenedAura { get; private set; }

        public int GetMagicExperience(MagicSchool school) => magicExperience.TryGetValue(school, out int xp) ? xp : 0;
        public int GetWeaponExperience(WeaponType weapon) => weaponExperience.TryGetValue(weapon, out int xp) ? xp : 0;

        public void StudyMagic(MagicSchool school, int xp)
        {
            if (xp <= 0) return;
            magicExperience[school] = Math.Min(1000000, GetMagicExperience(school) + xp);
        }

        public static readonly int[] WeaponXpThresholds = { 0, 30, 100, 250, 500, 900, 1500, 2500 };
        public int GetWeaponLevel(WeaponType weapon)
        {
            int xp = GetWeaponExperience(weapon);
            int level = 0;
            for (int i = 1; i < WeaponXpThresholds.Length; i++)
                if (xp >= WeaponXpThresholds[i]) level = i;
            return level;
        }
        public string GetWeaponRank(WeaponType weapon)
        {
            string[] ranks = { "Iniciante", "Aprendiz", "Praticante", "Veterano", "Especialista", "Mestre", "Grao-mestre", "Lenda" };
            return ranks[GetWeaponLevel(weapon)];
        }
        public void GrowMana(int circle)
        {
            MaxMana = 100f + Math.Max(0, Math.Min(9, circle)) * 35f;
            if (Mana > MaxMana) Mana = MaxMana;
        }
        public void TrainWeapon(WeaponType weapon, int xp)
        {
            if (xp <= 0) return;
            weaponExperience[weapon] = Math.Min(1000000, GetWeaponExperience(weapon) + xp);
        }

        public bool CastSpell(float manaCost)
        {
            if (float.IsNaN(manaCost) || float.IsInfinity(manaCost) || manaCost <= 0 || Mana < manaCost) return false;
            Mana -= manaCost;
            return true;
        }

        public bool CastKnownSpell(SpellDefinition spell)
        {
            if (!Arcane.CanCast(spell, Mana)) return false;
            return CastSpell(spell.ManaCost);
        }

        public void AwakenAura()
        {
            if (HasAwakenedAura) return;
            HasAwakenedAura = true;
            MaxAura = 50f;
            Aura = MaxAura;
        }

        public bool UseAura(float cost)
        {
            if (!HasAwakenedAura || float.IsNaN(cost) || float.IsInfinity(cost) || cost <= 0 || Aura < cost) return false;
            Aura -= cost;
            return true;
        }

        public void Regenerate(float seconds)
        {
            if (float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0) return;
            Mana = Math.Min(MaxMana, Mana + seconds * 2f);
            if (HasAwakenedAura) Aura = Math.Min(MaxAura, Aura + seconds);
        }
    }
}
