using System;
using System.Globalization;

namespace ArcaneMartialAscension.Core
{
    // The same values feed native trait stats and the character panels.
    public struct TraitAttributes
    {
        public readonly float Health, Damage, Armor, AttackSpeed;
        public TraitAttributes(float health=0, float damage=0, float armor=0, float attackSpeed=0)
        { Health=health; Damage=damage; Armor=armor; AttackSpeed=attackSpeed; }

        public TraitAttributes Plus(TraitAttributes other)
        { return new TraitAttributes(Health+other.Health,Damage+other.Damage,Armor+other.Armor,AttackSpeed+other.AttackSpeed); }

        public string Describe()
        {
            string result="";
            Append(ref result,"Vida",Health); Append(ref result,"Dano",Damage);
            Append(ref result,"Armadura",Armor); Append(ref result,"Vel. de ataque",AttackSpeed);
            return result.Length==0?"Sem bônus de atributos":result;
        }
        private static void Append(ref string result,string name,float value)
        {
            if(value==0)return;
            if(result.Length>0)result+=" · ";
            result+=name+" "+(value>0?"+":"")+value.ToString("0.#",CultureInfo.InvariantCulture);
        }
    }

    public static class TraitAttributeCatalog
    {
        public const int MaxWeaponLevel=7;
        public static TraitAttributes Tradition(ArcaneTradition tradition)
        {
            switch(tradition)
            {
                case ArcaneTradition.Warlock:return new TraitAttributes(health:20,damage:4,armor:1);
                case ArcaneTradition.Sorcerer:return new TraitAttributes(health:15,damage:5,attackSpeed:1);
                default:return new TraitAttributes(health:10,damage:2,attackSpeed:3);
            }
        }
        public static TraitAttributes Weapon(WeaponType weapon)
        {
            switch(weapon)
            {
                case WeaponType.Spear:return new TraitAttributes(damage:2,armor:2,attackSpeed:2);
                case WeaponType.Bow:return new TraitAttributes(damage:2,attackSpeed:5);
                case WeaponType.Axe:return new TraitAttributes(health:10,damage:5);
                default:return new TraitAttributes(damage:3,armor:1,attackSpeed:4);
            }
        }
        // Only the highest earned rank is active, on top of the equipped discipline.
        public static TraitAttributes Rank(WeaponType weapon,int level)
        {
            int rank=Math.Max(0,Math.Min(MaxWeaponLevel,level));
            switch(weapon)
            {
                case WeaponType.Spear:return new TraitAttributes(damage:rank*1.5f,armor:rank,attackSpeed:rank);
                case WeaponType.Bow:return new TraitAttributes(damage:rank,attackSpeed:rank*3);
                case WeaponType.Axe:return new TraitAttributes(health:rank*5,damage:rank*3,armor:rank*.5f);
                default:return new TraitAttributes(damage:rank*2,armor:rank*.5f,attackSpeed:rank*1.5f);
            }
        }
        public static TraitAttributes School(MagicSchool school)
        {
            switch(school)
            {
                case MagicSchool.Abjuration:return new TraitAttributes(health:15,armor:4);
                case MagicSchool.Conjuration:return new TraitAttributes(health:10,damage:1,attackSpeed:2);
                case MagicSchool.Divination:return new TraitAttributes(armor:1,attackSpeed:6);
                case MagicSchool.Enchantment:return new TraitAttributes(damage:1,armor:1,attackSpeed:3);
                case MagicSchool.Evocation:return new TraitAttributes(damage:4);
                case MagicSchool.Illusion:return new TraitAttributes(armor:2,attackSpeed:4);
                case MagicSchool.Necromancy:return new TraitAttributes(health:25,damage:2);
                default:return new TraitAttributes(health:10,damage:2,armor:2);
            }
        }
    }
}
