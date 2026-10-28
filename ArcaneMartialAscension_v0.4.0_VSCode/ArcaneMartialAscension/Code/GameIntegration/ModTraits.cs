using System;
using UnityEngine;
using ArcaneMartialAscension.Core;

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
        private const string GroupRanks = "ama_mastery_ranks";
        private const string GroupIdentities="ama_arcane_identities";
        private static int registeredCount;

        public static void Register()
        {
            registeredCount=0;
            AddGroup(GroupArcane, new Color(0.40f, 0.65f, 1f));
            AddGroup(GroupMartial, new Color(1f, 0.72f, 0.35f));
            AddGroup(GroupSchools, new Color(0.72f, 0.49f, 1f));
            AddGroup(GroupRanks, new Color(1f,0.55f,0.25f));
            AddGroup(GroupIdentities,new Color(.75f,.55f,1f));

            ActorTrait wizard = Create(Wizard, GroupArcane, "wizard");
            ApplyAttributes(wizard,TraitAttributeCatalog.Tradition(ArcaneTradition.Wizard));
            wizard.action_attack_target = new AttackAction(ActorAbilities.OnWizardAttack);
            RegisterTrait(wizard);

            ActorTrait warlock = Create(Warlock, GroupArcane, "warlock");
            ApplyAttributes(warlock,TraitAttributeCatalog.Tradition(ArcaneTradition.Warlock));
            warlock.action_attack_target = new AttackAction(ActorAbilities.OnWarlockAttack);
            RegisterTrait(warlock);

            ActorTrait sorcerer = Create(Sorcerer, GroupArcane, "sorcerer");
            ApplyAttributes(sorcerer,TraitAttributeCatalog.Tradition(ArcaneTradition.Sorcerer));
            sorcerer.action_attack_target = new AttackAction(ActorAbilities.OnSorcererAttack);
            RegisterTrait(sorcerer);

            ActorTrait sword = Create(Sword, GroupMartial, "sword");
            ApplyAttributes(sword,TraitAttributeCatalog.Weapon(Core.WeaponType.Sword));
            sword.action_attack_target = new AttackAction(ActorAbilities.OnSwordAttack);
            RegisterTrait(sword);

            ActorTrait spear = Create(Spear, GroupMartial, "spear");
            ApplyAttributes(spear,TraitAttributeCatalog.Weapon(Core.WeaponType.Spear));
            spear.action_attack_target = new AttackAction(ActorAbilities.OnSpearAttack);
            RegisterTrait(spear);
            ActorTrait bow = Create(Bow, GroupMartial, "bow");
            ApplyAttributes(bow,TraitAttributeCatalog.Weapon(Core.WeaponType.Bow));
            bow.action_attack_target = new AttackAction(ActorAbilities.OnBowAttack);
            RegisterTrait(bow);
            ActorTrait axe = Create(Axe, GroupMartial, "axe");
            ApplyAttributes(axe,TraitAttributeCatalog.Weapon(Core.WeaponType.Axe));
            axe.action_attack_target = new AttackAction(ActorAbilities.OnAxeAttack);
            RegisterTrait(axe);
            // Rank traits are granted only after qualifying XP, and remain visible.
            for (int type = 0; type < 4; type++)
            {
                string[] icons = { "sword", "spear", "bow", "axe" };
                for (int rank = 1; rank <= TraitAttributeCatalog.MaxWeaponLevel; rank++)
                {
                    ActorTrait levelTrait = Create(RankId((Core.WeaponType)type, rank), GroupRanks, icons[type]);
                    ApplyAttributes(levelTrait,TraitAttributeCatalog.Rank((Core.WeaponType)type,rank));
                    RegisterTrait(levelTrait);
                }
            }
            ActorTrait protectedTrait = Create(SpellShield, GroupArcane, "abjuration");
            ApplyAttributes(protectedTrait,new TraitAttributes(armor:8));
            RegisterTrait(protectedTrait);
            ActorTrait boostedTrait = Create(SpellBoost, GroupArcane, "transmutation");
            ApplyAttributes(boostedTrait,new TraitAttributes(damage:5));
            RegisterTrait(boostedTrait);

            ActorTrait focus=Create(SpellFocus,GroupArcane,"divination");
            ApplyAttributes(focus,new TraitAttributes(attackSpeed:7));
            RegisterTrait(focus);
            ActorTrait mirage=Create(SpellIllusion,GroupArcane,"illusion");
            ApplyAttributes(mirage,new TraitAttributes(armor:6));
            RegisterTrait(mirage);
            ActorTrait weaken=Create(SpellWeaken,GroupArcane,"enchantment");
            ApplyAttributes(weaken,new TraitAttributes(damage:-5));
            RegisterTrait(weaken);

            ActorTrait aura = Create(Aura, GroupMartial, "aura");
            ApplyAttributes(aura,new TraitAttributes(health:30,armor:6));
            aura.action_special_effect = new WorldAction(ActorAbilities.OnAuraTick);
            RegisterTrait(aura);

            for(int school=0;school<8;school++)AddSchool((MagicSchool)school);
            foreach(ArcaneIdentity identity in ArcaneIdentities.All)
            {
                ActorTrait trait=Create(identity.TraitId,GroupIdentities,identity.Icon);
                ApplyAttributes(trait,identity.Attributes);RegisterTrait(trait);
            }
            Debug.Log("[AMA Stats] "+registeredCount+" traços registrados com atributos nativos verificados.");
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

        private static ActorTrait Create(string id, string group, string icon)
        {
            ActorTrait trait = new ActorTrait
            {
                id = id,
                group_id = group,
                path_icon = "ui/icons/ama/" + icon
            };
            GameCompat.ConfigureProgressionTrait(trait);
            return trait;
        }

        private static void AddSchool(MagicSchool school)
        {
            ActorTrait trait=Create(SchoolTrait(school),GroupSchools,school.ToString().ToLowerInvariant());
            ApplyAttributes(trait,TraitAttributeCatalog.School(school));
            RegisterTrait(trait);
        }

        private static void ApplyAttributes(ActorTrait trait,TraitAttributes attributes)
        {
            AddRequiredStat(trait,"health",attributes.Health);
            AddRequiredStat(trait,"damage",attributes.Damage);
            AddRequiredStat(trait,"armor",attributes.Armor);
            AddRequiredStat(trait,"attack_speed",attributes.AttackSpeed);
        }
        private static void AddRequiredStat(ActorTrait trait,string stat,float value)
        {
            if(value!=0 && !GameCompat.AddStat(trait,stat,value))
                throw new InvalidOperationException("Não foi possível registrar "+stat+" no traço "+trait.id);
        }

        private static void RegisterTrait(ActorTrait trait)
        {
            if (AssetManager.traits.get(trait.id) == null)
                AssetManager.traits.add(trait);
            registeredCount++;
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
