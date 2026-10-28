using ArcaneMartialAscension.Spells;

namespace ArcaneMartialAscension.Core
{
    public sealed class SchoolOverview
    {
        public int KnownSpells {get;internal set;}
        public int HighestLearnedCircle {get;internal set;}=-1;
    }

    // Read-only projection: a catalog filter never changes a character's circle.
    public sealed class ArcaneOverview
    {
        public readonly SchoolOverview[] Schools=new SchoolOverview[8];
        public int KnownSpells {get;private set;}
        public int HighestLearnedCircle {get;private set;}=-1;
        public bool HasTraining {get;private set;}
        public ArcaneOverview(CharacterProgression progress)
        {
            for(int i=0;i<Schools.Length;i++)Schools[i]=new SchoolOverview();
            if(progress==null)return;
            foreach(string id in progress.Arcane.KnownSpellIds)
            {
                SpellEntry spell=SpellCatalog.Find(id);
                if(spell==null)continue;
                SchoolOverview school=Schools[(int)spell.School];
                school.KnownSpells++; KnownSpells++;
                if(spell.Circle>school.HighestLearnedCircle)school.HighestLearnedCircle=spell.Circle;
                if(spell.Circle>HighestLearnedCircle)HighestLearnedCircle=spell.Circle;
            }
            HasTraining=KnownSpells>0 || progress.Arcane.GetExperience(ArcaneTradition.Wizard)>0 ||
                progress.Arcane.GetExperience(ArcaneTradition.Warlock)>0 || progress.Arcane.GetExperience(ArcaneTradition.Sorcerer)>0;
        }
    }
}
