using ArcaneMartialAscension.Core;
using MartialWeaponType = ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.Martial
{
    public sealed class TechniqueEntry
    {
        public readonly string Name;
        public readonly int RequiredLevel;
        public readonly string Description;
        public readonly string Id;
        public readonly MartialWeaponType Weapon;
        public readonly bool HasRuntime;
        public readonly float Damage,Cooldown;

        public TechniqueEntry(MartialWeaponType weapon,string id,string name,int level,string description,float damage=0,float cooldown=0)
        {Weapon=weapon;Id="ama.technique."+id;Name=name;RequiredLevel=level;Description=description;Damage=damage;Cooldown=cooldown;HasRuntime=damage>0 && cooldown>0;}
    }

    // Only explicitly implemented techniques can be learned or executed.
    public static class TechniqueCatalog
    {
        private static readonly TechniqueEntry[][] entries = {
            new[] {
                new TechniqueEntry(MartialWeaponType.Sword,"sword.precise_cut","Corte preciso",1,"Reforça um acerto de espada com dano adicional.",6,3),
                new TechniqueEntry(MartialWeaponType.Sword,"sword.parry","Aparo",3,"Desviar o golpe e abrir espaço para reagir."),
                new TechniqueEntry(MartialWeaponType.Sword,"sword.combo","Sequência de cortes",5,"Encadear golpes com controle e ritmo."),
                new TechniqueEntry(MartialWeaponType.Sword,"sword.aura_wave","Onda de aura",7,"Projetar um corte de energia à distância.")
            },
            new[] {
                new TechniqueEntry(MartialWeaponType.Spear,"spear.precise_thrust","Estocada precisa",1,"Reforça um acerto de lança com dano adicional.",5,2.5f),
                new TechniqueEntry(MartialWeaponType.Spear,"spear.sweep","Varredura",3,"Controlar o espaço com um golpe amplo."),
                new TechniqueEntry(MartialWeaponType.Spear,"spear.charge","Investida",5,"Avançar e atacar com a lança."),
                new TechniqueEntry(MartialWeaponType.Spear,"spear.aura_lance","Lança de aura",7,"Projetar a força de uma estocada.")
            },
            new[] {
                new TechniqueEntry(MartialWeaponType.Bow,"bow.precise_shot","Tiro preciso",1,"Reforça o impacto de uma flecha que acertou o alvo.",4,3),
                new TechniqueEntry(MartialWeaponType.Bow,"bow.double_shot","Disparo duplo",3,"Soltar duas flechas em sequência."),
                new TechniqueEntry(MartialWeaponType.Bow,"bow.piercing_arrow","Flecha perfurante",5,"Concentrar a força em um disparo."),
                new TechniqueEntry(MartialWeaponType.Bow,"bow.arrow_rain","Chuva de flechas",7,"Cobrir uma região com disparos.")
            },
            new[] {
                new TechniqueEntry(MartialWeaponType.Axe,"axe.heavy_blow","Golpe pesado",1,"Reforça um acerto de machado com dano adicional.",8,4),
                new TechniqueEntry(MartialWeaponType.Axe,"axe.guard_break","Quebra-guarda",3,"Abrir a defesa de um adversário."),
                new TechniqueEntry(MartialWeaponType.Axe,"axe.spinning_cut","Corte giratório",5,"Executar um golpe amplo ao redor."),
                new TechniqueEntry(MartialWeaponType.Axe,"axe.aura_impact","Impacto de aura",7,"Liberar energia em um golpe pesado.")
            }
        };

        public static TechniqueEntry[] ForWeapon(MartialWeaponType weapon)
        {return entries[(int)weapon];}
    }
}
