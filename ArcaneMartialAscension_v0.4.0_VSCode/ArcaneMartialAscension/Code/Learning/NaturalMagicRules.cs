using System;
using ArcaneMartialAscension.Core;

namespace ArcaneMartialAscension.Learning
{
    public sealed class ArcaneFormation
    {
        public ArcaneTradition? Tradition {get;internal set;}
        public bool Ready {get;internal set;}
        public double Work {get;internal set;}
        internal bool Observed,Available;
        internal double LastTime;
        public void ResetObservation(){Observed=false;Available=false;}
    }

    public sealed class FormationContext
    {
        public long ActorId;
        public double WorldTime;
        public bool Eligible,Available;
        public ArcaneTradition? ExistingTradition;
    }

    // Initial autonomous events, not the future institution/lineage/patron simulation.
    public static class NaturalMagicRules
    {
        public static uint AptitudeHash(long actorId)
        {
            uint value=unchecked((uint)(actorId^(actorId>>32)));
            value=unchecked((value^0xA3C59AC3u)*0x85EBCA6Bu);
            return value^(value>>13);
        }
        public static ArcaneTradition? Vocation(long actorId)
        {
            if(actorId<=0)return null;
            uint roll=AptitudeHash(actorId)%100;
            return roll<7?ArcaneTradition.Wizard:roll<9?ArcaneTradition.Warlock:roll<12?(ArcaneTradition?)ArcaneTradition.Sorcerer:null;
        }
        public static string Name(ArcaneTradition tradition)
        {return tradition==ArcaneTradition.Warlock?"Bruxo":tradition==ArcaneTradition.Sorcerer?"Feiticeiro":"Mago";}
        public static double RequiredWork(ArcaneTradition tradition)
        {return tradition==ArcaneTradition.Warlock?64:tradition==ArcaneTradition.Sorcerer?24:0;}
        public static KnowledgeSource Source(ArcaneTradition tradition)
        {return tradition==ArcaneTradition.Warlock?KnowledgeSource.Pact:tradition==ArcaneTradition.Sorcerer?KnowledgeSource.Awakening:KnowledgeSource.SelfStudy;}
        public static string PatronName(string id)
        {ArcaneIdentity identity=ArcaneIdentities.Patron(id);return identity!=null?identity.Name:id??"Sem pacto";}
        public static string OriginName(SorcerousOrigin origin)
        {
            ArcaneIdentity identity=ArcaneIdentities.Lineage(origin);return identity!=null?identity.Name:"Não despertada";
        }
        public static string PatronFor(long actorId)
        {return new[]{"ama.patron.fiend","ama.patron.archfey","ama.patron.celestial","ama.patron.great_old_one"}[(AptitudeHash(actorId)>>8)%4];}
        public static SorcerousOrigin OriginFor(long actorId)
        {return new[]{SorcerousOrigin.Draconic,SorcerousOrigin.Wild,SorcerousOrigin.Aberrant,SorcerousOrigin.Clockwork}[(AptitudeHash(actorId)>>8)%4];}
        public static bool Observe(CharacterProgression progress,FormationContext context)
        {
            if(progress==null || context==null || context.ActorId<=0 || double.IsNaN(context.WorldTime) || double.IsInfinity(context.WorldTime))return false;
            ArcaneFormation formation=progress.Formation;
            ArcaneTradition? choice=formation.Tradition??context.ExistingTradition??Vocation(context.ActorId);
            if(choice.HasValue && ((int)choice.Value<0 || (int)choice.Value>2))return false;
            double elapsed=formation.Observed?context.WorldTime-formation.LastTime:0;
            bool available=context.Eligible && context.Available;
            bool continuing=formation.Observed && formation.Available && available;
            formation.Observed=true;formation.LastTime=context.WorldTime;formation.Available=available;
            if(!context.Eligible || !choice.HasValue || formation.Ready)return false;
            formation.Tradition=choice;
            if(elapsed<0){formation.Work=0;return false;}
            if(!available)return false;
            ArcaneTradition tradition=choice.Value;
            if(continuing && elapsed>0)formation.Work+=Math.Min(elapsed,LearningRules.MaxObservedSeconds);
            if(formation.Work<RequiredWork(tradition))return false;
            if(tradition==ArcaneTradition.Warlock && progress.Arcane.PatronId==null)
                progress.Arcane.SetPatron(PatronFor(context.ActorId));
            if(tradition==ArcaneTradition.Sorcerer && progress.Arcane.Origin==SorcerousOrigin.None)
                progress.Arcane.SetOrigin(OriginFor(context.ActorId));
            formation.Ready=true;
            progress.RecordOrigin("ama.tradition."+tradition.ToString().ToLowerInvariant(),new KnowledgeOrigin(Source(tradition),context.WorldTime,context.ActorId));
            progress.Learning.LastAction=tradition==ArcaneTradition.Warlock?"Firmou um pacto com "+PatronName(progress.Arcane.PatronId)+".":
                tradition==ArcaneTradition.Sorcerer?"Despertou a origem "+OriginName(progress.Arcane.Origin)+".":"Iniciou os estudos de Mago.";
            return true;
        }
        public static string Status(CharacterProgression progress)
        {
            if(progress==null || !progress.Formation.Tradition.HasValue)return "Sem formação arcana.";
            ArcaneFormation formation=progress.Formation;
            ArcaneTradition tradition=formation.Tradition.Value;
            if(!formation.Ready)return (tradition==ArcaneTradition.Warlock?"Preparando ritual de pacto": "Despertando poder inato")+" · "+(int)formation.Work+" / "+RequiredWork(tradition);
            return Name(tradition)+(tradition==ArcaneTradition.Warlock?" · "+PatronName(progress.Arcane.PatronId):
                tradition==ArcaneTradition.Sorcerer?" · origem "+OriginName(progress.Arcane.Origin):" · estudo próprio");
        }
    }
}
