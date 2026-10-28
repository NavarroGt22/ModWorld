using System;
using System.Linq;
using ArcaneMartialAscension.Core;
using ArcaneMartialAscension.Learning;
using ArcaneMartialAscension.Martial;
using ArcaneMartialAscension.Spells;
using Weapon=ArcaneMartialAscension.Core.WeaponType;

internal static class Phase2RulesTests
{
    private const string Spark="ama.spell.evocation.0.centelha_rubra";
    private sealed class World : ISpellEffectWorld
    {
        public int Calls;
        public bool Applied=true,Throws;
        public Action DuringApply;
        public SpellEffectOutcome Apply(SpellEffect effect)
        {Calls++;if(Throws)throw new Exception("native failure");if(DuringApply!=null)DuringApply();return new SpellEffectOutcome{Applied=Applied,HealthLost=Applied?8:0,TargetsAffected=Applied?1:0};}
    }
    private static void Assert(bool condition,string message){if(!condition)throw new Exception(message);}
    private static CharacterProgression Trained(string id=Spark)
    {
        var progress=new CharacterProgression();progress.Arcane.Study(ArcaneTradition.Wizard,100);
        LearningRules.Learn(progress,SpellCatalog.Find(id),KnowledgeSource.Player,0,1);return progress;
    }
    private static SpellCastRequest Request(string id=Spark)
    {return new SpellCastRequest{Spell=SpellCatalog.Find(id),CasterId=1,TargetId=2,CasterAlive=true,TargetAlive=true,HasSchool=true,Relation=SpellTargetRelation.Enemy,WorldTime=10,Distance=2};}
    private static TechniqueEntry Basic(Weapon weapon){return TechniqueCatalog.ForWeapon(weapon)[0];}
    public static void Run(Action<string,Action> check)
    {
        check("phase 2: pending control spells cannot spend mana or start cooldown",()=>{
            const string id="ama.spell.abjuration.0.repulsao_runica";
            var progress=Trained(id);var session=new SpellCastingSession(progress);SpellCastTicket ticket;
            Assert(session.TryReserve(Request(id),out ticket).Code=="pending" && ticket==null && progress.Mana==100,"Fake control effect accepted");
            Assert(session.CooldownRemaining(SpellCatalog.Find(id),10)==0,"Pending cooldown");
        });
        check("phase 2: unlearned magic is rejected without mutation",()=>{
            var progress=new CharacterProgression();var session=new SpellCastingSession(progress);SpellCastTicket ticket;
            Assert(session.TryReserve(Request(),out ticket).Code=="unknown" && progress.Mana==100 && !session.IsBusy,"Unknown spell accepted");
        });
        check("phase 2: reservation locks a caster without spending mana",()=>{
            var progress=Trained();var session=new SpellCastingSession(progress);SpellCastTicket first,second;
            Assert(session.TryReserve(Request(),out first)==null && session.IsBusy && progress.Mana==100,"Reservation spent mana");
            Assert(session.TryReserve(Request(),out second).Code=="busy" && second==null,"Concurrent reservation");
            session.Cancel(first);Assert(!session.IsBusy && progress.Mana==100,"Cancel failed");
        });
        check("phase 2: failed effects cost no mana, XP or cooldown",()=>{
            var progress=Trained();int xp=progress.Arcane.GetExperience(ArcaneTradition.Wizard);
            var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();session.TryReserve(request,out ticket);
            var result=session.Complete(ticket,request,new World{Applied=false});
            Assert(!result.Applied && progress.Mana==100 && progress.Arcane.GetExperience(ArcaneTradition.Wizard)==xp && session.CooldownRemaining(request.Spell,10)==0,"Failed effect charged");
        });
        check("phase 2: landed effect commits exactly one cost and cooldown",()=>{
            var progress=Trained();var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();session.TryReserve(request,out ticket);var world=new World();
            var result=session.Complete(ticket,request,world);
            Assert(result.Applied && result.ManaSpent==3 && progress.Mana==97 && session.CooldownRemaining(request.Spell,11)==1,"Wrong cost/cooldown");
            Assert(!session.Complete(ticket,request,world).Applied && world.Calls==1 && progress.Mana==97,"Ticket replay");
        });
        check("phase 2: cooldown uses simulation time and expires at its exact boundary",()=>{
            var session=new SpellCastingSession(Trained());SpellCastTicket ticket;var request=Request();session.TryReserve(request,out ticket);session.Complete(ticket,request,new World());
            request.WorldTime=11.99;Assert(session.Validate(request).Code=="cooldown","Early cooldown expiry");
            request.WorldTime=12;Assert(session.Validate(request)==null,"Exact expiry rejected");
        });
        check("phase 2: exact mana is accepted and insufficient mana is rejected",()=>{
            var progress=Trained();progress.CastSpell(97);var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();
            Assert(session.TryReserve(request,out ticket)==null,"Exact mana rejected");session.Complete(ticket,request,new World());
            request.WorldTime=20;Assert(progress.Mana==0 && session.Validate(request).Code=="mana","Overspending");
        });
        check("phase 2: target death during flight releases the reservation",()=>{
            var progress=Trained();var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();session.TryReserve(request,out ticket);
            request.TargetAlive=false;var world=new World();var result=session.Complete(ticket,request,world);
            Assert(result.Code=="target" && !session.IsBusy && progress.Mana==100 && world.Calls==0,"Dead target hit");
        });
        check("phase 2: moving out of range cannot land a reserved effect",()=>{
            var progress=Trained();var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();session.TryReserve(request,out ticket);request.Distance=100;
            Assert(session.Complete(ticket,request,new World()).Code=="range" && progress.Mana==100,"Out-of-range hit");
        });
        check("phase 2: pause blocks initiation and resolution without cost",()=>{
            var progress=Trained();var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();request.Paused=true;
            Assert(session.TryReserve(request,out ticket).Code=="paused","Paused initiation");request.Paused=false;session.TryReserve(request,out ticket);request.Paused=true;
            Assert(session.Complete(ticket,request,new World()).Code=="paused" && progress.Mana==100,"Paused resolution");
        });
        check("phase 2: reserved cast identities cannot be substituted",()=>{
            var progress=Trained();var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();session.TryReserve(request,out ticket);request.TargetId=99;
            Assert(session.Complete(ticket,request,new World()).Code=="identity" && progress.Mana==100,"Identity substitution");
        });
        check("phase 2: automatic attacks cannot target allies or the caster",()=>{
            var session=new SpellCastingSession(Trained());var request=Request();request.Relation=SpellTargetRelation.Ally;
            Assert(session.Validate(request).Code=="hostile","Automatic friendly fire");request.Manual=true;Assert(session.Validate(request)==null,"Explicit sandbox targeting changed");
            request.TargetId=request.CasterId;request.Relation=SpellTargetRelation.Self;Assert(session.Validate(request).Code=="hostile","Self damage");
        });
        check("phase 2: beneficial spells require friendly targets",()=>{
            const string id="ama.spell.transmutation.0.pele_de_argila";var session=new SpellCastingSession(Trained(id));var request=Request(id);
            Assert(session.Validate(request).Code=="friendly","Enemy buff accepted");request.Relation=SpellTargetRelation.Self;request.TargetId=request.CasterId;Assert(session.Validate(request)==null,"Self buff rejected");
        });
        check("phase 2: weapon enchantment requires an actual equipped item",()=>{
            const string id="ama.spell.transmutation.1.endurecer_aco";var session=new SpellCastingSession(Trained(id));var request=Request(id);request.Relation=SpellTargetRelation.Ally;
            Assert(session.Validate(request).Code=="weapon","Unarmed enchantment");request.TargetHasWeapon=true;Assert(session.Validate(request)==null,"Weapon enchantment rejected");
        });
        check("phase 2: invalid clocks and positions cannot reach an effect",()=>{
            var session=new SpellCastingSession(Trained());var request=Request();
            foreach(double time in new[]{double.NaN,double.PositiveInfinity,-1d}){request.WorldTime=time;Assert(session.Validate(request).Code=="time_position","Invalid time");}
            request.WorldTime=10;request.Distance=float.NaN;Assert(session.Validate(request).Code=="time_position","Invalid position");
        });
        check("phase 2: missing school is rejected and learned circle 1 remains usable",()=>{
            var session=new SpellCastingSession(Trained());var request=Request();request.HasSchool=false;Assert(session.Validate(request).Code=="school","Missing school accepted");
            var low=new CharacterProgression();var advanced=SpellCatalog.Find("ama.spell.evocation.1.lanca_eletrica");
            low.Arcane.Study(ArcaneTradition.Wizard,100);LearningRules.Learn(low,advanced,KnowledgeSource.Player,0,1);
            Assert(new SpellCastingSession(low).Validate(Request(advanced.Id))==null,"Learned circle 1 rejected");
        });
        check("phase 2: reentrant completion cannot apply or charge twice",()=>{
            var progress=Trained();var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();session.TryReserve(request,out ticket);var world=new World();
            world.DuringApply=()=>Assert(session.Complete(ticket,request,new World()).Code=="ticket","Reentrant cast");
            session.Complete(ticket,request,world);Assert(world.Calls==1 && progress.Mana==97,"Reentrant cost");
        });
        check("phase 2: effect exceptions release the reservation without charging",()=>{
            var progress=Trained();var session=new SpellCastingSession(progress);SpellCastTicket ticket;var request=Request();session.TryReserve(request,out ticket);
            bool threw=false;try{session.Complete(ticket,request,new World{Throws=true});}catch{threw=true;}
            Assert(threw && !session.IsBusy && progress.Mana==100,"Exception stranded reservation");
        });
        check("phase 2: spell availability comes from explicit profiles and no untested spell is labelled tested",()=>{
            var active=SpellCatalog.All.Where(SpellCatalog.HasRuntime).ToArray();
            Assert(active.Length==11 && active.All(s=>s.Circle<=1),"Unexpected available catalog");
            Assert(SpellCatalog.All.All(s=>SpellRuntimeCatalog.State(s)!=SpellDevelopmentState.Tested),"Untested spell labelled tested");
            Assert(!new SpellEffect(SpellEffectKind.Summon).IsSupported && !new SpellEffect(SpellEffectKind.Teleport).IsSupported && !new SpellEffect(SpellEffectKind.Shield).IsSupported,"Unimplemented adapter enabled");
        });
        check("phase 2: mastery alone and inspection do not grant techniques",()=>{
            var progress=new CharacterProgression();progress.TrainWeapon(Weapon.Sword,2500);new ArcaneOverview(progress);
            Assert(progress.Techniques.Get(Basic(Weapon.Sword).Id)==null,"Technique granted on inspection/level alone");
        });
        check("phase 2: actual training learns a weapon's basic technique with its origin",()=>{
            var progress=new CharacterProgression();
            for(int i=0;i<15;i++)LearningRules.AwardCombat(progress,Weapon.Spear,i,i,42);
            var basic=Basic(Weapon.Spear);Assert(progress.Techniques.Get(basic.Id).Level==1 && progress.GetOrigin(basic.Id).Source==KnowledgeSource.Combat,"Missing learned technique/origin");
            Assert(progress.Techniques.Get(Basic(Weapon.Sword).Id)==null,"Wrong weapon learned");
        });
        check("phase 2: planned techniques stay unavailable even at mastery 7",()=>{
            var progress=new CharacterProgression();progress.TrainWeapon(Weapon.Axe,2500);
            Assert(TechniqueRules.LearnFromTraining(progress,Weapon.Axe,KnowledgeSource.Player,1,1)==1,"Wrong learned count");
            Assert(TechniqueCatalog.ForWeapon(Weapon.Axe).Skip(1).All(t=>progress.Techniques.Get(t.Id)==null && !TechniqueRules.CanUse(progress,t,Weapon.Axe,100)),"Planned technique unlocked");
        });
        check("phase 2: technique ranks need confirmed uses and remain separate from weapon rank",()=>{
            var progress=new CharacterProgression();progress.TrainWeapon(Weapon.Sword,2500);TechniqueRules.LearnFromTraining(progress,Weapon.Sword,KnowledgeSource.Player,1,1);var entry=Basic(Weapon.Sword);
            Assert(progress.Techniques.Get(entry.Id).Rank=="I","Weapon level granted technique rank");
            Assert(!TechniqueRules.ConfirmHit(progress,entry,Weapon.Sword,10,0) && progress.Techniques.Get(entry.Id).SuccessfulUses==0,"Failed use trained technique");
            for(int i=0;i<5;i++)Assert(TechniqueRules.ConfirmHit(progress,entry,Weapon.Sword,10+i*10,1),"Technique use rejected");
            Assert(progress.Techniques.Get(entry.Id).Rank=="II","Rank II missing");
            for(int i=5;i<30;i++)TechniqueRules.ConfirmHit(progress,entry,Weapon.Sword,10+i*10,1);
            Assert(progress.Techniques.Get(entry.Id).Rank=="III" && progress.Techniques.Get(entry.Id).SuccessfulUses==20,"Technique cap");
        });
        check("phase 2: techniques check equipment and their own cooldown",()=>{
            var progress=new CharacterProgression();progress.TrainWeapon(Weapon.Bow,30);TechniqueRules.LearnFromTraining(progress,Weapon.Bow,KnowledgeSource.Practice,1,1);var entry=Basic(Weapon.Bow);
            Assert(!TechniqueRules.CanUse(progress,entry,null,10) && !TechniqueRules.CanUse(progress,entry,Weapon.Axe,10),"Wrong gear accepted");
            TechniqueRules.ConfirmHit(progress,entry,Weapon.Bow,10,1);Assert(!TechniqueRules.CanUse(progress,entry,Weapon.Bow,12.99) && TechniqueRules.CanUse(progress,entry,Weapon.Bow,13),"Cooldown boundary");
        });
        check("phase 2: technique progression is independent per character",()=>{
            var first=new CharacterProgression();var second=new CharacterProgression();first.TrainWeapon(Weapon.Axe,30);second.TrainWeapon(Weapon.Axe,30);
            TechniqueRules.LearnFromTraining(first,Weapon.Axe,KnowledgeSource.Practice,1,1);
            Assert(second.Techniques.Get(Basic(Weapon.Axe).Id)==null,"Knowledge leaked to another actor");
        });
        check("phase 2: invalid technique times cannot grant knowledge or uses",()=>{
            var progress=new CharacterProgression();progress.TrainWeapon(Weapon.Sword,30);var entry=Basic(Weapon.Sword);
            Assert(TechniqueRules.LearnFromTraining(progress,Weapon.Sword,KnowledgeSource.Player,double.NaN,1)==0,"Invalid learning time");
            Assert(TechniqueRules.LearnFromTraining(progress,(Weapon)99,KnowledgeSource.Player,1,1)==0,"Invalid weapon");
            TechniqueRules.LearnFromTraining(progress,Weapon.Sword,KnowledgeSource.Player,0,1);
            Assert(!TechniqueRules.CanUse(progress,entry,Weapon.Sword,-1) && !TechniqueRules.ConfirmHit(progress,entry,Weapon.Sword,double.PositiveInfinity,1),"Invalid use time");
        });
        check("phase 2: manual weapon XP cannot overflow into a negative level",()=>{
            var progress=new CharacterProgression();progress.TrainWeapon(Weapon.Axe,100);progress.TrainWeapon(Weapon.Axe,int.MaxValue);
            Assert(progress.GetWeaponLevel(Weapon.Axe)==7 && progress.GetWeaponExperience(Weapon.Axe)==1000000,"XP integer overflow");
        });
    }
}
