using System;
using System.Linq;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Learning;
using ArcaneMartialAscension.Spells;

internal static class NaturalMagicTests
{
    private static void Assert(bool ok,string message){if(!ok)throw new Exception(message);}
    private static FormationContext Context(ArcaneTradition? tradition,long id=42)
    {return new FormationContext{ActorId=id,Eligible=true,Available=true,ExistingTradition=tradition};}
    private static void Advance(CharacterProgression p,FormationContext c,int from,int to)
    {for(int i=from;i<=to;i++){c.WorldTime=i;NaturalMagicRules.Observe(p,c);}}
    private sealed class Effect : ISpellEffectWorld
    {public SpellEffectOutcome Apply(SpellEffect effect){return new SpellEffectOutcome{Applied=true,HealthLost=8,TargetsAffected=1};}}
    private static SpellCastRequest Request(SpellEntry spell)
    {return new SpellCastRequest{Spell=spell,CasterId=42,TargetId=43,CasterAlive=true,TargetAlive=true,HasSchool=true,Relation=SpellTargetRelation.Enemy,Distance=1,WorldTime=1000};}
    public static void Run(Action<string,Action> check)
    {
        check("natural: vocation is stable and preserves total twelve-percent aptitude",()=>{
            int[] counts=new int[3];
            for(long id=1;id<=10000;id++)
            {var v=NaturalMagicRules.Vocation(id);Assert(v==NaturalMagicRules.Vocation(id),"Rerolled");
                Assert(v.HasValue==LearningRules.HasArcaneAptitude(id),"Aptitude changed");if(v.HasValue)counts[(int)v.Value]++;}
            Assert(counts[0]>550 && counts[0]<850 && counts[1]>120 && counts[1]<280 && counts[2]>200 && counts[2]<400,"Distribution outside broad bounds");
            Assert(!NaturalMagicRules.Vocation(0).HasValue && !NaturalMagicRules.Vocation(-1).HasValue,"Invalid ID");
        });
        check("natural: ineligible and unavailable observations cannot form a class",()=>{
            var p=new CharacterProgression();var c=Context(ArcaneTradition.Sorcerer);c.Eligible=false;Advance(p,c,0,100);
            Assert(!p.Formation.Tradition.HasValue && !p.Formation.Ready,"Ineligible formation");
            c.Eligible=true;c.Available=false;Advance(p,c,101,200);Assert(!p.Formation.Ready && p.Formation.Work==0,"Rest formed a class");
        });
        check("natural: sorcerer awakens only after twenty-four observed units",()=>{
            var p=new CharacterProgression();var c=Context(ArcaneTradition.Sorcerer);Advance(p,c,0,23);
            Assert(!p.Formation.Ready && p.Arcane.Origin==SorcerousOrigin.None,"Early origin");Advance(p,c,24,24);
            Assert(p.Formation.Ready && p.Arcane.Origin!=SorcerousOrigin.None && p.Arcane.PatronId==null,"Origin missing");
            Assert(!p.Arcane.KnownSpellIds.Any() && p.Arcane.HighestCircle==0,"Free spell or XP");
            Assert(p.GetOrigin("ama.tradition.sorcerer").Source==KnowledgeSource.Awakening,"Missing awakening source");
        });
        check("natural: warlock ritual requires sixty-four observed units and records pact",()=>{
            var p=new CharacterProgression();var c=Context(ArcaneTradition.Warlock);Advance(p,c,0,63);
            Assert(p.Arcane.PatronId==null && !p.Formation.Ready,"Instant patron");Advance(p,c,64,64);
            Assert(p.Formation.Ready && p.Arcane.PatronId!=null && !p.Arcane.KnownSpellIds.Any(),"Missing ritual or free spell");
            Assert(p.GetOrigin("ama.tradition.warlock").ActorId==42 && p.GetOrigin("ama.tradition.warlock").Source==KnowledgeSource.Pact,"Wrong pact owner");
        });
        check("natural: preexisting origin and patron survive formation",()=>{
            var p=new CharacterProgression();p.Arcane.SetPatron("existing.patron");Advance(p,Context(ArcaneTradition.Warlock),0,100);
            Assert(p.Arcane.PatronId=="existing.patron","Patron overwritten");
            var q=new CharacterProgression();q.Arcane.SetOrigin(SorcerousOrigin.Fey);Advance(q,Context(ArcaneTradition.Sorcerer),0,50);
            Assert(q.Arcane.Origin==SorcerousOrigin.Fey,"Origin overwritten");
        });
        check("natural: formation completion cannot repeat or reassign the path",()=>{
            var p=new CharacterProgression();var c=Context(ArcaneTradition.Warlock);Advance(p,c,0,64);string patron=p.Arcane.PatronId;
            c.ExistingTradition=ArcaneTradition.Sorcerer;c.WorldTime=100;
            Assert(!NaturalMagicRules.Observe(p,c) && p.Formation.Tradition==ArcaneTradition.Warlock && p.Arcane.PatronId==patron && p.Arcane.Origin==SorcerousOrigin.None,"Reassigned powers");
        });
        check("natural: same clock and long gaps cannot complete a ritual",()=>{
            var p=new CharacterProgression();var c=Context(ArcaneTradition.Warlock);
            for(int i=0;i<1000;i++)NaturalMagicRules.Observe(p,c);
            Assert(p.Formation.Work==0 && !p.Formation.Ready,"Paused work");c.WorldTime=10000;NaturalMagicRules.Observe(p,c);
            Assert(p.Formation.Work==10 && !p.Formation.Ready,"Unbounded catch-up");
        });
        check("natural: sleep and disabled intervals award no retroactive formation",()=>{
            var p=new CharacterProgression();var c=Context(ArcaneTradition.Sorcerer);Advance(p,c,0,10);c.Available=false;Advance(p,c,11,100);
            c.Available=true;Advance(p,c,101,105);Assert(p.Formation.Work==14,"Sleep credited");
            p.Formation.ResetObservation();c.WorldTime=5000;NaturalMagicRules.Observe(p,c);Assert(p.Formation.Work==14,"Disabled gap credited");
        });
        check("natural: invalid time and clock reversal cannot create powers",()=>{
            var p=new CharacterProgression();var c=Context(ArcaneTradition.Warlock);c.WorldTime=double.NaN;
            Assert(!NaturalMagicRules.Observe(p,c) && !p.Formation.Tradition.HasValue,"NaN mutated");Advance(p,c,0,30);c.WorldTime=1;
            NaturalMagicRules.Observe(p,c);Assert(!p.Formation.Ready && p.Formation.Work==0,"Reversal credited");
        });
        check("natural: warlocks and sorcerers cannot study before their actual source exists",()=>{
            foreach(var tradition in new[]{ArcaneTradition.Warlock,ArcaneTradition.Sorcerer})
            {var p=new CharacterProgression();var c=new LearningContext{ActorId=42,Eligible=true,Available=true,MagicEligible=true,Tradition=tradition};
                for(int i=0;i<500;i++){c.WorldTime=i;LearningRules.Tick(p,c);}Assert(!p.Arcane.KnownSpellIds.Any() && p.Arcane.GetExperience(tradition)==0,"Source bypassed");}
        });
        check("natural: each tradition learns and automatically casts without Wizard XP",()=>{
            foreach(ArcaneTradition tradition in Enum.GetValues(typeof(ArcaneTradition)))
            {
                var p=new CharacterProgression();var f=Context(tradition);var l=new LearningContext{ActorId=42,Eligible=true,Available=true,Tradition=tradition};
                for(int i=0;i<=800;i++)
                {f.WorldTime=i;NaturalMagicRules.Observe(p,f);l.WorldTime=i;l.MagicEligible=p.Formation.Ready;LearningRules.Tick(p,l);}
                Assert(p.Arcane.KnownSpellIds.Count()==4 && p.Arcane.GetCircle(tradition)==1,"Curriculum incomplete: "+tradition);
                Assert(p.Arcane.KnownSpellIds.All(id=>p.Arcane.TraditionFor(id)==tradition),"Wrong spell tradition");
                if(tradition!=ArcaneTradition.Wizard)Assert(p.Arcane.GetExperience(ArcaneTradition.Wizard)==0,"Study created Wizard XP");
                var session=new SpellCastingSession(p);SpellEntry spell=SpellAiRules.Choose(p,session,Request);Assert(spell!=null,"AI chose nothing");
                int before=p.Arcane.GetExperience(tradition);SpellCastTicket ticket;var request=Request(spell);Assert(session.TryReserve(request,out ticket)==null,"Cast blocked");
                var result=session.Complete(ticket,request,new Effect());Assert(result.Applied && p.Mana==100-spell.Mana && p.Arcane.GetExperience(tradition)==before+8+spell.Circle*2,"Wrong effect or XP");
                if(tradition!=ArcaneTradition.Wizard)Assert(p.Arcane.GetExperience(ArcaneTradition.Wizard)==0,"Cast created Wizard XP");
            }
        });
        check("natural: Fey origin learns Conjuration instead of generic Evocation",()=>{
            var p=new CharacterProgression();p.Arcane.SetOrigin(SorcerousOrigin.Fey);
            var c=new LearningContext{ActorId=42,Eligible=true,Available=true,MagicEligible=true,Tradition=ArcaneTradition.Sorcerer};
            for(int i=0;i<=500;i++){c.WorldTime=i;LearningRules.Tick(p,c);}
            Assert(p.Arcane.KnownSpellIds.Count()==4 && p.Arcane.KnownSpellIds.All(id=>SpellCatalog.Find(id).School!=MagicSchool.Evocation),"Wrong affinity");
        });
        check("natural: formation and knowledge stay owned by each individual",()=>{
            var a=new CharacterProgression();var b=new CharacterProgression();Advance(a,Context(ArcaneTradition.Warlock,1),0,64);
            Advance(b,Context(ArcaneTradition.Sorcerer,2),0,24);Assert(a.Arcane.Origin==SorcerousOrigin.None && b.Arcane.PatronId==null,"Shared formation");
        });
        check("natural: spell learning preserves the first tradition and cannot borrow another circle",()=>{
            var p=new CharacterProgression();p.Arcane.SetPatron("patron");p.Arcane.Study(ArcaneTradition.Wizard,1000);
            var spell=SpellCatalog.Find("ama.spell.necromancy.1.dreno_de_vigor");
            Assert(!LearningRules.Learn(p,spell,KnowledgeSource.Pact,0,42,ArcaneTradition.Warlock),"Borrowed Wizard circle");
            p.Arcane.Study(ArcaneTradition.Warlock,100);Assert(LearningRules.Learn(p,spell,KnowledgeSource.Pact,1,42,ArcaneTradition.Warlock),"Pact rejected");
            Assert(!LearningRules.Learn(p,spell,KnowledgeSource.Player,2,42) && p.Arcane.TraditionFor(spell.Id)==ArcaneTradition.Warlock,"Tradition overwritten");
        });
        check("natural AI: enemy, pause and dead-target rules reject unsafe automatic choices",()=>{
            var p=new CharacterProgression();var spell=SpellCatalog.Find("ama.spell.evocation.0.centelha_rubra");LearningRules.Learn(p,spell,KnowledgeSource.Player,0,42);
            var session=new SpellCastingSession(p);var request=Request(spell);request.Relation=SpellTargetRelation.Ally;
            Assert(SpellAiRules.Choose(p,session,s=>request)==null,"Friendly fire");request.Relation=SpellTargetRelation.Enemy;request.Paused=true;
            Assert(SpellAiRules.Choose(p,session,s=>request)==null,"Paused cast");request.Paused=false;request.TargetAlive=false;
            Assert(SpellAiRules.Choose(p,session,s=>request)==null,"Dead target");
        });
        check("natural AI: unaffordable preferred spells fall back to a valid basic spell",()=>{
            var p=new CharacterProgression();p.Arcane.Study(ArcaneTradition.Wizard,100);
            var basic=SpellCatalog.Find("ama.spell.evocation.0.centelha_rubra");var high=SpellCatalog.Find("ama.spell.evocation.1.lanca_eletrica");
            LearningRules.Learn(p,basic,KnowledgeSource.Player,0,42);LearningRules.Learn(p,high,KnowledgeSource.Player,0,42);p.CastSpell(100-basic.Mana);
            Assert(SpellAiRules.Choose(p,new SpellCastingSession(p),Request,high.Id)==basic,"No mana fallback");
        });
    }
}
