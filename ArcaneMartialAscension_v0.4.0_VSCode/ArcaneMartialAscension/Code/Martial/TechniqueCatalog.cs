using ArcaneMartialAscension.Core;
using MartialWeaponType = ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.Martial
{
    public sealed class TechniqueEntry
    {
        public readonly string Name;
        public readonly int RequiredLevel;
        public readonly string Description;

        public TechniqueEntry(string name,int level,string description)
        {Name=name;RequiredLevel=level;Description=description;}
    }

    // Preview of the martial roadmap. These techniques have no combat executor yet.
    public static class TechniqueCatalog
    {
        private static readonly TechniqueEntry[][] entries = {
            new[] {
                new TechniqueEntry("Corte preciso",1,"Um golpe direcionado com a espada."),
                new TechniqueEntry("Aparo",3,"Desviar o golpe e abrir espaço para reagir."),
                new TechniqueEntry("Sequência de cortes",5,"Encadear golpes com controle e ritmo."),
                new TechniqueEntry("Onda de aura",7,"Projetar um corte de energia à distância.")
            },
            new[] {
                new TechniqueEntry("Estocada precisa",1,"Concentrar o ataque na ponta da lança."),
                new TechniqueEntry("Varredura",3,"Controlar o espaço com um golpe amplo."),
                new TechniqueEntry("Investida",5,"Avançar e atacar com a lança."),
                new TechniqueEntry("Lança de aura",7,"Projetar a força de uma estocada.")
            },
            new[] {
                new TechniqueEntry("Tiro preciso",1,"Preparar um disparo cuidadoso."),
                new TechniqueEntry("Disparo duplo",3,"Soltar duas flechas em sequência."),
                new TechniqueEntry("Flecha perfurante",5,"Concentrar a força em um disparo."),
                new TechniqueEntry("Chuva de flechas",7,"Cobrir uma região com disparos.")
            },
            new[] {
                new TechniqueEntry("Golpe pesado",1,"Aplicar o peso do machado no ataque."),
                new TechniqueEntry("Quebra-guarda",3,"Abrir a defesa de um adversário."),
                new TechniqueEntry("Corte giratório",5,"Executar um golpe amplo ao redor."),
                new TechniqueEntry("Impacto de aura",7,"Liberar energia em um golpe pesado.")
            }
        };

        public static TechniqueEntry[] ForWeapon(MartialWeaponType weapon)
        {return entries[(int)weapon];}
    }
}
