using System;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Spells;
using MartialWeaponType=ArcaneMartialAscension.Core.WeaponType;

namespace ArcaneMartialAscension.Learning
{
    public enum KnowledgeSource { SelfStudy, Practice, Combat, Player }

    public sealed class KnowledgeOrigin
    {
        public readonly KnowledgeSource Source;
        public readonly double WorldTime;
        public readonly long ActorId;
        public KnowledgeOrigin(KnowledgeSource source,double worldTime,long actorId)
        {Source=source;WorldTime=worldTime;ActorId=actorId;}
        public string Description
        {
            get
            {
                switch(Source)
                {
                    case KnowledgeSource.SelfStudy:return "estudo próprio";
                    case KnowledgeSource.Practice:return "prática com a arma";
                    case KnowledgeSource.Combat:return "combate";
                    default:return "intervenção do jogador";
                }
            }
        }
    }

    public sealed class LearningProgress
    {
        internal bool Observed,WasAvailable,WasMagicEligible;
        internal double LastTime,StudyClock,SpellWork,PracticeClock;
        internal MartialWeaponType? LastWeapon;
        internal long LastCombatAction=long.MinValue;
        public string StudyingSpellId {get;internal set;}
        public string LastAction {get;internal set;}="Ainda sem sessões de aprendizado.";
        public double StudyProgress {get{return SpellWork;}}
        public void ResetObservation()
        {Observed=false;WasAvailable=false;WasMagicEligible=false;LastWeapon=null;}
    }

    public sealed class LearningContext
    {
        public double WorldTime;
        public long ActorId;
        public bool Eligible,Available,MagicEligible;
        public MartialWeaponType? Weapon;
    }

    public sealed class LearningOutcome
    {
        public SpellEntry LearnedSpell;
        public MartialWeaponType? PracticedWeapon;
        public int PracticeXp;
    }

    // Pure simulation rules. The first curriculum deliberately stops at circle 1.
    public static class LearningRules
    {
        public const double StudySessionSeconds=8;
        public const double PracticeSessionSeconds=10;
        public const double MaxObservedSeconds=10;
        public const int PracticeXpPerSession=2;
        public const int ArcaneAptitudePercent=12;
        private static readonly string[] Curriculum={
            "ama.spell.evocation.0.centelha_rubra",
            "ama.spell.evocation.0.estilhaco_glacial",
            "ama.spell.evocation.1.lanca_eletrica",
            "ama.spell.evocation.1.rajada_de_cinzas"
        };

        public static bool HasArcaneAptitude(long actorId)
        {
            if(actorId<=0)return false;
            uint value=unchecked((uint)(actorId^(actorId>>32)));
            value=unchecked((value^0xA3C59AC3u)*0x85EBCA6Bu);
            value^=value>>13;
            return value%100<ArcaneAptitudePercent;
        }

        public static string WeaponKnowledgeId(MartialWeaponType weapon)
        {return "ama.mastery."+weapon.ToString().ToLowerInvariant();}
        public static string WeaponName(MartialWeaponType weapon)
        {
            switch(weapon)
            {
                case MartialWeaponType.Spear:return "lança";
                case MartialWeaponType.Bow:return "arco";
                case MartialWeaponType.Axe:return "machado";
                default:return "espada";
            }
        }

        public static bool Learn(CharacterProgression progress,SpellEntry spell,KnowledgeSource source,double now,long actorId)
        {
            if(progress==null || spell==null || double.IsNaN(now) || double.IsInfinity(now))return false;
            SpellDefinition definition=new SpellDefinition(spell.Id,spell.School,spell.Circle,spell.Mana);
            if(!progress.Arcane.Learn(definition,ArcaneTradition.Wizard))return false;
            progress.RecordOrigin(spell.Id,new KnowledgeOrigin(source,now,actorId));
            return true;
        }

        public static bool AwardCombat(CharacterProgression progress,MartialWeaponType weapon,long actionToken,double now,long actorId)
        {
            if(progress==null || (int)weapon<0 || (int)weapon>3 || double.IsNaN(now) || double.IsInfinity(now) ||
                progress.GetWeaponLevel(weapon)>=7 || progress.Learning.LastCombatAction==actionToken)return false;
            progress.Learning.LastCombatAction=actionToken;
            progress.TrainWeapon(weapon,Math.Min(2,CharacterProgression.WeaponXpThresholds[7]-progress.GetWeaponExperience(weapon)));
            progress.RecordOrigin(WeaponKnowledgeId(weapon),new KnowledgeOrigin(KnowledgeSource.Combat,now,actorId));
            progress.Learning.LastAction="Praticou "+WeaponName(weapon)+" em combate.";
            return true;
        }

        private static SpellEntry NextSpell(CharacterProgression progress,bool requireCircle)
        {
            foreach(string id in Curriculum)
            {
                SpellEntry spell=SpellCatalog.Find(id);
                if(spell==null || spell.Circle>1 || !SpellCatalog.HasRuntime(spell) || progress.Arcane.Knows(id))continue;
                if(!requireCircle || progress.Arcane.GetCircle(ArcaneTradition.Wizard)>=spell.Circle)return spell;
            }
            return null;
        }

        public static LearningOutcome Tick(CharacterProgression progress,LearningContext context)
        {
            LearningOutcome result=new LearningOutcome();
            if(progress==null || context==null || double.IsNaN(context.WorldTime) || double.IsInfinity(context.WorldTime))return result;
            LearningProgress learning=progress.Learning;
            bool available=context.Eligible && context.Available;
            double elapsed=learning.Observed?context.WorldTime-learning.LastTime:0;
            bool continuing=learning.Observed && learning.WasAvailable && available;
            bool continuingMagic=continuing && learning.WasMagicEligible && context.MagicEligible;
            bool sameWeapon=learning.LastWeapon==context.Weapon;
            if(!sameWeapon)learning.PracticeClock=0;
            learning.Observed=true;learning.LastTime=context.WorldTime;
            learning.WasAvailable=available;learning.WasMagicEligible=context.MagicEligible;
            learning.LastWeapon=context.Weapon;
            if(elapsed<0)
            {learning.StudyClock=learning.PracticeClock=0;return result;}
            if(!continuing || elapsed<=0)return result;
            elapsed=Math.Min(elapsed,MaxObservedSeconds);
            bool studying=continuingMagic && NextSpell(progress,false)!=null;
            bool practicing=sameWeapon && context.Weapon.HasValue && progress.GetWeaponLevel(context.Weapon.Value)<7;
            // A character cannot spend the same interval twice on two activities.
            double workTime=studying && practicing?elapsed*.5:elapsed;
            if(studying)
            {
                learning.StudyClock+=workTime;
                if(learning.StudyClock>=StudySessionSeconds)
                {
                    learning.StudyClock-=StudySessionSeconds;
                    int room=Math.Max(0,ArcaneProgression.CircleXpThresholds[2]-1-progress.Arcane.GetExperience(ArcaneTradition.Wizard));
                    progress.Arcane.Study(ArcaneTradition.Wizard,Math.Min(5,room));
                    progress.StudyMagic(MagicSchool.Evocation,5);
                    SpellEntry spell=NextSpell(progress,true);
                    if(spell!=null)
                    {
                        if(learning.StudyingSpellId!=spell.Id){learning.StudyingSpellId=spell.Id;learning.SpellWork=0;}
                        learning.SpellWork+=StudySessionSeconds;
                        learning.LastAction="Estudando "+spell.Name+" ("+(int)learning.SpellWork+" / "+(spell.Circle==0?32:64)+").";
                        if(learning.SpellWork>=(spell.Circle==0?32:64) && Learn(progress,spell,KnowledgeSource.SelfStudy,context.WorldTime,context.ActorId))
                        {
                            result.LearnedSpell=spell;
                            learning.LastAction="Aprendeu "+spell.Name+" por estudo próprio.";
                            learning.StudyingSpellId=null;learning.SpellWork=0;
                        }
                    }
                    else learning.LastAction="Estudando fundamentos para alcançar o círculo 1.";
                }
            }
            if(practicing)
            {
                learning.PracticeClock+=workTime;
                if(learning.PracticeClock>=PracticeSessionSeconds)
                {
                    learning.PracticeClock-=PracticeSessionSeconds;
                    MartialWeaponType weapon=context.Weapon.Value;
                    int xp=Math.Min(PracticeXpPerSession,CharacterProgression.WeaponXpThresholds[7]-progress.GetWeaponExperience(weapon));
                    progress.TrainWeapon(weapon,xp);
                    progress.RecordOrigin(WeaponKnowledgeId(weapon),new KnowledgeOrigin(KnowledgeSource.Practice,context.WorldTime,context.ActorId));
                    learning.LastAction="Treinou "+WeaponName(weapon)+" com a arma equipada.";
                    result.PracticedWeapon=weapon;result.PracticeXp=xp;
                }
            }
            return result;
        }
    }
}
