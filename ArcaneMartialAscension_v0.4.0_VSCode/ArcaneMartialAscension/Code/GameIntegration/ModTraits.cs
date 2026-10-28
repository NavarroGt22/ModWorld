using System;
using UnityEngine;

namespace ArcaneMartialAscension.GameIntegration
{
    // Real ActorTrait registration: registered traits appear in character info/trait editor.
    public static class ModTraits
    {
        public const string Wizard = "ama_wizard";
        public const string Warlock = "ama_warlock";
        public const string Sorcerer = "ama_sorcerer";
        public const string Sword = "ama_sword_master";
        public const string Spear = "ama_spear_master";
        public const string Bow = "ama_bow_master";
        public const string Axe = "ama_axe_master";
        public const string SpellShield = "ama_spell_shield";
        public const string SpellBoost = "ama_spell_boost";
        public const string SpellFocus = "ama_spell_focus";
        public const string SpellIllusion = "ama_spell_illusion";
        public const string SpellWeaken = "ama_spell_weaken";
        public const string Aura = "ama_aura_awakened";
        public const string Evocation = "ama_school_evocation";
        private const string GroupArcane = "ama_arcane";
        private const string GroupMartial = "ama_martial";
        private const string GroupSchools = "ama_schools";

        public static void Register()
        {
            AddGroup(GroupArcane, new Color(0.40f, 0.65f, 1f));
            AddGroup(GroupMartial, new Color(1f, 0.72f, 0.35f));
            AddGroup(GroupSchools, new Color(0.72f, 0.49f, 1f));

            ActorTrait wizard = Create(Wizard, GroupArcane, "wizard", 4f);
            GameCompat.AddStat(wizard, "damage", 3f);
            wizard.action_attack_target = new AttackAction(ActorAbilities.OnWizardAttack);
            RegisterTrait(wizard);

            ActorTrait warlock = Create(Warlock, GroupArcane, "warlock", 2f);
            GameCompat.AddStat(warlock, "damage", 4f);
            warlock.action_attack_target = new AttackAction(ActorAbilities.OnWarlockAttack);
            RegisterTrait(warlock);

            ActorTrait sorcerer = Create(Sorcerer, GroupArcane, "sorcerer", 3f);
            GameCompat.AddStat(sorcerer, "damage", 4f);
            sorcerer.action_attack_target = new AttackAction(ActorAbilities.OnSorcererAttack);
            RegisterTrait(sorcerer);

            ActorTrait sword = Create(Sword, GroupMartial, "sword", 10f);
            GameCompat.AddStat(sword, "damage", 5f);
            GameCompat.AddStat(sword, "attack_speed", 8f);
            sword.action_attack_target = new AttackAction(ActorAbilities.OnSwordAttack);
            RegisterTrait(sword);

            ActorTrait spear = Create(Spear, GroupMartial, "spear", 6f);
            GameCompat.AddStat(spear, "damage", 2f);
            spear.action_attack_target = new AttackAction(ActorAbilities.OnSpearAttack);
            RegisterTrait(spear);
            ActorTrait bow = Create(Bow, GroupMartial, "bow", 6f);
            GameCompat.AddStat(bow, "attack_speed", 5f);
            bow.action_attack_target = new AttackAction(ActorAbilities.OnBowAttack);
            RegisterTrait(bow);
            ActorTrait axe = Create(Axe, GroupMartial, "axe", 6f);
            GameCompat.AddStat(axe, "damage", 4f);
            axe.action_attack_target = new AttackAction(ActorAbilities.OnAxeAttack);
            RegisterTrait(axe);
            // Rank traits are granted only after qualifying XP, and remain visible.
            for (int type = 0; type < 4; type++)
            {
                string[] ids = { Sword, Spear, Bow, Axe };
                string[] icons = { "sword", "spear", "bow", "axe" };
                for (int rank = 1; rank <= 3; rank++)
                {
                    ActorTrait levelTrait = Create(RankId((Core.WeaponType)type, rank), GroupMartial, icons[type], 0f);
                    GameCompat.AddStat(levelTrait, "damage", rank * 2f);
                    GameCompat.AddStat(levelTrait, "attack_speed", rank * 1.5f);
                    RegisterTrait(levelTrait);
                }
            }
            ActorTrait protectedTrait = Create(SpellShield, GroupArcane, "abjuration", 0f);
            GameCompat.AddStat(protectedTrait, "armor", 8f);
            RegisterTrait(protectedTrait);
            ActorTrait boostedTrait = Create(SpellBoost, GroupArcane, "transmutation", 0f);
            GameCompat.AddStat(boostedTrait, "damage", 5f);
            RegisterTrait(boostedTrait);

            ActorTrait focus=Create(SpellFocus,GroupArcane,"divination",0f);
            GameCompat.AddStat(focus,"attack_speed",7f);
            RegisterTrait(focus);
            ActorTrait mirage=Create(SpellIllusion,GroupArcane,"illusion",0f);
            GameCompat.AddStat(mirage,"armor",6f);
            RegisterTrait(mirage);
            ActorTrait weaken=Create(SpellWeaken,GroupArcane,"enchantment",0f);
            GameCompat.AddStat(weaken,"damage",-5f);
            RegisterTrait(weaken);

            ActorTrait aura = Create(Aura, GroupMartial, "aura", 3f);
            GameCompat.AddStat(aura, "armor", 6f);
            GameCompat.AddStat(aura, "health", 30f);
            aura.action_special_effect = new WorldAction(ActorAbilities.OnAuraTick);
            RegisterTrait(aura);

            AddSchool("ama_school_abjuration", "abjuration", 1.6f, 0, 3, 0);
            AddSchool("ama_school_conjuration", "conjuration", 1.6f, 1, 0, 5);
            AddSchool("ama_school_divination", "divination", 1.6f, 0, 0, 8);
            AddSchool("ama_school_enchantment", "enchantment", 1.6f, 1, 0, 4);
            AddSchool(Evocation, "evocation", 2.5f, 3, 0, 0);
            AddSchool("ama_school_illusion", "illusion", 1.6f, 0, 1, 4);
            AddSchool("ama_school_necromancy", "necromancy", 1.6f, 2, 0, 0);
            AddSchool("ama_school_transmutation", "transmutation", 1.6f, 1, 2, 0);
        }

        public static string RankId(Core.WeaponType weapon, int tier)
        {
            return "ama_mastery_" + weapon.ToString().ToLowerInvariant() + "_" + tier;
        }
        public static string WeaponTrait(Core.WeaponType weapon)
        {
            switch (weapon)
            {
                case Core.WeaponType.Spear: return Spear;
                case Core.WeaponType.Bow: return Bow;
                case Core.WeaponType.Axe: return Axe;
                default: return Sword;
            }
        }
        public static string SchoolTrait(Core.MagicSchool school)
        {
            return "ama_school_" + school.ToString().ToLowerInvariant();
        }
        private static void AddGroup(string id, Color color)
        {
            if (AssetManager.trait_groups.get(id) != null) return;
            AssetManager.trait_groups.add(new ActorTraitGroupAsset
            {
                id = id, name = "trait_group_" + id, color = "#" + ColorUtility.ToHtmlStringRGB(color)
            });
        }

        private static ActorTrait Create(string id, string group, string icon, float birthChance)
        {
            ActorTrait trait = new ActorTrait
            {
                id = id,
                group_id = group,
                path_icon = "ui/icons/ama/" + icon
            };
            GameCompat.SetBirthChanceIfAvailable(trait, birthChance);
            return trait;
        }

        private static void AddSchool(string id, string icon, float birthChance, int damage, int armor, int attackSpeed)
        {
            ActorTrait trait = Create(id, GroupSchools, icon, birthChance);
            if (damage != 0) GameCompat.AddStat(trait, "damage", damage);
            if (armor != 0) GameCompat.AddStat(trait, "armor", armor);
            if (attackSpeed != 0) GameCompat.AddStat(trait, "attack_speed", attackSpeed);
            RegisterTrait(trait);
        }

        private static void RegisterTrait(ActorTrait trait)
        {
            if (AssetManager.traits.get(trait.id) == null)
                AssetManager.traits.add(trait);
            // Make it accessible in the trait editor as well as character details.
            // API versions differ: unlock via reflection so a removed optional
            // editor helper cannot prevent compilation/loading of every trait.
            try
            {
                System.Reflection.MethodInfo unlock = typeof(PlayerConfig).GetMethod(
                    "unlockTrait", System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static);
                if (unlock != null) unlock.Invoke(null, new object[] { trait.id });
            }
            catch (Exception error)
            {
                Debug.LogWarning("[AMA] Could not unlock trait in editor: " + error.Message);
            }
        }
    }
}
